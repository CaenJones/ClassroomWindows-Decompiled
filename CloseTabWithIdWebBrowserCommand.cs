// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.CloseTabWithIdWebBrowserCommand
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

#nullable disable
namespace ClassroomWindows
{
  public sealed class CloseTabWithIdWebBrowserCommand : IWebBrowserCommand
  {
    public readonly long _data;

    public CloseTabWithIdWebBrowserCommand(long data) => this._data = data;

    public string CreateCommandJson()
    {
      return string.Format("{{\"command\": \"closeTabWithId\", \"data\": {0}}}", (object) this._data);
    }
  }
}
