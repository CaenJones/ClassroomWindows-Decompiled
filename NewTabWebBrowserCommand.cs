
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
