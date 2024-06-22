
using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

#nullable disable
namespace ClassroomWindows
{
  public sealed class StudentPolicyApi
  {
    private static StudentPolicyApi instance = (StudentPolicyApi) null;
    private static readonly object LOCK = new object();
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private StudentPolicy _studentPolicy;
    private Dictionary<string, Group> _Groups = new Dictionary<string, Group>();
    private List<IPNetworkAddress> campus_networks = new List<IPNetworkAddress>();

    public StudentPolicy StudentPolicy => this._studentPolicy;

    public Dictionary<string, Group> Groups => this._Groups;

    public static StudentPolicyApi Instance
    {
      get
      {
        lock (StudentPolicyApi.LOCK)
        {
          if (StudentPolicyApi.instance == null)
          {
            try
            {
              StudentPolicyApi.instance = new StudentPolicyApi();
            }
            catch (Exception ex)
            {
              throw new Exception("Failed trying to initialize StudentPolicyApi().", ex);
            }
          }
          return StudentPolicyApi.instance;
        }
      }
    }

    private StudentPolicyApi()
    {
    }

    private static string JsonString(ref string delimiter, string name, string value)
    {
      if (string.IsNullOrEmpty(value))
        return string.Empty;
      string str = delimiter + "\"" + name + "\":" + JsonConvert.ToString(value);
      delimiter = ",";
      return str;
    }

    public StudentPolicyApi.Result Init()
    {
      string empty = string.Empty;
      string str1;
      if (ConfigManager.Instance.Config.useLogonID)
      {
        str1 = StudentPolicyApi.JsonString(ref empty, "machine_name", Environment.MachineName) + StudentPolicyApi.JsonString(ref empty, "domain_name", Environment.UserDomainName) + StudentPolicyApi.JsonString(ref empty, "user_name", Environment.UserName) + StudentPolicyApi.JsonString(ref empty, "msa_email", UserInfo.msaEmail) + StudentPolicyApi.JsonString(ref empty, "ad_email", UserInfo.adEmail) + StudentPolicyApi.JsonString(ref empty, "ad_upn", UserInfo.adUpn);
      }
      else
      {
        StudentPolicyApi.logger.Debug("Using configuration user ID = " + ConfigManager.Instance.Config.userID);
        if (ConfigManager.Instance.Config.userID.Contains("@"))
        {
          str1 = StudentPolicyApi.JsonString(ref empty, "machine_name", Environment.MachineName) + StudentPolicyApi.JsonString(ref empty, "domain_name", Environment.UserDomainName) + StudentPolicyApi.JsonString(ref empty, "user_name", Environment.UserName) + StudentPolicyApi.JsonString(ref empty, "msa_email", ConfigManager.Instance.Config.userID);
        }
        else
        {
          string userId = ConfigManager.Instance.Config.userID;
          int length = userId.LastIndexOf('\\');
          str1 = length < 0 ? StudentPolicyApi.JsonString(ref empty, "machine_name", Environment.MachineName) + StudentPolicyApi.JsonString(ref empty, "user_name", userId) : StudentPolicyApi.JsonString(ref empty, "machine_name", Environment.MachineName) + StudentPolicyApi.JsonString(ref empty, "domain_name", userId.Substring(0, length)) + StudentPolicyApi.JsonString(ref empty, "user_name", userId.Substring(length + 1));
        }
      }
      string str2 = ConfigManager.Instance.Config.policyURL + "/policy";
      StudentPolicyApi.logger.Debug("Policy request URL " + str2);
      string str3 = "{" + str1 + "}";
      StudentPolicyApi.logger.Debug("Policy request body = " + str3);
      RestClient restClient = new RestClient(str2);
      RestRequest restRequest1 = new RestRequest((Method) 1);
      restRequest1.AddHeader("Cache-Control", "no-cache");
      restRequest1.AddHeader("customerID", ConfigManager.Instance.Config.customerID);
      restRequest1.AddHeader("version", Global.versionHeader);
      restRequest1.AddHeader("x-api-key", ConfigManager.Instance.Config.apiKey);
      restRequest1.AddHeader("Content-Type", "application/json");
      restRequest1.AddParameter("undefined", (object) str3, (ParameterType) 4);
      RestRequest restRequest2 = restRequest1;
      IRestResponse irestResponse = restClient.Execute((IRestRequest) restRequest2);
      if (!irestResponse.IsSuccessful)
      {
        StudentPolicyApi.logger.Warn("Policy API failed, " + Global.HttpStatusText(irestResponse.StatusCode));
        return StudentPolicyApi.Result.NotConnected;
      }
      StudentPolicyApi.logger.Debug("Policy API completed with " + Global.HttpStatusText(irestResponse.StatusCode));
      if (irestResponse.StatusCode != HttpStatusCode.OK)
        return StudentPolicyApi.Result.Error;
      string content = irestResponse.Content;
      try
      {
        this._studentPolicy = JsonConvert.DeserializeObject<StudentPolicy>(content);
      }
      catch (JsonReaderException ex)
      {
        StudentPolicyApi.logger.Debug(((Exception) ex).Message);
      }
      catch (JsonSerializationException ex)
      {
        StudentPolicyApi.logger.Debug(((Exception) ex).Message);
        return StudentPolicyApi.Result.Error;
      }
      StudentPolicyApi.logger.Debug("Parsed Policy:" + Global.newLine + "user_guid: " + this._studentPolicy.user_guid + Global.newLine + "first_name: " + this._studentPolicy.first_name + Global.newLine + "last_name: " + this._studentPolicy.last_name + Global.newLine + "email: " + this._studentPolicy.email + Global.newLine + "username: " + this._studentPolicy.username + Global.newLine + "user_type: " + (this._studentPolicy.user_type == null ? "none" : this._studentPolicy.user_type));
      if (string.IsNullOrEmpty(this._studentPolicy.email))
      {
        StudentPolicyApi.logger.Warn("Policy email is blank");
        return StudentPolicyApi.Result.Error;
      }
      if (this._studentPolicy == null)
        return StudentPolicyApi.Result.Error;
      this.ProcessGroups();
      this.ProcessCampusNetworks();
      IpApi.Instance.SetOnCampus();
      return StudentPolicyApi.Result.Success;
    }

