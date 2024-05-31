// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.WebBrowserModule
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

#nullable disable
namespace ClassroomWindows
{
  public sealed class WebBrowserModule : IModule, IDisposable
  {
    public const int chromePort = 61337;
    public const int edgePort = 62337;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private HttpListener listener;
    private Queue<IWebBrowserCommand> _pendingCommands = new Queue<IWebBrowserCommand>();
    private List<BrowserTab> _browserTabs = new List<BrowserTab>();
    private BrowserTab activeTab;
    private bool _browserActive;
    private double stateTime;
    private bool _isDirty = true;
    public readonly string name;
    private readonly string processName;
    private readonly string uriPrefix;
    private readonly int port;
    private readonly AsyncCallback asyncCallback;

    public bool IsDirty
    {
      get => this._isDirty;
      set => this._isDirty = value;
    }

    public List<BrowserTab> Tabs => this._browserTabs;

    public bool IsEdge => this.port == 62337;

    public bool IsChrome => this.port == 61337;

    public WebBrowserModule(
      string name,
      string processName,
      int port,
      AsyncCallback asyncCallback)
    {
      this.name = name;
      this.processName = processName;
      this.port = port;
      this.uriPrefix = string.Format("http://localhost:{0}/", (object) port);
      this.asyncCallback = asyncCallback;
      this.StartListener();
    }

    ~WebBrowserModule() => this.Dispose();

    public void Update(BackgroundWorker bgw, double deltaTime) => this.stateTime += deltaTime;

    public void Close()
    {
      this.Tabs.Clear();
      this.activeTab = (BrowserTab) null;
    }

    public void Dispose()
    {
      if (this.listener != null)
      {
        WebBrowserModule.logger.Debug("Close listener");
        this.listener.Abort();
        this.listener.Close();
        this.listener = (HttpListener) null;
      }
      else
        WebBrowserModule.logger.Debug("null listener");
    }

    public void CloseTabWithId(long tabId)
    {
      this._pendingCommands.Enqueue((IWebBrowserCommand) new CloseTabWithIdWebBrowserCommand(tabId));
    }

    public void CloseTabWithUrl(string url)
    {
      foreach (BrowserTab browserTab in this._browserTabs)
      {
        if (url.Equals(browserTab.url))
        {
          this.CloseTabWithId((long) browserTab.id);
          break;
        }
      }
    }

    public void FocusTab(long tabId)
    {
      this._pendingCommands.Enqueue((IWebBrowserCommand) new FocusTabWebBrowserCommand(tabId));
    }

    public void NewTab(string url)
    {
      WebBrowserModule.logger.Debug("Opening \"" + url + "\" in new browser instance ...");
      Process.Start(url);
    }

    public void UpdateTab(long tabId, string url)
    {
      this._pendingCommands.Enqueue((IWebBrowserCommand) new UpdateTabWebBrowserCommand(tabId, url));
    }

    public void OnUnlocked()
    {
      WebBrowserModule.logger.Debug("Session unlocked; resetting last communication timer");
    }

    public bool IsOpen()
    {
      foreach (Process process in Process.GetProcesses())
      {
        if (process.ProcessName == this.processName && !process.HasExited)
          return true;
      }
      return false;
    }

    private void OnBrowserTabsChanged()
    {
      this.stateTime = 0.0;
      this._isDirty = true;
      foreach (BrowserTab browserTab in this._browserTabs)
      {
        string encrypted_customer_id;
        string group_guid;
        if (StudentPolicyApi.Instance.IsSiteBlocked(browserTab.url, AblyConnectionManager.Instance.CurrentPresenceState, out encrypted_customer_id, out group_guid))
        {
          string blockPage = ConfigManager.Instance.GetBlockPage(encrypted_customer_id, group_guid);
          WebBrowserModule.logger.Debug("Redirecting URL = \"" + browserTab.url + "\" to block page \"" + blockPage + "\"");
          this.UpdateTab((long) browserTab.id, blockPage);
        }
      }
    }

    private bool IsNewActiveTab(BrowserTab activeTab)
    {
      if (!activeTab.active)
        return false;
      foreach (BrowserTab browserTab in this._browserTabs)
      {
        if (browserTab.id == activeTab.id && browserTab.windowId == activeTab.windowId && (!browserTab.active || browserTab.url != activeTab.url))
          return true;
      }
      return true;
    }

    private bool IsBrowserActive(JObject tab)
    {
      JToken jtoken = tab.GetValue("browserActive");
      return jtoken != null && JToken.op_Explicit(jtoken);
    }

