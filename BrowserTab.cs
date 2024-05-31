// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.BrowserTab
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

#nullable disable
namespace ClassroomWindows
{
  public sealed class BrowserTab
  {
    public bool active;
    public string browserName;
    public string favicon;
    public int id;
    public string title;
    public string url;
    public int windowId;
    public WebBrowserModule module;

    public bool SameAs(BrowserTab other)
    {
      return this.active == other.active && this.browserName == other.browserName && this.favicon == other.favicon && this.id == other.id && this.title == other.title && this.url == other.url && this.windowId == other.windowId;
    }
  }
}
