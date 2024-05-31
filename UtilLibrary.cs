// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.UtilLibrary
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using NLog;
using System;
using System.Runtime.InteropServices;

#nullable disable
namespace ClassroomWindows
{
  public class UtilLibrary
  {
    private const string name = "Util.dll";
    private const string GetMicrosoftAccountEmailProc = "GetMicrosoftAccountEmail";
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private IntPtr module = IntPtr.Zero;

    ~UtilLibrary() => this.Free();

    public bool Load()
    {
      this.module = Win32.LoadLibrary("Util.dll");
      if (this.module == IntPtr.Zero)
      {
        UtilLibrary.logger.Error("Unable to load \"Util.dll\". " + Global.Win32ErrorMessage());
        return true;
      }
      UtilLibrary.logger.Info("Library \"Util.dll\" loaded");
      return true;
    }

    public void Free()
    {
      if (!(this.module != IntPtr.Zero))
        return;
      Win32.FreeLibrary(this.module);
    }

    private string UnmarshalString(IntPtr valuePtr)
    {
      if (valuePtr == IntPtr.Zero)
        return string.Empty;
      string stringUni = Marshal.PtrToStringUni(valuePtr);
      Marshal.FreeCoTaskMem(valuePtr);
      return stringUni;
    }

    public bool GetMicrosoftAccountEmail(out string microsoftAccountEmail)
    {
      if (this.module != IntPtr.Zero)
      {
        IntPtr procAddress = Win32.GetProcAddress(this.module, nameof (GetMicrosoftAccountEmail));
        if (procAddress != IntPtr.Zero)
        {
          IntPtr microsoftAccountEmail1;
          if (((UtilLibrary.GetMicrosoftAccountEmailDelegate) Marshal.GetDelegateForFunctionPointer(procAddress, typeof (UtilLibrary.GetMicrosoftAccountEmailDelegate)))(out microsoftAccountEmail1))
          {
            microsoftAccountEmail = this.UnmarshalString(microsoftAccountEmail1);
            return true;
          }
          UtilLibrary.logger.Error("Unable to get Microsoft account email. " + Global.Win32ErrorMessage());
        }
        else
          UtilLibrary.logger.Error("Procedure \"GetMicrosoftAccountEmail\" not found");
      }
      microsoftAccountEmail = string.Empty;
      return false;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate bool GetMicrosoftAccountEmailDelegate(out IntPtr microsoftAccountEmail);
  }
}
