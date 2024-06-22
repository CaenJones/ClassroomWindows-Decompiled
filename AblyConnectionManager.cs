
using ClassroomWindows.Properties;
using IO.Ably;
using IO.Ably.Realtime;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#nullable disable
namespace ClassroomWindows
{
  public sealed class AblyConnectionManager
  {
    private const char channelDelimiter = ':';
    public AblyRealtime ably;
    private IRealtimeChannel userChannel;
    private PresenceState _currentPresenceState;
    private bool presenceUpdated;
    private string lastPresenceState;
    private bool lastRecording;
    private string _presenceState;
    private bool _recording;
    private bool _policyRefreshNeeded = true;
    private Dictionary<string, PublishedGroup> _publishedGroups = new Dictionary<string, PublishedGroup>();
    private static AblyConnectionManager s_instance = (AblyConnectionManager) null;
    private static readonly object LOCK = new object();
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public bool Connected => this.ably != null && this.ably.Connection.State == 2;

    public bool IsUserChannelReady => this.Connected && this.userChannel.State == 2;

    public static AblyConnectionManager Instance
    {
      get
      {
        lock (AblyConnectionManager.LOCK)
        {
          if (AblyConnectionManager.s_instance == null)
            AblyConnectionManager.s_instance = new AblyConnectionManager();
          return AblyConnectionManager.s_instance;
        }
      }
    }

    private AblyConnectionManager()
    {
    }

