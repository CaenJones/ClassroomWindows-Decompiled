// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.NewTabWebBrowserCommand
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

#nullable disable
namespace ClassroomWindows
{
  public sealed class NewTabWebBrowserCommand : IWebBrowserCommand
  {
    public readonly string _data;

    public NewTabWebBrowserCommand(string data) => this._data = data;

    public string CreateCommandJson()
    {
      return "{\"command\": \"newTab\", \"data\": \"" + this._data + "\"}";
    }
  }
}
