// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.FocusTabWebBrowserCommand
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

#nullable disable
namespace ClassroomWindows
{
  public sealed class FocusTabWebBrowserCommand : IWebBrowserCommand
  {
    public readonly long _data;

    public FocusTabWebBrowserCommand(long data) => this._data = data;

    public string CreateCommandJson()
    {
      return string.Format("{{\"command\": \"focusTab\", \"data\": {0}}}", (object) this._data);
    }
  }
}