    private static string GetStringValue(Dictionary<string, object> data, string valueName)
    {
      // ISSUE: reference to a compiler-generated field
      if (AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__4 == null)
      {
        // ISSUE: reference to a compiler-generated field
        AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__4 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (AblyConnectionManager)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target1 = AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__4.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p4 = AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__4;
      // ISSUE: reference to a compiler-generated field
      if (AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__3 == null)
      {
        // ISSUE: reference to a compiler-generated field
        AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof (AblyConnectionManager), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, bool> target2 = AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__3.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, bool>> p3 = AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__3;
      object obj1;
      bool flag = data.TryGetValue(valueName, out obj1);
      object obj2;
      if (flag)
      {
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__2 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__2 = CallSite<Func<CallSite, bool, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof (AblyConnectionManager), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, bool, object, object> target3 = AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__2.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, bool, object, object>> p2 = AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__2;
        int num = flag ? 1 : 0;
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__1 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, Type, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof (AblyConnectionManager), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, Type, object> target4 = AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__1.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, Type, object>> p1 = AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__1;
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "GetType", (IEnumerable<Type>) null, typeof (AblyConnectionManager), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj3 = AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__0.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__21.\u003C\u003Ep__0, obj1);
        Type type = typeof (string);
        object obj4 = target4((CallSite) p1, obj3, type);
        obj2 = target3((CallSite) p2, num != 0, obj4);
      }
      else
        obj2 = (object) flag;
      object obj5 = target2((CallSite) p3, obj2) ? obj1 : (object) null;
      return target1((CallSite) p4, obj5);
    }

    public void UpdatePresence() => this.presenceUpdated = false;

    public void UpdateGroups()
    {
      this._publishedGroups.Clear();
      WebBrowsersManagerModule.Instance.SetDirty();
    }

    public async Task InitAsync()
    {
      AblyConnectionManager connectionManager = this;
      Config config = ConfigManager.Instance.Config;
      ClientOptions clientOptions1 = new ClientOptions(config.auth);
      ((AuthOptions) clientOptions1).QueryTime = new bool?(true);
      clientOptions1.UseBinaryProtocol = true;
      clientOptions1.AutoConnect = false;
      clientOptions1.AutomaticNetworkStateMonitoring = false;
      clientOptions1.ClientId = StudentPolicyApi.Instance.StudentPolicy.email;
      clientOptions1.FallbackHosts = new string[5]
      {
        "lightspeed-a-fallback.ably-realtime.com",
        "lightspeed-b-fallback.ably-realtime.com",
        "lightspeed-c-fallback.ably-realtime.com",
        "lightspeed-d-fallback.ably-realtime.com",
        "lightspeed-e-fallback.ably-realtime.com"
      };
      clientOptions1.Environment = "lightspeed";
      ClientOptions clientOptions2 = clientOptions1;
      connectionManager.ably = new AblyRealtime(clientOptions2);
      connectionManager.ably.Connection.ConnectionStateChanged += new EventHandler<ConnectionStateChange>(connectionManager.ConnectionStateChanged);
      AblyConnectionManager.logger.Debug("Initial connect");
      connectionManager.ably.Connect();
      connectionManager.userChannel = connectionManager.ably.Channels.Get(string.Format("{0}{1}", (object) config.customerID, (object) ':') + StudentPolicyApi.Instance.StudentPolicy.email);
      connectionManager.userChannel.StateChanged += new EventHandler<ChannelStateChange>(connectionManager.UserChannelStateChanged);
      connectionManager.userChannel.Presence.Subscribe(new Action<PresenceMessage>(connectionManager.Presence_MessageReceived));
      connectionManager.SubscribeUser();
      connectionManager.SetPresenceStateFromString("working");
      try
      {
        Result result = await connectionManager.userChannel.AttachAsync();
      }
      catch (Exception ex)
      {
        AblyConnectionManager.logger.Error(ex, "User channel attach failed");
      }
      connectionManager.EnterUserChannelPresence();
    }

    public void Update(double deltaTime)
    {
      StudentPolicyApi instance1 = StudentPolicyApi.Instance;
      if (this.IsPolicyRefreshNeeded)
      {
        AblyConnectionManager.logger.Debug("Reload student policy ...");
        int num = (int) instance1.Init();
        this._policyRefreshNeeded = false;
      }
      if (this.IsUserChannelReady)
      {
        if (!IpApi.Instance.OnCampus)
          return;
        this.PublishPresenceAsync().Wait();
        instance1.Update(deltaTime);
        WebBrowsersManagerModule instance2 = WebBrowsersManagerModule.Instance;
        if (!instance2.IsDirty())
          return;
        AblyConnectionManager.logger.Debug("Publishing tabs and group updates");
        this.PublishTabs();
        this.PublishGroupUpdates();
        instance2.OnTabsPublished();
      }
      else
        AblyConnectionManager.logger.Debug("User channel not ready");
    }

    public async Task<bool> IsClientIdPresent()
    {
      IEnumerable<PresenceMessage> async = await this.userChannel.Presence.GetAsync((string) null, (string) null, true);
      string email = StudentPolicyApi.Instance.StudentPolicy.email;
      foreach (PresenceMessage presenceMessage in async)
      {
        AblyConnectionManager.logger.Trace("ClientId = " + presenceMessage.ClientId);
        if (presenceMessage.ClientId == email)
          return true;
      }
      AblyConnectionManager.logger.Debug("ClientId \"" + email + "\" not present");
      return false;
    }

    public async Task PublishPresenceAsync()
    {
      if (await this.IsClientIdPresent() && this.presenceUpdated && this.lastPresenceState == this._presenceState && this.lastRecording == this._recording)
      {
        AblyConnectionManager.logger.Trace(string.Format("Unchanged, {0}, recording = {1}", (object) this._presenceState, (object) this._recording));
      }
      else
      {
        Dictionary<string, object> dictionary = new Dictionary<string, object>()
        {
          ["state"] = (object) this._presenceState,
          ["recording"] = (object) this._recording,
          ["ip"] = (object) IpApi.Instance.IpAddress
        };
        try
        {
          Result result = await this.userChannel.Presence.UpdateAsync((object) dictionary);
        }
        catch (Exception ex)
        {
          AblyConnectionManager.logger.Error(ex, string.Format("Failed, {0}, recording = {1}", (object) this._presenceState, (object) this._recording));
          return;
        }
        AblyConnectionManager.logger.Debug(string.Format("Updated, state = {0}, recording = {1}, ", (object) this._presenceState, (object) this._recording) + "external IP = " + IpApi.Instance.IpAddress);
        this.presenceUpdated = true;
        this.lastPresenceState = this._presenceState;
        this.lastRecording = this._recording;
      }
    }

    public void PublishTabs()
    {
      Dictionary<string, object>[] browserTabs = this.GetBrowserTabs();
      foreach (Dictionary<string, object> dictionary in browserTabs)
      {
        string str1 = "";
        string str2 = "";
        foreach (KeyValuePair<string, object> keyValuePair in dictionary)
        {
          str1 += string.Format("{0}{1}:\"{2}\"", (object) str2, (object) keyValuePair.Key, keyValuePair.Value);
          str2 = ", ";
        }
        AblyConnectionManager.logger.Trace(str1);
      }
      this.userChannel.PublishAsync("tabs", (object) browserTabs, (string) null);
    }

    public void PublishGroupUpdates()
    {
      foreach (KeyValuePair<string, Group> group1 in StudentPolicyApi.Instance.Groups)
      {
        Group group2 = group1.Value;
        if (group2.IsActive())
        {
          AblyConnectionManager.logger.Debug(group2.Info + ", active");
          this.PublishUpdatesForGroup(group2, false);
        }
        else
          AblyConnectionManager.logger.Debug(group2.Info + ", inactive");
      }
    }

    public void PublishUpdatesForGroup(Group group, bool force)
    {
      this.GetBrowserTabs();
      string url;
      string favicon;
      string browser;
      WebBrowsersManagerModule.Instance.GetCurrentlyBrowsing(out url, out favicon, out browser);
      WebsiteTracker websiteTracker = group.WebsiteTracker;
      if (websiteTracker == null)
      {
        AblyConnectionManager.logger.Warn(Group.FormatGuid(group.Guid) + ": No WebsiteTracker found");
      }
      else
      {
        PublishedGroup newGroup = new PublishedGroup()
        {
          currentUrl = url,
          currentBrowser = browser,
          currentFavicon = favicon,
          visits = websiteTracker.Visits,
          mostViewed = websiteTracker.MostViewed
        };
        if (!force && !this.IsGroupDataChanged(group.Guid, newGroup))
          return;
        string str = "";
        if (!string.IsNullOrEmpty(url))
          str = new Uri(url).Host;
        string mostViewed = websiteTracker.MostViewed;
        double mostViewedTime = websiteTracker.MostViewedTime;
        Dictionary<string, object> dictionary1 = new Dictionary<string, object>()
        {
          ["currentHost"] = (object) str,
          ["currentBrowser"] = (object) browser,
          ["favico"] = (object) favicon,
          ["visits"] = (object) websiteTracker.Visits,
          ["top"] = (object) mostViewed,
          ["topTime"] = (object) mostViewedTime
        };
        Dictionary<string, Dictionary<string, object>> dictionary2 = new Dictionary<string, Dictionary<string, object>>();
        dictionary2[group.Guid] = dictionary1;
        this._publishedGroups[group.Guid] = newGroup;
        AblyConnectionManager.logger.Trace(Global.newLine + "Caching group guid = \"" + group.Guid + "\"" + Global.newLine + "Current host = \"" + url + "\"" + Global.newLine + "Current favico = \"" + favicon + "\"" + Global.newLine + string.Format("Visits = {0}{1}", (object) websiteTracker.Visits, (object) Global.newLine) + "Top host = \"" + mostViewed + "\"" + Global.newLine + string.Format("Top host time = {0}{1}", (object) mostViewedTime, (object) Global.newLine));
        this.userChannel.PublishAsync("groupUpdate", (object) dictionary2, (string) null);
      }
    }

    private bool IsGroupDataChanged(string guid, PublishedGroup newGroup)
    {
      PublishedGroup publishedGroup;
      try
      {
        publishedGroup = this._publishedGroups[guid];
      }
      catch (KeyNotFoundException ex)
      {
        return true;
      }
      return !publishedGroup.SameAs(newGroup);
    }

    public bool IsPolicyRefreshNeeded => this._policyRefreshNeeded && this.Connected;

    public PresenceState CurrentPresenceState
    {
      get => this._currentPresenceState;
      set
      {
        if (this._currentPresenceState == value)
          return;
        this._currentPresenceState = value;
        switch (this._currentPresenceState)
        {
          case PresenceState.NeedHelp:
            SystemTrayManager.Instance.UpdateIcon(Resources.icon_need_help);
            this._presenceState = "need help";
            break;
          case PresenceState.Done:
            SystemTrayManager.Instance.UpdateIcon(Resources.icon_done);
            this._presenceState = "done";
            break;
          case PresenceState.NeedExtension:
            SystemTrayManager.Instance.UpdateIcon(Resources.icon_need_help);
            this._presenceState = "need extension";
            break;
          default:
            SystemTrayManager.Instance.UpdateIcon(Resources.icon_working);
            this._presenceState = "working";
            break;
        }
      }
    }

    public bool Recording
    {
      get => this._recording;
      set => this._recording = value;
    }

    public void SetPresenceStateFromString(string value)
    {
      switch (value)
      {
        case "need help":
          this.CurrentPresenceState = PresenceState.NeedHelp;
          break;
        case "done":
          this.CurrentPresenceState = PresenceState.Done;
          break;
        case "need extension":
          this.CurrentPresenceState = PresenceState.NeedExtension;
          break;
        default:
          this.CurrentPresenceState = PresenceState.Working;
          break;
      }
    }

    public void offer_rtc_ice(string sessionId, string sdp, string sdpMid, int sdpMLineIndex)
    {
      AblyConnectionManager.logger.Debug("Session ID = " + sessionId + "," + Global.newLine + "SDP = \"" + sdp + "\"," + Global.newLine + "SDP media stream ID = " + sdpMid + "," + Global.newLine + string.Format("SDP m-line index = {0}", (object) sdpMLineIndex));
      Dictionary<string, object> dictionary = new Dictionary<string, object>()
      {
        ["candidate"] = (object) sdp,
        [nameof (sdpMid)] = (object) sdpMid,
        [nameof (sdpMLineIndex)] = (object) sdpMLineIndex
      };
      this.userChannel.PublishAsync(nameof (offer_rtc_ice), (object) new Dictionary<string, object>()
      {
        [nameof (sessionId)] = (object) sessionId,
        ["ice"] = (object) dictionary
      }, (string) null);
    }

    public void offer_rtc(string sessionId, string sdp)
    {
      AblyConnectionManager.logger.Debug("Session ID = " + sessionId);
      Dictionary<string, object> dictionary = new Dictionary<string, object>()
      {
        ["type"] = (object) "offer",
        [nameof (sdp)] = (object) sdp
      };
      this.userChannel.PublishAsync(nameof (offer_rtc), (object) new Dictionary<string, object>()
      {
        [nameof (sessionId)] = (object) sessionId,
        ["offer"] = (object) dictionary
      }, (string) null);
    }

    public Dictionary<string, object>[] GetBrowserTabs()
    {
      List<Dictionary<string, object>> dictionaryList = new List<Dictionary<string, object>>();
      foreach (BrowserTab tab in WebBrowsersManagerModule.Instance.GetTabs(false))
      {
        Dictionary<string, object> dictionary = new Dictionary<string, object>()
        {
          ["active"] = (object) tab.active,
          ["browser"] = (object) tab.browserName,
          ["url"] = (object) tab.url,
          ["title"] = (object) tab.title,
          ["windowID"] = (object) tab.windowId,
          ["id"] = (object) tab.id,
          ["favico"] = (object) tab.favicon,
          ["foreground"] = (object) WebBrowsersManagerModule.Instance.IsForeground(tab)
        };
        dictionaryList.Add(dictionary);
      }
      return dictionaryList.ToArray();
    }

    private void EnterUserChannelPresence()
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["state"] = this._presenceState;
      if (this.userChannel.State == 2)
      {
        try
        {
          this.userChannel.Presence.EnterAsync((object) dictionary);
        }
        catch (Exception ex)
        {
          AblyConnectionManager.logger.Error(ex, "Unable to publisy user presence");
        }
      }
      else
        AblyConnectionManager.logger.Debug(string.Format("Unable to publish user presence because channel is {0}", (object) this.userChannel.State));
    }

