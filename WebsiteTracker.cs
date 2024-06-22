
using NLog;
using System;
using System.Collections.Generic;

#nullable disable
namespace ClassroomWindows
{
  public sealed class WebsiteTracker
  {
    public Dictionary<string, ulong> reportUrlsVisited = new Dictionary<string, ulong>();
    private HashSet<string> urlsVisited = new HashSet<string>();
    private Dictionary<string, double> viewedHostsTimes = new Dictionary<string, double>();
    private string activeHost = "";
    private string prevURL = "";
    private string mostViewed = "";
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public int Visits => this.urlsVisited.Count;

    public double MostViewedTime
    {
      get
      {
        return !string.IsNullOrEmpty(this.mostViewed) && this.viewedHostsTimes.ContainsKey(this.mostViewed) ? this.viewedHostsTimes[this.mostViewed] : 0.0;
      }
    }

    public string MostViewed => this.mostViewed;

    public void Track(string groupGuid, string url, double deltaTime)
    {
      if (string.IsNullOrEmpty(url))
        return;
      if (!url.Equals(this.prevURL))
      {
        this.prevURL = url;
        this.ReportUrlVisited(url, groupGuid);
      }
      if (this.urlsVisited.Add(url))
        WebsiteTracker.logger.Debug(Group.FormatGuid(groupGuid) + ": URL = \"" + url + "\"" + string.Format(" added to visited, count = {0}", (object) this.Visits));
      string host = new Uri(url).Host;
      if (string.IsNullOrEmpty(host))
      {
        WebsiteTracker.logger.Trace("Ignoring blank host for URL = \"" + url + "\"");
      }
      else
      {
        if (host.Equals(this.activeHost) && this.viewedHostsTimes.ContainsKey(host))
        {
          double num = this.viewedHostsTimes[host] += deltaTime;
          WebsiteTracker.logger.Trace(Group.FormatGuid(groupGuid) + ": viewed time for " + string.Format("\"{0}\" = {1}", (object) host, (object) num));
        }
        else if (this.viewedHostsTimes.ContainsKey(host))
        {
          this.activeHost = host;
          this.viewedHostsTimes[host] += deltaTime;
          WebsiteTracker.logger.Trace(Group.FormatGuid(groupGuid) + ": active host = \"" + this.activeHost + "\"");
        }
        else
        {
          this.activeHost = host;
          this.viewedHostsTimes[host] = 0.0;
          WebsiteTracker.logger.Trace(Group.FormatGuid(groupGuid) + ": active host = \"" + this.activeHost + "\"");
        }
        if (!this.viewedHostsTimes.ContainsKey(host))
          WebsiteTracker.logger.Debug(Group.FormatGuid(groupGuid) + ": Host \"" + host + "\" has no viewed time");
        if (string.IsNullOrEmpty(this.mostViewed))
        {
          this.mostViewed = host;
          WebsiteTracker.logger.Debug(Group.FormatGuid(groupGuid) + ": initial most viewed = \"" + this.mostViewed + "\"");
        }
        else if (this.viewedHostsTimes.ContainsKey(host) && this.viewedHostsTimes.ContainsKey(this.mostViewed))
        {
          double viewedHostsTime1 = this.viewedHostsTimes[host];
          double viewedHostsTime2 = this.viewedHostsTimes[this.mostViewed];
          if (viewedHostsTime1 > viewedHostsTime2)
          {
            if (this.mostViewed.Equals(host))
              return;
            WebsiteTracker.logger.Debug(Group.FormatGuid(groupGuid) + ": most viewed changing from \"" + this.mostViewed + "\" to = \"" + host + "\"");
            this.mostViewed = host;
          }
          else
            WebsiteTracker.logger.Trace(string.Format("{0}: most viewed = \"{1}\" elapsed = {2}, active host \"{3}\" elapsed = {4}", (object) Group.FormatGuid(groupGuid), (object) this.mostViewed, (object) viewedHostsTime2, (object) host, (object) viewedHostsTime1));
        }
        else
        {
          if (this.viewedHostsTimes.ContainsKey(this.mostViewed))
            return;
          WebsiteTracker.logger.Debug(Group.FormatGuid(groupGuid) + ": most viewed \"" + this.mostViewed + "\" not found; setting to active host \"" + host + "\"");
          this.mostViewed = host;
        }
      }
    }

    private void ReportUrlVisited(string url, string groupGuid)
    {
      if (this.reportUrlsVisited.ContainsKey(url))
        return;
      bool flag = false;
      try
      {
        this.reportUrlsVisited.Add(url, Global.GetUnixTimeStamp());
        flag = true;
      }
      catch (ArgumentException ex)
      {
      }
      if (!flag)
        return;
      WebsiteTracker.logger.Debug(Group.FormatGuid(groupGuid) + ": URL = \"" + url + "\" " + string.Format("added to visited report, count = {0}", (object) this.reportUrlsVisited.Count));
    }

    public void ResetUrlsVisited(string groupGuid)
    {
      switch (this.Visits)
      {
        case 0:
          WebsiteTracker.logger.Debug(Group.FormatGuid(groupGuid) + ": no URLs visited");
          return;
        case 1:
          string[] array = new string[1];
          this.urlsVisited.CopyTo(array, 0);
          WebsiteTracker.logger.Debug(Group.FormatGuid(groupGuid) + ": Clearing visited URL = \"" + array[0] + "\"");
          break;
        default:
          WebsiteTracker.logger.Debug(string.Format("{0}: Clearing {1} visited URLs:", (object) Group.FormatGuid(groupGuid), (object) this.Visits));
          using (HashSet<string>.Enumerator enumerator = this.urlsVisited.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              string current = enumerator.Current;
              WebsiteTracker.logger.Debug("    \"" + current + "\"");
            }
            break;
          }
      }
      this.urlsVisited.Clear();
      this.viewedHostsTimes.Clear();
    }
  }
}
