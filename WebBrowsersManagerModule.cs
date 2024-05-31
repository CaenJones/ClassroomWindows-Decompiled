// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.WebBrowsersManagerModule
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

#nullable disable
namespace ClassroomWindows
{
  public sealed class WebBrowsersManagerModule : IModule
  {
    private double stateTime;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static WebBrowsersManagerModule s_instance = (WebBrowsersManagerModule) null;
    private static readonly object LOCK = new object();
    private List<WebBrowserModule> webBrowserModules = new List<WebBrowserModule>();
    private BrowserTab currentlyBrowsing;
    private WebBrowserModule chromeModule = new WebBrowserModule("Google Chrome", "chrome", 61337, new AsyncCallback(WebBrowsersManagerModule.ChromeProcessRequestCallback));
    private WebBrowserModule edgeModule = new WebBrowserModule("Microsoft Edge", "msedge", 62337, new AsyncCallback(WebBrowsersManagerModule.EdgeProcessRequestCallback));

    public static WebBrowsersManagerModule Instance
    {
      get
      {
        lock (WebBrowsersManagerModule.LOCK)
        {
          if (WebBrowsersManagerModule.s_instance == null)
            WebBrowsersManagerModule.s_instance = new WebBrowsersManagerModule();
          return WebBrowsersManagerModule.s_instance;
        }
      }
    }

    private WebBrowsersManagerModule()
    {
      this.webBrowserModules.Add(this.chromeModule);
      this.webBrowserModules.Add(this.edgeModule);
    }

    public void GetCurrentlyBrowsing(out string url, out string favicon, out string browser)
    {
      BrowserTab currentlyBrowsing = this.currentlyBrowsing;
      if (currentlyBrowsing == null)
      {
        url = "";
        favicon = "";
        browser = "";
      }
      else
      {
        url = currentlyBrowsing.url;
        favicon = currentlyBrowsing.favicon;
        browser = currentlyBrowsing.browserName;
      }
    }

    public string GetCurrentlyBrowsingUrl()
    {
      BrowserTab currentlyBrowsing = this.currentlyBrowsing;
      return currentlyBrowsing != null ? currentlyBrowsing.url : "";
    }

    public void SetCurrentlyBrowsing(BrowserTab browserTab) => this.currentlyBrowsing = browserTab;

    private void SetCurrentlyBrowsingToFirstActive()
    {
      foreach (WebBrowserModule webBrowserModule in this.webBrowserModules)
      {
        foreach (BrowserTab tab in webBrowserModule.Tabs)
        {
          if (tab.active)
          {
            this.currentlyBrowsing = tab;
            return;
          }
        }
      }
      this.currentlyBrowsing = (BrowserTab) null;
    }

    public void NoBrowserTabs(WebBrowserModule webBrowser)
    {
      BrowserTab currentlyBrowsing = this.currentlyBrowsing;
      if (currentlyBrowsing == null || !(currentlyBrowsing.browserName == webBrowser.name))
        return;
      WebBrowsersManagerModule.logger.Debug("Clearing currently browsing for closed browser \"" + webBrowser.name + "\"");
      webBrowser.Close();
      this.SetCurrentlyBrowsingToFirstActive();
    }

    public List<BrowserTab> GetTabs(bool details)
    {
      List<BrowserTab> tabs = new List<BrowserTab>();
      foreach (WebBrowserModule webBrowserModule in this.webBrowserModules)
      {
        if (webBrowserModule.Tabs.Count > 0)
        {
          if (details)
            WebBrowsersManagerModule.logger.Debug(string.Format("{0} web browser has {1} tab(s)", (object) webBrowserModule.name, (object) webBrowserModule.Tabs.Count));
          tabs.AddRange((IEnumerable<BrowserTab>) webBrowserModule.Tabs);
        }
        else if (details)
          WebBrowsersManagerModule.logger.Debug(webBrowserModule.name + " web browser no tabs");
      }
      return tabs;
    }

    public void CloseTabWithId(long tabId)
    {
      foreach (BrowserTab tab in this.GetTabs(true))
      {
        if ((long) tab.id == tabId)
          tab.module.CloseTabWithId(tabId);
      }
    }

    public void CloseTabWithUrl(string url)
    {
      foreach (BrowserTab tab in this.GetTabs(true))
      {
        WebBrowsersManagerModule.logger.Debug("tab URL to close = \"" + url + "\", tab URL = \"" + tab.url + "\"");
        if (url.Equals(tab.url))
        {
          WebBrowsersManagerModule.logger.Debug("Closing tab with URL = \"" + tab.url + "\" ...");
          tab.module.CloseTabWithUrl(url);
        }
      }
    }

    public void FocusTab(long tabId)
    {
      foreach (BrowserTab tab in this.GetTabs(true))
      {
        if ((long) tab.id == tabId)
          tab.module.FocusTab(tabId);
      }
    }

    public void NewTab(string url)
    {
      WebBrowsersManagerModule.logger.Debug("New Tab for \"" + url + "\"");
      Process.Start(url);
    }

    private void Launch() => throw new NotImplementedException();

    public void UpdateTab(long tabId, string url)
    {
      foreach (BrowserTab tab in this.GetTabs(false))
      {
        if ((long) tab.id == tabId)
          tab.module.UpdateTab(tabId, url);
      }
    }

    public void SetActiveURL(string url)
    {
      foreach (BrowserTab tab in this.GetTabs(false))
      {
        if (tab.active)
          tab.module.UpdateTab((long) tab.id, url);
      }
    }

    public WebBrowserModule GetWebBrowserModuleForPort(int port)
    {
      if (port == 61337)
        return this.chromeModule;
      return port == 62337 ? this.edgeModule : (WebBrowserModule) null;
    }

    public void OnUnlocked()
    {
      foreach (WebBrowserModule webBrowserModule in this.webBrowserModules)
        webBrowserModule.OnUnlocked();
    }

    public void Update(BackgroundWorker bgw, double deltaTime)
    {
      this.stateTime += deltaTime;
      if (this.stateTime < 0.5)
        return;
      this.stateTime = 0.0;
      this.chromeModule.Update(bgw, deltaTime);
      this.edgeModule.Update(bgw, deltaTime);
      foreach (WebBrowserModule webBrowserModule in this.webBrowserModules)
      {
        if (!webBrowserModule.IsOpen())
          this.NoBrowserTabs(webBrowserModule);
      }
    }

    public bool IsDirty()
    {
      foreach (WebBrowserModule webBrowserModule in this.webBrowserModules)
      {
        if (webBrowserModule.IsDirty)
          return true;
      }
      return false;
    }

    public void SetDirty()
    {
      foreach (WebBrowserModule webBrowserModule in this.webBrowserModules)
        webBrowserModule.IsDirty = true;
    }

    public void OnTabsPublished()
    {
      foreach (WebBrowserModule webBrowserModule in this.webBrowserModules)
        webBrowserModule.IsDirty = false;
    }

    public bool IsForeground(BrowserTab browserTab)
    {
      BrowserTab currentlyBrowsing = this.currentlyBrowsing;
      return currentlyBrowsing != null && currentlyBrowsing.SameAs(browserTab);
    }

    private static void ChromeProcessRequestCallback(IAsyncResult result)
    {
      WebBrowsersManagerModule.Instance.chromeModule.ProcessRequest(result);
    }

    private static void EdgeProcessRequestCallback(IAsyncResult result)
    {
      WebBrowsersManagerModule.Instance.edgeModule.ProcessRequest(result);
    }
  }
}