    private void SubscribeUser()
    {
      AblyConnectionManager.logger.Debug(this.userChannel.Name);
      this.userChannel.Subscribe("setState", new Action<Message>(this.Handle_setState));
      this.userChannel.Subscribe("closeTab", new Action<Message>(this.Handle_closeTab));
      this.userChannel.Subscribe("focusTab", new Action<Message>(this.Handle_focusTab));
      this.userChannel.Subscribe("request_rtc", new Action<Message>(this.Handle_request_rtc));
      this.userChannel.Subscribe("answer_rtc", new Action<Message>(this.Handle_answer_rtc));
      this.userChannel.Subscribe("answer_rtc_ice", new Action<Message>(this.Handle_answer_rtc_ice));
      this.userChannel.Subscribe("tm", new Action<Message>(this.Handle_teacherMessage));
      this.userChannel.Subscribe("url", new Action<Message>(this.Handle_url));
      this.userChannel.Subscribe("lock", new Action<Message>(this.Handle_lock));
      this.userChannel.Subscribe("unlock", new Action<Message>(this.Handle_unlock));
      this.userChannel.Subscribe("sendGroupUpdate", new Action<Message>(this.Handle_sendGroupUpdate));
      this.userChannel.Subscribe("policyUpdate", new Action<Message>(this.Handle_policyUpdate));
      this.userChannel.Subscribe("startRecording", new Action<Message>(this.Handle_startRecording));
    }

