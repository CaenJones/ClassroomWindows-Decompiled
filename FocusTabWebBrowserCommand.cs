
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
