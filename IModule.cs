
using System.ComponentModel;

#nullable disable
namespace ClassroomWindows
{
  public interface IModule
  {
    void Update(BackgroundWorker bgw, double deltaTime);
  }
}