    private void ConnectionStateChanged(object sender, ConnectionStateChange state)
    {
      ErrorInfo reason = state.Reason;
      ConnectionState current = state.Current;
      if (current != 2)
      {
        if (current != 3)
        {
          if (current == 7)
          {
            if (reason == null)
              AblyConnectionManager.logger.Debug("Failed; no reason given. Exiting ...");
            else
              AblyConnectionManager.logger.Debug(string.Format("Failed due to {0}, code = {1}. Exiting ...", (object) reason.ToString(), (object) reason.Code));
            Program.Exit();
          }
          else if (reason == null)
            AblyConnectionManager.logger.Debug(string.Format("{0}; no reason given", (object) state.Current));
          else
            AblyConnectionManager.logger.Debug(string.Format("{0}; {1}, code = {2}", (object) state.Current, (object) reason.ToString(), (object) reason.Code));
        }
        else
        {
          if (reason == null)
            AblyConnectionManager.logger.Debug("Disconnected; no reason given. Exiting ...");
          else
            AblyConnectionManager.logger.Debug(string.Format("Disconnected due to {0}, code = {1}. Exiting ...", (object) reason.ToString(), (object) reason.Code));
          Program.Exit();
        }
      }
      else
      {
        AblyConnectionManager.logger.Debug("Connected, attaching user channel ...");
        this.userChannel.Attach((Action<bool, ErrorInfo>) null);
      }
    }