    private string Waiting(int interval)
    {
      return string.Format("Waiting {0} minute{1}", (object) interval, interval == 1 ? (object) "" : (object) "s");
    }

    public void InitWithRetry()
    {
      int[] numArray = new int[8]
      {
        1,
        3,
        5,
        10,
        15,
        30,
        45,
        60
      };
      int num = numArray.GetLength(0) - 1;
      int index = 0;
      while (true)
      {
        int interval;
        switch (this.Init())
        {
          case StudentPolicyApi.Result.Success:
            goto label_2;
          case StudentPolicyApi.Result.NotConnected:
            index = 0;
            interval = 3;
            StudentPolicyApi.logger.Debug(this.Waiting(interval) + " to connect to the Internet ...");
            break;
          default:
            interval = numArray[index];
            if (index < num)
              ++index;
            StudentPolicyApi.logger.Debug(this.Waiting(interval) + " before getting the Student Policy ...");
            break;
        }
        Thread.Sleep(interval * 60 * 1000);
      }
label_2:;
    }

    public bool IsSiteBlocked(
      string url,
      PresenceState presenceState,
      out string encrypted_customer_id,
      out string group_guid)
    {
      group_guid = (string) null;
      encrypted_customer_id = this._studentPolicy.district.encrypted_customer_id;
      if (!IpApi.Instance.OnCampus || string.IsNullOrEmpty(url) || ConfigManager.Instance.IsBlockPage(url))
        return false;
      Uri uri = new Uri(url);
      if (presenceState == PresenceState.Done)
      {
        foreach (KeyValuePair<string, Group> group1 in this._Groups)
        {
          Group group2 = group1.Value;
          if (group2.IsActive() && group2.AllowWhenDone)
          {
            StudentPolicyApi.logger.Debug(uri.Host + uri.AbsolutePath + " allowed for group \"" + group2.Name + "\" (allow for \"done\" students)");
            return false;
          }
        }
      }
      foreach (KeyValuePair<string, Group> group3 in this._Groups)
      {
        Group group4 = group3.Value;
        if (group4.IsActive())
        {
          if (!group4.HaveZoneList)
          {
            StudentPolicyApi.logger.Debug(uri.Host + uri.AbsolutePath + " allowed for group \"" + group4.Name + "\" (No zone list)");
            return false;
          }
          if (group4.DefaultInternet)
          {
            StudentPolicyApi.logger.Debug(uri.Host + uri.AbsolutePath + " allowed for group \"" + group4.Name + "\" (Default Internet)");
            return false;
          }
          if (group4.IsSiteAllowed(uri))
          {
            StudentPolicyApi.logger.Debug(uri.Host + uri.AbsolutePath + " allowed for group \"" + group4.Name + "\" (Custom Internet)");
            return false;
          }
        }
      }
      foreach (KeyValuePair<string, Group> group5 in this._Groups)
      {
        Group group6 = group5.Value;
        if (group6.IsActive())
        {
          if (group6.InternetOff)
          {
            StudentPolicyApi.logger.Debug(uri.Host + uri.AbsolutePath + " blocked for group \"" + group6.Name + "\" (Internet Off)");
            group_guid = group6.Guid;
            return true;
          }
          if (group6.IsSiteBlocked(uri))
          {
            StudentPolicyApi.logger.Debug(uri.Host + uri.AbsolutePath + " blocked for group \"" + group6.Name + "\" (Custom Internet)");
            group_guid = group6.Guid;
            return true;
          }
        }
      }
      StudentPolicyApi.logger.Debug(uri.Host + uri.AbsolutePath + " allowed by default");
      return false;
    }

