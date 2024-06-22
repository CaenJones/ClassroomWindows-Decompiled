
#nullable disable
namespace ClassroomWindows
{
  public class PublishedGroup
  {
    public string currentUrl;
    public string currentBrowser;
    public string currentFavicon;
    public int visits;
    public string mostViewed;

    public bool SameAs(PublishedGroup other)
    {
      return this.currentUrl == other.currentUrl && this.currentBrowser == other.currentBrowser && this.currentFavicon == other.currentFavicon && this.visits == other.visits && this.mostViewed == other.mostViewed;
    }
  }
}