    private void UserChannelStateChanged(object sender, ChannelStateChange state)
    {
      if (this.Connected)
      {
        ChannelState current = state.Current;
        if (current == 4 || current == 6)
        {
          AblyConnectionManager.logger.Debug(string.Format("{0} ", (object) state.Current) + string.Format("(connection state is {0}), reattaching ...", (object) this.ably.Connection.State));
          this.userChannel.Attach((Action<bool, ErrorInfo>) null);
          return;
        }
      }
      AblyConnectionManager.logger.Debug(string.Format("{0} (connection state is {1})", (object) state.Current, (object) this.ably.Connection.State));
    }

    private void Presence_MessageReceived(PresenceMessage message)
    {
      string source = message.Data.ToString();
      AblyConnectionManager.logger.Debug("Message data = \"" + Global.RemoveLineEndings(source) + "\"");
      Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(source);
      object obj;
      if (dictionary.TryGetValue("viewingTabs", out obj))
      {
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__48.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__48.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (bool), typeof (AblyConnectionManager)));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__48.\u003C\u003Ep__0.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__48.\u003C\u003Ep__0, obj) && IpApi.Instance.OnCampus)
          this.PublishTabs();
      }
      if (!dictionary.TryGetValue("state", out obj))
        return;
      // ISSUE: reference to a compiler-generated field
      if (AblyConnectionManager.\u003C\u003Eo__48.\u003C\u003Ep__1 == null)
      {
        // ISSUE: reference to a compiler-generated field
        AblyConnectionManager.\u003C\u003Eo__48.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (AblyConnectionManager)));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      this.SetPresenceStateFromString(AblyConnectionManager.\u003C\u003Eo__48.\u003C\u003Ep__1.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__48.\u003C\u003Ep__1, obj));
    }

    private void Handle_setState(Message message)
    {
      string str = message.Data.ToString();
      AblyConnectionManager.logger.Debug("Message data = \"" + str + "\"");
      if (!IpApi.Instance.OnCampus)
        return;
      this.SetPresenceStateFromString(str);
    }

    private void Handle_teacherMessage(Message message)
    {
      string str = message.Data.ToString();
      AblyConnectionManager.logger.Debug("Message data = \"" + str + "\"");
      if (!IpApi.Instance.OnCampus)
        return;
      Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(str);
      object obj;
      if (!dictionary.TryGetValue("m", out obj))
        return;
      // ISSUE: reference to a compiler-generated field
      if (AblyConnectionManager.\u003C\u003Eo__50.\u003C\u003Ep__0 == null)
      {
        // ISSUE: reference to a compiler-generated field
        AblyConnectionManager.\u003C\u003Eo__50.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (AblyConnectionManager)));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      string tipText = AblyConnectionManager.\u003C\u003Eo__50.\u003C\u003Ep__0.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__50.\u003C\u003Ep__0, obj);
      NotificationType notificationType = NotificationType.None;
      if (dictionary.TryGetValue("mId", out obj))
      {
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__50.\u003C\u003Ep__1 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__50.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, long>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (long), typeof (AblyConnectionManager)));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        notificationType = (NotificationType) AblyConnectionManager.\u003C\u003Eo__50.\u003C\u003Ep__1.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__50.\u003C\u003Ep__1, obj);
      }
      SystemTrayManager.Instance.DisplayBalloonTip(tipText, notificationType);
    }

    private void Handle_closeTab(Message message)
    {
      string str = message.Data.ToString();
      AblyConnectionManager.logger.Debug("Message data = \"" + str + "\"");
      if (!IpApi.Instance.OnCampus)
        return;
      Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(str);
      object obj;
      if (dictionary.TryGetValue("tabId", out obj))
      {
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__51.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__51.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, long>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (long), typeof (AblyConnectionManager)));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        long tabId = AblyConnectionManager.\u003C\u003Eo__51.\u003C\u003Ep__0.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__51.\u003C\u003Ep__0, obj);
        AblyConnectionManager.logger.Debug(string.Format("tabId = {0}", (object) tabId));
        WebBrowsersManagerModule.Instance.CloseTabWithId(tabId);
      }
      else
      {
        if (!dictionary.TryGetValue("url", out obj))
          return;
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__51.\u003C\u003Ep__1 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__51.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (AblyConnectionManager)));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        string url = AblyConnectionManager.\u003C\u003Eo__51.\u003C\u003Ep__1.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__51.\u003C\u003Ep__1, obj);
        AblyConnectionManager.logger.Debug("url = " + url);
        WebBrowsersManagerModule.Instance.CloseTabWithUrl(url);
      }
    }

    private void Handle_focusTab(Message message)
    {
      string str = message.Data.ToString();
      AblyConnectionManager.logger.Debug("Message data = \"" + str + "\"");
      object obj;
      if (!IpApi.Instance.OnCampus || !JsonConvert.DeserializeObject<Dictionary<string, object>>(str).TryGetValue("tabId", out obj))
        return;
      // ISSUE: reference to a compiler-generated field
      if (AblyConnectionManager.\u003C\u003Eo__52.\u003C\u003Ep__0 == null)
      {
        // ISSUE: reference to a compiler-generated field
        AblyConnectionManager.\u003C\u003Eo__52.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, long>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (long), typeof (AblyConnectionManager)));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      long tabId = AblyConnectionManager.\u003C\u003Eo__52.\u003C\u003Ep__0.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__52.\u003C\u003Ep__0, obj);
      AblyConnectionManager.logger.Debug(string.Format("tabId = {0}", (object) tabId));
      WebBrowsersManagerModule.Instance.FocusTab(tabId);
    }

    private void Handle_url(Message message)
    {
      string url = message.Data.ToString();
      AblyConnectionManager.logger.Debug("Message data = \"" + url + "\"");
      if (!IpApi.Instance.OnCampus)
        return;
      WebBrowsersManagerModule.Instance.NewTab(url);
    }

    private void Handle_lock(Message message)
    {
      string str = message.Data.ToString();
      AblyConnectionManager.logger.Debug("Message data = \"" + str + "\"");
      if (!IpApi.Instance.OnCampus)
        return;
      LockScreen.Lock(AblyConnectionManager.GetStringValue(JsonConvert.DeserializeObject<Dictionary<string, object>>(str), "lockMessage"));
    }

    private void Handle_unlock(Message message)
    {
      string str = message.Data.ToString();
      AblyConnectionManager.logger.Debug("Message data = \"" + str + "\"");
      if (!IpApi.Instance.OnCampus)
        return;
      LockScreen.Unlock();
    }

    private void Handle_sendGroupUpdate(Message message)
    {
      string str = message.Data.ToString();
      AblyConnectionManager.logger.Debug("Message data = \"" + str + "\"");
      Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(str);
      if (!IpApi.Instance.OnCampus)
        return;
      object obj;
      if (dictionary.TryGetValue("groupGuid", out obj))
      {
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__56.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__56.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (AblyConnectionManager)));
        }
        Group group;
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        if (!StudentPolicyApi.Instance.GetGroup(AblyConnectionManager.\u003C\u003Eo__56.\u003C\u003Ep__0.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__56.\u003C\u003Ep__0, obj), out group))
          return;
        if (group.IsActive())
        {
          AblyConnectionManager.logger.Debug(group.Info + ", active, publish group update");
          this.PublishUpdatesForGroup(group, true);
        }
        else
          AblyConnectionManager.logger.Debug(group.Info + ", inactive");
      }
      else
        AblyConnectionManager.logger.Debug("Missing \"groupGuid\"");
    }

    private void Handle_policyUpdate(Message message)
    {
      string str = message.Data.ToString();
      AblyConnectionManager.logger.Debug("Message data = \"" + str + "\"");
      this._policyRefreshNeeded = true;
      AblyConnectionManager.logger.Debug("Policy refresh flag set");
    }

    private void Handle_startRecording(Message message)
    {
      string str = message.Data.ToString();
      AblyConnectionManager.logger.Debug("Message data = \"" + str + "\"");
      int howManySeconds;
      if (!IpApi.Instance.OnCampus || !JsonConvert.DeserializeObject<Dictionary<string, int>>(str).TryGetValue("duration", out howManySeconds))
        return;
      RecorderModule.Instance.StartRecording(howManySeconds);
    }

    private void Handle_request_rtc(Message message)
    {
      string str1 = message.Data.ToString();
      AblyConnectionManager.logger.Trace("Message Data = \"" + str1 + "\"");
      if (!IpApi.Instance.OnCampus)
        return;
      Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(str1);
      object obj1;
      object obj2;
      if (dictionary.TryGetValue("sessionId", out obj1) && dictionary.TryGetValue("customIce", out obj2))
      {
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__59.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__59.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (AblyConnectionManager)));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        string sessionId = AblyConnectionManager.\u003C\u003Eo__59.\u003C\u003Ep__0.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__59.\u003C\u003Ep__0, obj1);
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__59.\u003C\u003Ep__1 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__59.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, JArray>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (JArray), typeof (AblyConnectionManager)));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        JArray jarray = AblyConnectionManager.\u003C\u003Eo__59.\u003C\u003Ep__1.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__59.\u003C\u003Ep__1, obj2);
        List<WebRTCModule.CustomIceParam> customIceParams = new List<WebRTCModule.CustomIceParam>();
        string str2 = "";
        string str3 = "";
        foreach (JObject jobject in jarray)
        {
          string str4 = JToken.op_Explicit(jobject["url"]);
          string str5 = JToken.op_Explicit(jobject["credential"]);
          string str6 = JToken.op_Explicit(jobject["username"]);
          if (str4 != null)
          {
            WebRTCModule.CustomIceParam customIceParam = new WebRTCModule.CustomIceParam()
            {
              url = str4,
              credential = str5 ?? string.Empty,
              username = str6 ?? string.Empty
            };
            str2 = str2 + str3 + Global.newLine + str4;
            str3 = ",";
            customIceParams.Add(customIceParam);
          }
        }
        AblyConnectionManager.logger.Debug("Session ID = " + sessionId + ", ICE servers:" + str2);
        WebRTCModule.Instance.CreateSession(customIceParams, sessionId);
      }
      else
        AblyConnectionManager.logger.Warn("Missing \"sessionid\" or \"customIce\"");
    }

    private void Handle_answer_rtc(Message message)
    {
      string str1 = message.Data.ToString();
      AblyConnectionManager.logger.Trace("Message Data = \"" + str1 + "\"");
      if (!IpApi.Instance.OnCampus)
        return;
      Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(str1);
      object obj1;
      object obj2;
      if (dictionary.TryGetValue("sessionId", out obj1) && dictionary.TryGetValue("answer", out obj2))
      {
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__60.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__60.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (AblyConnectionManager)));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        string sessionId = AblyConnectionManager.\u003C\u003Eo__60.\u003C\u003Ep__0.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__60.\u003C\u003Ep__0, obj1);
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__60.\u003C\u003Ep__1 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__60.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, JObject>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (JObject), typeof (AblyConnectionManager)));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        JObject jobject = AblyConnectionManager.\u003C\u003Eo__60.\u003C\u003Ep__1.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__60.\u003C\u003Ep__1, obj2);
        string str2 = JToken.op_Explicit(jobject["type"]);
        string str3 = JToken.op_Explicit(jobject["sdp"]);
        AblyConnectionManager.logger.Debug("Session ID = " + sessionId + ", type = " + str2 + ", SDP:" + Global.newLine + Global.IndentLines(str3));
        WebRTCModule.Instance.SetAnswer(sessionId, str3);
      }
      else
        AblyConnectionManager.logger.Warn("Missing \"sessionid\" or \"answer\"");
    }

    private void Handle_answer_rtc_ice(Message message)
    {
      string str = message.Data.ToString();
      AblyConnectionManager.logger.Trace(Global.newLine + "Message Data = \"" + str + "\"");
      if (!IpApi.Instance.OnCampus)
        return;
      Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(str);
      object obj1;
      object obj2;
      if (dictionary.TryGetValue("sessionId", out obj1) && dictionary.TryGetValue("ice", out obj2))
      {
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (AblyConnectionManager)));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        string sessionId = AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__0.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__0, obj1);
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__2 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__2 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof (AblyConnectionManager), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, bool> target = AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__2.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, bool>> p2 = AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__2;
        // ISSUE: reference to a compiler-generated field
        if (AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__1 == null)
        {
          // ISSUE: reference to a compiler-generated field
          AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof (AblyConnectionManager), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj3 = AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__1.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__1, obj2, (object) null);
        if (target((CallSite) p2, obj3))
        {
          // ISSUE: reference to a compiler-generated field
          if (AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__3 == null)
          {
            // ISSUE: reference to a compiler-generated field
            AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, JObject>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (JObject), typeof (AblyConnectionManager)));
          }
          // ISSUE: reference to a compiler-generated field
          // ISSUE: reference to a compiler-generated field
          JObject jobject = AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__3.Target((CallSite) AblyConnectionManager.\u003C\u003Eo__61.\u003C\u003Ep__3, obj2);
          string sdp = JToken.op_Explicit(jobject["candidate"]);
          string sdp_mid = JToken.op_Explicit(jobject["sdpMid"]);
          int sdp_mline_index = JToken.op_Explicit(jobject["sdpMLineIndex"]);
          AblyConnectionManager.logger.Debug("Session ID = " + sessionId + Global.newLine + "Candidate = " + sdp + Global.newLine + "SDP media stream ID = " + sdp_mid + Global.newLine + string.Format("SDP m-line index = {0}", (object) sdp_mline_index));
          WebRTCModule.Instance.AddIceCandidate(sessionId, sdp_mid, sdp_mline_index, sdp);
        }
        else
          AblyConnectionManager.logger.Warn("Missing \"ice\"");
      }
      else
        AblyConnectionManager.logger.Warn("Missing \"sessionid\" or \"ice\"");
    }
  }
}