    public bool IsOnCampus(string ipAddressText)
    {
      if (this.campus_networks.Count == 0)
        return true;
      IPAddress address;
      if (IPAddress.TryParse(ipAddressText, out address))
      {
        foreach (IPNetworkAddress campusNetwork in this.campus_networks)
        {
          if (campusNetwork.IsInNetwork(address))
          {
            StudentPolicyApi.logger.Debug(string.Format("{0} belongs to ", (object) address) + string.Format("campus network {0}", (object) campusNetwork));
            return true;
          }
        }
      }
      else
        StudentPolicyApi.logger.Warn("Invalid IP address format \"" + ipAddressText + "\"");
      return false;
    }

    public void Update(double deltaTime)
    {
      if (!IpApi.Instance.OnCampus)
        return;
      foreach (KeyValuePair<string, Group> group1 in StudentPolicyApi.Instance.Groups)
      {
        Group group2 = group1.Value;
        string currentlyBrowsingUrl = WebBrowsersManagerModule.Instance.GetCurrentlyBrowsingUrl();
        group2.Update(currentlyBrowsingUrl, deltaTime);
        if (group2.IsActive())
          AblyConnectionManager.Instance.PublishUpdatesForGroup(group2, false);
      }
    }

    private void ProcessCampusNetworks()
    {
      if (this._studentPolicy.district == null || this._studentPolicy.district.campus_networks == null)
        return;
      foreach (string campusNetwork in this._studentPolicy.district.campus_networks)
      {
        if (!string.IsNullOrEmpty(campusNetwork))
        {
          string ipAddressText = campusNetwork;
          if (ipAddressText.IndexOf('/') == -1)
            ipAddressText += "/32";
          IPNetworkAddress networkAddress;
          if (IPNetworkAddress.TryParse(ipAddressText, out networkAddress))
          {
            StudentPolicyApi.logger.Info("Campus network " + ipAddressText + " added");
            this.campus_networks.Add(networkAddress);
          }
          else
            StudentPolicyApi.logger.Warn("Ignoring invalid campus network " + ipAddressText);
        }
      }
    }

    private static bool HasGuid(PolicyGroup group)
    {
      if (group.group_guid != null)
        return true;
      StudentPolicyApi.logger.Debug("\"" + group.name + "\" group missing GUID; ignoring");
      return false;
    }

    public bool GetGroup(string guid, out Group group)
    {
      if (this.Groups.TryGetValue(guid, out group))
        return true;
      StudentPolicyApi.logger.Warn(Group.FormatGuid(guid) + ": not found");
      return false;
    }

