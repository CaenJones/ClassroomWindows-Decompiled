// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.Group
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using NLog;
using System;
using System.Collections.Generic;

#nullable disable
namespace ClassroomWindows
{
  public sealed class Group
  {
    private DateTime urlsVisitedResetTime = new DateTime(DateTime.MaxValue.Ticks);
    private DayOfWeek dayOfWeek;
    private PolicyGroup policyGroup;
    private List<Site> zoneList = new List<Site>();
    private bool _allowWhenDone;
    private bool zoneListBlocked;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public WebsiteTracker WebsiteTracker { get; set; }

    public string EncryptedUserEmail { get; set; }

    public string Name => this.policyGroup.name;

    public string Guid => this.policyGroup.group_guid;

    public string Info => "\"" + this.Name + "\", " + Group.FormatGuid(this.Guid);

    public bool AllowWhenDone => this._allowWhenDone;

    public bool DefaultInternet => this.policyGroup.web_rules == null;

    public bool InternetOff { get; set; }

    public bool HaveZoneList { get; set; }

    public Group(PolicyGroup policyGroup, WebsiteTracker websiteTracker)
    {
      this.policyGroup = policyGroup;
      this.WebsiteTracker = websiteTracker;
      this.ParsePolicyGroup();
      this.dayOfWeek = DateTime.Now.DayOfWeek;
      this.SetUrlsVisitedResetTime();
    }

    public static string FormatGuid(string guid) => "Group " + guid;

    public void Update(string url, double deltaTime)
    {
      if (DateTime.Compare(DateTime.Now, this.urlsVisitedResetTime) >= 0)
      {
        this.WebsiteTracker.ResetUrlsVisited(this.Guid);
        if (this.dayOfWeek == DateTime.Now.DayOfWeek)
          this.dayOfWeek = this.GetNextDayOfWeek(this.dayOfWeek);
        this.SetUrlsVisitedResetTime();
      }
      if (!this.IsActive())
        return;
      this.WebsiteTracker.Track(this.Guid, url, deltaTime);
    }

    public bool IsSiteAllowed(Uri uri)
    {
      if (this.zoneListBlocked)
      {
        foreach (Site zone in this.zoneList)
        {
          if (zone.Match(uri))
            return false;
        }
        return true;
      }
      foreach (Site zone in this.zoneList)
      {
        if (zone.Match(uri))
          return true;
      }
      return false;
    }

    public bool IsSiteBlocked(Uri uri)
    {
      if (this.zoneListBlocked)
      {
        foreach (Site zone in this.zoneList)
        {
          if (zone.Match(uri))
            return true;
        }
        return false;
      }
      foreach (Site zone in this.zoneList)
      {
        if (zone.Match(uri))
          return false;
      }
      return true;
    }

    public bool IsActive()
    {
      if (this.policyGroup != null && this.policyGroup.class_schedule != null)
      {
        DateTime now = DateTime.Now;
        int relativeTime = Group.DaySchedule.GetRelativeTime(now.Hour, now.Minute);
        string str = ((int) now.DayOfWeek).ToString();
        foreach (KeyValuePair<string, Schedule> keyValuePair in this.policyGroup.class_schedule)
        {
          if (!string.IsNullOrEmpty(keyValuePair.Key) && keyValuePair.Key.Contains(str))
          {
            Schedule schedule = keyValuePair.Value;
            if (schedule != null && schedule.HasValues && new Group.DaySchedule(schedule).InSchedule(relativeTime))
              return true;
          }
        }
      }
      return false;
    }

    private DayOfWeek GetNextDayOfWeek(DayOfWeek dayOfWeek)
    {
      return dayOfWeek == DayOfWeek.Saturday ? DayOfWeek.Sunday : dayOfWeek + 1;
    }

