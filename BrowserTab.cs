
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