    public void ProcessRequest(IAsyncResult result)
    {
      if (this.listener == null)
      {
        WebBrowserModule.logger.Debug(this.name + " null listener");
      }
      else
      {
        HttpListenerContext context = (HttpListenerContext) null;
        if (!this.listener.IsListening)
        {
          WebBrowserModule.logger.Debug(this.name + " not listening");
        }
        else
        {
          try
          {
            context = this.listener.EndGetContext(result);
          }
          catch (Exception ex)
          {
            WebBrowserModule.logger.Warn(ex, this.name + " end get context failed");
          }
          if (context == null)
          {
            WebBrowserModule.logger.Debug(this.name + "; null context");
          }
          else
          {
            HttpListenerRequest request = context.Request;
            if (request == null)
            {
              WebBrowserModule.logger.Debug(this.name + "; null request");
            }
            else
            {
              using (StreamReader streamReader = new StreamReader(request.InputStream, request.ContentEncoding))
              {
                string end = streamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(end))
                {
                  Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(end);
                  object obj1;
                  if (dictionary.TryGetValue("action", out obj1))
                  {
                    // ISSUE: reference to a compiler-generated field
                    if (WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__0 == null)
                    {
                      // ISSUE: reference to a compiler-generated field
                      WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (WebBrowserModule)));
                    }
                    // ISSUE: reference to a compiler-generated field
                    // ISSUE: reference to a compiler-generated field
                    string str = WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__0.Target((CallSite) WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__0, obj1);
                    if (str.Equals("sendTabs"))
                    {
                      if (!IpApi.Instance.OnCampus)
                        return;
                      List<BrowserTab> tabs = new List<BrowserTab>();
                      // ISSUE: reference to a compiler-generated field
                      if (WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__3 == null)
                      {
                        // ISSUE: reference to a compiler-generated field
                        WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof (WebBrowserModule), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
                        {
                          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
                        }));
                      }
                      // ISSUE: reference to a compiler-generated field
                      Func<CallSite, object, bool> target1 = WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__3.Target;
                      // ISSUE: reference to a compiler-generated field
                      CallSite<Func<CallSite, object, bool>> p3 = WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__3;
                      object obj2;
                      bool flag1 = dictionary.TryGetValue("data", out obj2);
                      object obj3;
                      if (flag1)
                      {
                        // ISSUE: reference to a compiler-generated field
                        if (WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__2 == null)
                        {
                          // ISSUE: reference to a compiler-generated field
                          WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__2 = CallSite<Func<CallSite, bool, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof (WebBrowserModule), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
                          {
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null),
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
                          }));
                        }
                        // ISSUE: reference to a compiler-generated field
                        Func<CallSite, bool, object, object> target2 = WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__2.Target;
                        // ISSUE: reference to a compiler-generated field
                        CallSite<Func<CallSite, bool, object, object>> p2 = WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__2;
                        int num = flag1 ? 1 : 0;
                        // ISSUE: reference to a compiler-generated field
                        if (WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__1 == null)
                        {
                          // ISSUE: reference to a compiler-generated field
                          WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof (WebBrowserModule), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
                          {
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, (string) null)
                          }));
                        }
                        // ISSUE: reference to a compiler-generated field
                        // ISSUE: reference to a compiler-generated field
                        object obj4 = WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__1.Target((CallSite) WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__1, obj2, (object) null);
                        obj3 = target2((CallSite) p2, num != 0, obj4);
                      }
                      else
                        obj3 = (object) flag1;
                      if (target1((CallSite) p3, obj3))
                      {
                        // ISSUE: reference to a compiler-generated field
                        if (WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__4 == null)
                        {
                          // ISSUE: reference to a compiler-generated field
                          WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__4 = CallSite<Func<CallSite, object, JArray>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (JArray), typeof (WebBrowserModule)));
                        }
                        // ISSUE: reference to a compiler-generated field
                        // ISSUE: reference to a compiler-generated field
                        JArray jarray = WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__4.Target((CallSite) WebBrowserModule.\u003C\u003Eo__39.\u003C\u003Ep__4, obj2);
                        bool flag2 = false;
                        foreach (JObject tab in jarray)
                        {
                          BrowserTab browserTab = new BrowserTab()
                          {
                            active = JToken.op_Explicit(tab["active"]),
                            browserName = this.name,
                            favicon = JToken.op_Explicit(tab["favIconUrl"]),
                            id = JToken.op_Explicit(tab["id"]),
                            title = JToken.op_Explicit(tab["title"]),
                            url = JToken.op_Explicit(tab["url"]),
                            windowId = JToken.op_Explicit(tab["windowId"]),
                            module = this
                          };
                          if (this.IsBrowserActive(tab))
                            flag2 = true;
                          tabs.Add(browserTab);
                        }
                        if (this._browserTabs.Count > 0 && tabs.Count == 0)
                        {
                          this.activeTab = (BrowserTab) null;
                          WebBrowsersManagerModule.Instance.NoBrowserTabs(this);
                          this._browserTabs = tabs;
                          this.OnBrowserTabsChanged();
                        }
                        else if (this._browserActive != flag2)
                        {
                          this._browserActive = flag2;
                          if (this._browserActive)
                          {
                            foreach (BrowserTab browserTab in tabs)
                            {
                              if (browserTab.active)
                              {
                                this.activeTab = browserTab;
                                WebBrowserModule.logger.Debug("New active tab," + Global.newLine + "Browser = \"" + browserTab.browserName + "\"," + Global.newLine + "Title = \"" + browserTab.title + "\"," + Global.newLine + "URL = \"" + browserTab.url + "\"," + Global.newLine + string.Format("ID = {0},{1}", (object) browserTab.id, (object) Global.newLine) + string.Format("Window ID = {0}", (object) browserTab.windowId));
                                WebBrowsersManagerModule.Instance.SetCurrentlyBrowsing(this.activeTab);
                                break;
                              }
                            }
                          }
                          this._browserTabs = tabs;
                          this.OnBrowserTabsChanged();
                        }
                        else if (this.IsTabsChanged(tabs))
                        {
                          foreach (BrowserTab activeTab in tabs)
                          {
                            if (this.IsNewActiveTab(activeTab))
                            {
                              this.activeTab = activeTab;
                              WebBrowserModule.logger.Debug("New active tab," + Global.newLine + "Browser = \"" + this.activeTab.browserName + "\"," + Global.newLine + "Title = \"" + this.activeTab.title + "\"," + Global.newLine + "URL = \"" + this.activeTab.url + "\"," + Global.newLine + string.Format("ID = {0},{1}", (object) this.activeTab.id, (object) Global.newLine) + string.Format("Window ID = {0}", (object) this.activeTab.windowId));
                              WebBrowsersManagerModule.Instance.SetCurrentlyBrowsing(this.activeTab);
                            }
                            else
                              WebBrowserModule.logger.Debug((activeTab.active ? "Active" : "Inactive") + "," + Global.newLine + "Browser = \"" + activeTab.browserName + "\"," + Global.newLine + "Title = \"" + activeTab.title + "\"," + Global.newLine + "URL = \"" + activeTab.url + "\"," + Global.newLine + string.Format("ID = {0},{1}", (object) activeTab.id, (object) Global.newLine) + string.Format("Window ID = {0}", (object) activeTab.windowId));
                          }
                          this._browserTabs = tabs;
                          this.OnBrowserTabsChanged();
                        }
                      }
                    }
                    else
                      WebBrowserModule.logger.Debug("Browser action = \"" + str + "\"");
                  }
                  this.SendResponse(context);
                }
              }
              this.listener.BeginGetContext(this.asyncCallback, (object) null);
            }
          }
        }
      }
    }

    private bool IsTabsChanged(List<BrowserTab> tabs)
    {
      if (this._browserTabs.Count == 0)
        return tabs.Count != 0;
      if (this._browserTabs.Count != tabs.Count)
        return true;
      int num = 0;
      foreach (BrowserTab tab in tabs)
      {
        if (!tab.SameAs(this._browserTabs[num++]))
          return true;
        if (num >= this._browserTabs.Count)
          return false;
      }
      return false;
    }

    private void SendResponse(HttpListenerContext context)
    {
      Queue<IWebBrowserCommand> pendingCommands = this._pendingCommands;
      string s = pendingCommands.Count != 0 ? pendingCommands.Dequeue().CreateCommandJson() : KeepAliveCommand.CreateCommandJson();
      try
      {
        HttpListenerResponse response = context.Response;
        response.ContentType = "application/json";
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        response.ContentLength64 = (long) bytes.Length;
        Stream outputStream = response.OutputStream;
        outputStream.Write(bytes, 0, bytes.Length);
        outputStream.Close();
      }
      catch (Exception ex)
      {
        WebBrowserModule.logger.Warn(ex, this.name + "; send response");
      }
    }

    private void StartListener()
    {
      WebBrowserModule.logger.Debug(this.name + " listener starting ...");
      this.listener = new HttpListener();
      this.listener.Prefixes.Add(this.uriPrefix);
      try
      {
        this.listener.Start();
      }
      catch (HttpListenerException ex)
      {
        WebBrowserModule.logger.Warn((Exception) ex, this.name + " listener start failed");
        return;
      }
      WebBrowserModule.logger.Debug(this.name + " listener started");
      try
      {
        this.listener.BeginGetContext(this.asyncCallback, (object) null);
      }
      catch (HttpListenerException ex)
      {
        WebBrowserModule.logger.Warn((Exception) ex, this.name + " begin get context failed");
      }
    }
  }
}
