// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.Program
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using Microsoft.Win32;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

#nullable disable
namespace ClassroomWindows
{
  internal static class Program
  {
    public static bool sessionLocked = false;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static string instanceName = string.Format("Lightspeed Classroom Agent Session {0}", (object) Process.GetCurrentProcess().SessionId);
    private static readonly Mutex mutex = new Mutex(true, Program.instanceName);

    [STAThread]
    private static void Main(string[] arguments)
    {
      Program.logger.Info(Program.instanceName + ", Version " + Global.GetAppVersion());
      int width;
      int height;
      ClassroomWindows.Win32.GetVirtualScreenSize(out width, out height);
      Program.logger.Debug(string.Format("Virtual screen size: {0} x {1}", (object) width, (object) height));
      ClassroomWindows.Win32.GetDesktopSize(out width, out height);
      Program.logger.Debug(string.Format("Desktop size: {0} x {1}", (object) width, (object) height));
      if (!Arguments.Parse(arguments))
        return;
      if (!ConfigManager.Instance.Init())
        Program.logger.Error("Unable to initialize configuration");
      else if (Arguments.Lock)
      {
        LockScreen.Lock(Arguments.TeacherName);
        if (Arguments.LockSeconds > 0)
        {
          Program.logger.Debug("Unlocking screen in " + Global.SingularPlural(Arguments.LockSeconds, "second") + " ...");
          Thread.Sleep(Arguments.LockSeconds * 1000);
        }
        LockScreen.Unlock();
      }
      else if (!Program.mutex.WaitOne(TimeSpan.Zero, true))
      {
        Program.logger.Debug("Already running; exiting");
      }
      else
      {
        Program.logger.Debug("Initializing SSL ...");
        WebRtcNative.InitializeSSL();
        Program.logger.Debug("SSL Initialized");
        UserInfo.Get();
        Program.logger.Debug("Machine name = \"" + Environment.MachineName + "\"");
        Program.logger.Debug("Domain name = \"" + Environment.UserDomainName + "\"");
        Program.logger.Debug("User name = \"" + Environment.UserName + "\"");
        Program.logger.Debug("MSA email = \"" + UserInfo.msaEmail + "\"");
        Program.logger.Debug("AD email = \"" + UserInfo.adEmail + "\"");
        Program.logger.Debug("AD UPN = \"" + UserInfo.adUpn + "\"");
        SystemEvents.SessionSwitch += new SessionSwitchEventHandler(Program.SystemEvents_SessionSwitch);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        StudentPolicyApi.Instance.InitWithRetry();
        if (IpApi.Instance.Get())
          Program.logger.Info("External IP = " + IpApi.Instance.IpAddress);
        else
          Program.logger.Warn("Unable to fetch external IP");
        BackgroundWorker backgroundWorker = new BackgroundWorker();
        backgroundWorker.WorkerReportsProgress = true;
        backgroundWorker.WorkerSupportsCancellation = true;
        backgroundWorker.DoWork += new DoWorkEventHandler(Program.Bgw_DoWork);
        backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(Program.Bgw_ProgressChanged);
        backgroundWorker.RunWorkerAsync();
        SystemTrayManager.Instance.Display();
        Application.Run();
        Program.mutex.ReleaseMutex();
      }
    }

    private static void Bgw_DoWork(object sender, DoWorkEventArgs args)
    {
      Program.logger.Debug(nameof (Bgw_DoWork));
      BackgroundWorker bgw = (BackgroundWorker) sender;
      int num = 0;
      while (num < 3)
      {
        try
        {
          AblyConnectionManager.Instance.InitAsync().Wait();
          break;
        }
        catch (Exception ex)
        {
          Program.logger.Warn<Exception>(ex);
          ++num;
        }
      }
      IModule[] moduleArray = new IModule[5]
      {
        (IModule) RecorderModule.Instance,
        (IModule) WebBrowsersManagerModule.Instance,
        (IModule) AblyModule.Instance,
        (IModule) WebRTCModule.Instance,
        (IModule) ReportUrlsVistedModule.Instance
      };
      while (!bgw.CancellationPending)
      {
        Thread.Sleep(500);
        foreach (IModule module in moduleArray)
          module.Update(bgw, 0.5);
      }
    }

    private static void Bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
    }

    private static void InstallEdgeExtension()
    {
      string path = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EdgeExtension.Appx"));
      if (!File.Exists(path))
      {
        Program.logger.Error("Unable to locate edge extension file '{0}' - Cannot install.", path);
      }
      else
      {
        Runspace runspace = RunspaceFactory.CreateRunspace();
        runspace.Open();
        using (PowerShell powerShell = PowerShell.Create())
        {
          powerShell.Runspace = runspace;
          powerShell.AddCommand("Add-AppxPackage");
          powerShell.AddParameter("-Path", (object) path);
          Collection<PSObject> collection = powerShell.Invoke();
          Program.logger.Debug("Results Log from Installing Edge Extension:");
          foreach (PSObject psObject in collection)
            Program.logger.Debug(psObject.ToString());
        }
        runspace.Close();
      }
    }

    private static void SystemEvents_SessionSwitch(
      object sender,
      SessionSwitchEventArgs sessionEvent)
    {
      switch (sessionEvent.Reason)
      {
        case SessionSwitchReason.ConsoleConnect:
          Program.logger.Debug("Console connect");
          break;
        case SessionSwitchReason.ConsoleDisconnect:
          Program.logger.Debug("Console disconnect");
          break;
        case SessionSwitchReason.RemoteConnect:
          Program.logger.Debug("Remote connect");
          break;
        case SessionSwitchReason.RemoteDisconnect:
          Program.logger.Debug("Remote diconnect");
          break;
        case SessionSwitchReason.SessionLogon:
          Program.logger.Debug("Session logon");
          break;
        case SessionSwitchReason.SessionLogoff:
          Program.logger.Debug("Session logoff");
          break;
        case SessionSwitchReason.SessionLock:
          Program.logger.Debug("Session locked");
          Program.sessionLocked = true;
          break;
        case SessionSwitchReason.SessionUnlock:
          Program.logger.Debug("Session unlocked");
          Program.sessionLocked = false;
          WebBrowsersManagerModule.Instance.OnUnlocked();
          break;
        case SessionSwitchReason.SessionRemoteControl:
          Program.logger.Debug("Session remote control");
          break;
      }
    }

    public static void Exit()
    {
      IpApi.Instance.Stop();
      if (Application.MessageLoop)
      {
        Program.logger.Debug("Exit application ...");
        SystemTrayManager.Instance.DeleteIcon();
        Application.Exit();
      }
      else
      {
        Program.logger.Debug("Exit program ...");
        SystemTrayManager.Instance.DeleteIcon();
        Environment.Exit(1);
      }
    }
  }
}
