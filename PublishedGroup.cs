// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.PublishedGroup
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

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
