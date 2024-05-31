// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.UpdateTabWebBrowserCommand
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

#nullable disable
namespace ClassroomWindows
{
  public sealed class UpdateTabWebBrowserCommand : IWebBrowserCommand
  {
    public readonly long _data;
    public readonly string _data2;

    public UpdateTabWebBrowserCommand(long data, string data2)
    {
      this._data = data;
      this._data2 = data2;
    }

    public string CreateCommandJson()
    {
      return string.Format("{{\"command\": \"updateTab\", \"data\": {0}, \"data2\": \"{1}\"}}", (object) this._data, (object) this._data2);
    }
  }
}