    public void SetUrlsVisitedResetTime()
    {
      if (this.policyGroup == null || this.policyGroup.class_schedule == null)
        return;
      DateTime now = DateTime.Now;
      DayOfWeek dayOfWeek = this.dayOfWeek;
      DateTime dateTime = new DateTime(now.Year, now.Month, now.Day);
      bool flag = false;
      while (true)
      {
        foreach (KeyValuePair<string, Schedule> keyValuePair in this.policyGroup.class_schedule)
        {
          if (!string.IsNullOrEmpty(keyValuePair.Key) && keyValuePair.Key.Contains(((int) this.dayOfWeek).ToString()))
          {
            Schedule schedule = keyValuePair.Value;
            if (schedule != null && schedule.HasValues)
            {
              Group.DaySchedule daySchedule = new Group.DaySchedule(schedule);
              this.urlsVisitedResetTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, daySchedule.endHour, daySchedule.endMinute, 0);
              if (DateTime.Compare(now, this.urlsVisitedResetTime) < 0)
              {
                Group.logger.Debug(this.Info + ", " + string.Format("reset URLs visited on {0}, ", (object) this.urlsVisitedResetTime.DayOfWeek) + string.Format("{0}", (object) this.urlsVisitedResetTime));
                return;
              }
              flag = true;
            }
          }
        }
        this.dayOfWeek = this.GetNextDayOfWeek(this.dayOfWeek);
        if (this.dayOfWeek != dayOfWeek || flag)
          dateTime = dateTime.AddDays(1.0);
        else
          break;
      }
      this.urlsVisitedResetTime = new DateTime(DateTime.MaxValue.Ticks);
      Group.logger.Debug(this.Info + ", no schedule");
    }

    private void ParsePolicyGroup()
    {
      this.zoneList.Clear();
      this._allowWhenDone = false;
      this.zoneListBlocked = false;
      this.HaveZoneList = false;
      this.InternetOff = false;
      string str1 = this.Info + ", " + (this.IsActive() ? "active" : "inactive");
      bool flag = false;
      if (this.policyGroup.web_rules == null)
      {
        Group.logger.Debug(str1 + ", (default Internet)");
      }
      else
      {
        this.HaveZoneList = true;
        WebRules webRules = this.policyGroup.web_rules;
        if (webRules.sites != null)
        {
          foreach (string site in webRules.sites)
          {
            flag = true;
            this.zoneList.Add(new Site(site));
          }
        }
        this.EncryptedUserEmail = webRules.encrypted_user_email;
        this.zoneListBlocked = webRules.blocked.GetValueOrDefault();
        this._allowWhenDone = webRules.allow_when_done.GetValueOrDefault();
        if (!flag)
        {
          this.InternetOff = true;
          Group.logger.Debug(str1 + ", (Internet Off)");
        }
        else
        {
          Group.logger.Debug(string.Format("{0}, allow when done = {1}, (Custom Internet)", (object) str1, (object) this._allowWhenDone));
          string str2 = this.zoneListBlocked ? "Block access" : "Restrict browsing";
          switch (this.zoneList.Count)
          {
            case 0:
              Group.logger.Debug("    " + str2 + " to 0 websites");
              break;
            case 1:
              Group.logger.Debug(string.Format("    {0} to website \"{1}\"", (object) str2, (object) this.zoneList[0]));
              break;
            default:
              Group.logger.Debug(string.Format("    {0} to {1} websites:", (object) str2, (object) this.zoneList.Count));
              using (List<Site>.Enumerator enumerator = this.zoneList.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  Site current = enumerator.Current;
                  Group.logger.Debug("        " + current.URI + " / " + current.Host + current.PathAndQuery);
                }
                break;
              }
          }
        }
      }
    }

    private bool MatchURLs(string allowedURL, string studentHost)
    {
      return studentHost.Contains(allowedURL);
    }

    private class DaySchedule
    {
      public int startHour;
      public int startMinute;
      public int endHour;
      public int endMinute;

      public int Start => Group.DaySchedule.GetRelativeTime(this.startHour, this.startMinute);

      public int End => Group.DaySchedule.GetRelativeTime(this.endHour, this.endMinute);

      public DaySchedule(Schedule schedule)
      {
        this.startHour = (int) Math.Truncate((double) schedule.s.Value);
        this.endHour = (int) Math.Truncate((double) schedule.e.Value);
        this.startMinute = (int) Math.Truncate(100.0 * ((double) schedule.s.Value - (double) this.startHour));
        this.endMinute = (int) Math.Truncate(100.0 * ((double) schedule.e.Value - (double) this.endHour));
      }

      public bool InSchedule(int relativeTime)
      {
        return relativeTime >= this.Start && relativeTime <= this.End;
      }

      public static int GetRelativeTime(int hour, int minute) => hour * 60 + minute;
    }
  }
}