    private void ProcessGroups()
    {
      if (this._studentPolicy.groups == null)
      {
        StudentPolicyApi.logger.Debug("No policy groups");
        this._Groups.Clear();
      }
      else
      {
        this._studentPolicy.groups = Array.FindAll<PolicyGroup>(this._studentPolicy.groups, new Predicate<PolicyGroup>(StudentPolicyApi.HasGuid));
        List<string> stringList = new List<string>();
        foreach (KeyValuePair<string, Group> group1 in this._Groups)
        {
          Group group2 = group1.Value;
          if (!this._studentPolicy.IsGroupExists(group2.Guid))
            stringList.Add(group2.Guid);
        }
        foreach (string str in stringList)
        {
          this._Groups.Remove(str);
          StudentPolicyApi.logger.Debug(Group.FormatGuid(str) + ", removed");
        }
        foreach (PolicyGroup group3 in this._studentPolicy.groups)
        {
          Group group4;
          if (this._Groups.TryGetValue(group3.group_guid, out group4))
          {
            this._Groups[group3.group_guid] = new Group(group3, group4.WebsiteTracker);
            StudentPolicyApi.logger.Debug(Group.FormatGuid(group4.Guid) + ", updated");
          }
          else if (!string.IsNullOrEmpty(group3.name))
          {
            this._Groups[group3.group_guid] = new Group(group3, new WebsiteTracker());
            StudentPolicyApi.logger.Debug(Group.FormatGuid(group3.group_guid) + ", created");
          }
        }
      }
    }

    public void ReportUrlsVisited()
    {
      string str1 = "";
      string str2 = "";
      int num = 0;
      if (!IpApi.Instance.OnCampus)
        return;
      foreach (KeyValuePair<string, Group> group1 in this._Groups)
      {
        Group group2 = group1.Value;
        Dictionary<string, ulong> reportUrlsVisited = group2.WebsiteTracker.reportUrlsVisited;
        if (group2.IsActive())
        {
          foreach (KeyValuePair<string, ulong> keyValuePair in reportUrlsVisited)
          {
            StudentPolicyApi.logger.Debug("Reporting URL \"" + keyValuePair.Key + "\"");
            str1 = str1 + str2 + "{\"gguid\":\"" + group2.Guid + "\",\"url\":\"" + keyValuePair.Key + "\"," + string.Format("\"at\":{0}", (object) keyValuePair.Value) + "}";
            str2 = ",";
            ++num;
          }
        }
        reportUrlsVisited.Clear();
      }
      if (num <= 0)
        return;
      string str3 = "{\"report\":[" + str1 + "],\"user_info\":{\"username\":\"" + this._studentPolicy.username + "\",\"email\":\"" + this._studentPolicy.email + "\",\"guid\":\"" + this._studentPolicy.user_guid + "\"}}";
      string str4 = ConfigManager.Instance.Config.policyURL + "/report";
      StudentPolicyApi.logger.Debug("Report request URL " + str4);
      StudentPolicyApi.logger.Debug("Report request body = " + str3);
      RestClient restClient = new RestClient(str4);
      RestRequest restRequest1 = new RestRequest((Method) 1);
      restRequest1.AddHeader("Cache-Control", "no-cache");
      restRequest1.AddHeader("customerID", ConfigManager.Instance.Config.customerID);
      restRequest1.AddHeader("version", Global.versionHeader);
      restRequest1.AddHeader("x-api-key", ConfigManager.Instance.Config.apiKey);
      restRequest1.AddHeader("Content-Type", "application/json");
      restRequest1.AddParameter("undefined", (object) str3, (ParameterType) 4);
      RestRequest restRequest2 = restRequest1;
      IRestResponse irestResponse = restClient.Execute((IRestRequest) restRequest2);
      if (!irestResponse.IsSuccessful)
        StudentPolicyApi.logger.Warn("Report API failed, " + Global.HttpStatusText(irestResponse.StatusCode));
      else if (irestResponse.StatusCode != HttpStatusCode.OK)
        StudentPolicyApi.logger.Warn("Report API failed with " + Global.HttpStatusText(irestResponse.StatusCode));
      else
        StudentPolicyApi.logger.Debug("Report response: " + irestResponse.Content);
    }

    public enum Result
    {
      Success,
      Error,
      NotConnected,
    }
  }
}
