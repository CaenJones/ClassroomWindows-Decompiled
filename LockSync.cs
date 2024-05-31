// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.LockSync
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using NLog;

#nullable disable
namespace ClassroomWindows
{
  public static class LockSync
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static readonly object LOCK = new object();
    private static bool acquired = false;

    public static bool Acquire()
    {
      lock (LockSync.LOCK)
      {
        if (LockSync.acquired)
        {
          LockSync.logger.Debug("already acquired");
          return false;
        }
        LockSync.acquired = true;
        LockSync.logger.Debug("acquired");
        return true;
      }
    }

    public static void Release()
    {
      lock (LockSync.LOCK)
      {
        if (LockSync.acquired)
        {
          LockSync.acquired = false;
          LockSync.logger.Debug("released");
        }
        else
          LockSync.logger.Debug("not acquired");
      }
    }
  }
}
