
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
