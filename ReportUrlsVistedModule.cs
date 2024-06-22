
using NLog;
using System;
using System.ComponentModel;

#nullable disable
namespace ClassroomWindows
{
  public sealed class ReportUrlsVistedModule : IModule, IDisposable
  {
    private const double resetInterval = 45.0;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static ReportUrlsVistedModule s_instance = (ReportUrlsVistedModule) null;
    private static readonly object LOCK = new object();
    private double stateTime;

    public static ReportUrlsVistedModule Instance
    {
      get
      {
        lock (ReportUrlsVistedModule.LOCK)
        {
          if (ReportUrlsVistedModule.s_instance == null)
            ReportUrlsVistedModule.s_instance = new ReportUrlsVistedModule();
          return ReportUrlsVistedModule.s_instance;
        }
      }
    }

    ~ReportUrlsVistedModule() => this.Dispose();

    public void Update(BackgroundWorker bgw, double deltaTime)
    {
      this.stateTime += deltaTime;
      if (this.stateTime < 45.0)
        return;
      StudentPolicyApi.Instance.ReportUrlsVisited();
      this.stateTime = 0.0;
    }

    public void Dispose()
    {
    }
  }
}
