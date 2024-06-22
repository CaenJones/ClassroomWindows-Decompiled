
using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Net;
using System.Threading;

#nullable disable
namespace ClassroomWindows
{
  public sealed class IpApi
  {
    private const int ERROR_IO_PENDING = 997;
    private const int changeEvent = 0;
    private const int stopEvent = 1;
    private const int waitSeconds = 5;
    private Thread thread;
    private AutoResetEvent[] events = new AutoResetEvent[2]
    {
      new AutoResetEvent(false),
      new AutoResetEvent(false)
    };
    private static IpApi instance = (IpApi) null;
    private static readonly object LOCK = new object();
    private string _ipAddress;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public string IpAddress => this._ipAddress ?? "";

    public bool OnCampus { get; private set; } = true;

    public static IpApi Instance
    {
      get
      {
        lock (IpApi.LOCK)
        {
          if (IpApi.instance == null)
          {
            try
            {
              IpApi.instance = new IpApi();
            }
            catch (Exception ex)
            {
              throw new Exception("Failed trying to initialize IpApi().", ex);
            }
          }
          return IpApi.instance;
        }
      }
    }

    public bool Get()
    {
      string str = ConfigManager.Instance.Config.policyURL + "/ip";
      IpApi.logger.Debug("Request URL " + str);
      RestClient restClient = new RestClient(str);
      RestRequest restRequest1 = new RestRequest((Method) 0);
      restRequest1.AddHeader("Cache-Control", "no-cache");
      restRequest1.AddHeader("customerID", ConfigManager.Instance.Config.customerID);
      restRequest1.AddHeader("version", Global.versionHeader);
      restRequest1.AddHeader("x-api-key", ConfigManager.Instance.Config.apiKey);
      restRequest1.AddHeader("Content-Type", "application/json");
      RestRequest restRequest2 = restRequest1;
      IRestResponse irestResponse = restClient.Execute((IRestRequest) restRequest2);
      if (!irestResponse.IsSuccessful || irestResponse.StatusCode != HttpStatusCode.OK)
      {
        IpApi.logger.Warn("Unable get external IP address, " + Global.HttpStatusText(irestResponse.StatusCode));
        return false;
      }
      string content = irestResponse.Content;
      try
      {
        this._ipAddress = JsonConvert.DeserializeObject<IpApi.Result>(content).ip;
      }
      catch (JsonSerializationException ex)
      {
        this._ipAddress = (string) null;
        IpApi.logger.Debug(string.Format("Invalid format detected at line {0} ", (object) ex.LineNumber) + string.Format("position {0}", (object) ex.LinePosition));
      }
      this.SetOnCampus();
      return true;
    }

    public void SetOnCampus()
    {
      if (string.IsNullOrEmpty(this._ipAddress))
        return;
      bool flag = StudentPolicyApi.Instance.IsOnCampus(this._ipAddress);
      if (this.OnCampus == flag)
        return;
      IpApi.logger.Info(this.IpAddress + " is " + (flag ? "on" : "off") + " campus");
      this.OnCampus = flag;
    }

    public void ThreadFunction()
    {
      IpApi.logger.Debug(this.thread.Name + " started");
      try
      {
        NativeOverlapped overlapped = new NativeOverlapped();
        IntPtr zero = IntPtr.Zero;
        overlapped.EventHandle = this.events[0].SafeWaitHandle.DangerousGetHandle();
        uint num;
        while (true)
        {
          num = Win32.NotifyAddrChange(ref zero, ref overlapped);
          if (num == 997U)
          {
            if (WaitHandle.WaitAny((WaitHandle[]) this.events) == 0)
            {
              IpApi.logger.Debug(string.Format("Waiting {0} seconds before ", (object) 5) + "getting external IP ...");
              if (!this.events[1].WaitOne(5000))
              {
                this.Get();
                IpApi.logger.Debug("Network change notification; external IP = " + IpApi.Instance.IpAddress);
                AblyConnectionManager.Instance.UpdatePresence();
              }
              else
                goto label_8;
            }
            else
              break;
          }
          else
            goto label_6;
        }
        return;
label_8:
        return;
label_6:
        IpApi.logger.Warn(string.Format("NotifyAddrChange error = {0}", (object) num));
      }
      catch (Exception ex)
      {
        IpApi.logger.Error<Exception>(ex);
      }
    }

    public void Stop()
    {
      if (this.thread == null || !this.thread.IsAlive)
        return;
      this.events[1].Set();
      this.thread.Join();
    }

    ~IpApi() => this.Stop();

    private IpApi()
    {
      this._ipAddress = (string) null;
      this.thread = new Thread(new ThreadStart(this.ThreadFunction))
      {
        Name = "NotifyAddrChangeThread"
      };
      this.thread.Start();
    }

    private class Result
    {
      public string ip;
    }
  }
}
