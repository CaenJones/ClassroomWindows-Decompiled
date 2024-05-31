// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.UserInfo
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using Microsoft.Win32;
using NLog;
using System;
using System.DirectoryServices.AccountManagement;

#nullable disable
namespace ClassroomWindows
{
  internal static class UserInfo
  {
    private const string keyPath = "Software\\Lightspeed Systems\\Classroom Agent";
    private const string regMail = "Mail";
    private const string regUpn = "UPN";
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static string _msaEmail = string.Empty;
    private static string _adEmail = string.Empty;
    private static string _adUpn = string.Empty;

    public static string msaEmail => UserInfo._msaEmail;

    public static string adEmail => UserInfo._adEmail;

    public static string adUpn => UserInfo._adUpn;

    private static bool IsDomainAccount()
    {
      return string.Compare(Environment.MachineName, Environment.UserDomainName) != 0;
    }

    private static string GetRegString(RegistryKey key, string valueName)
    {
      try
      {
        object regString = key.GetValue(valueName);
        if (regString.GetType() == typeof (string))
          return (string) regString;
      }
      catch (UnauthorizedAccessException ex)
      {
        UserInfo.logger.Error(string.Format("Unable to get registry value \"{0}\\{1}\". Access denied.", (object) key, (object) valueName));
      }
      catch
      {
        UserInfo.logger.Error(string.Format("Unable to get registry value \"{0}\\{1}\"", (object) key, (object) valueName));
      }
      return string.Empty;
    }

    private static void SetRegString(RegistryKey key, string valueName, string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        try
        {
          key.DeleteValue(valueName);
          UserInfo.logger.Debug(string.Format("Registry value \"{0}\\{1}\" deleted.", (object) key, (object) valueName));
        }
        catch (UnauthorizedAccessException ex)
        {
          UserInfo.logger.Error(string.Format("Unable to delete registry value \"{0}\\{1}\". Access denied.", (object) key, (object) valueName));
        }
        catch
        {
          UserInfo.logger.Error(string.Format("Unable to delete registry value \"{0}\\{1}\"", (object) key, (object) valueName));
        }
      }
      else
      {
        try
        {
          key.SetValue(valueName, (object) value);
          UserInfo.logger.Debug(string.Format("Saved \"{0}\" to registry value \"{1}\\{2}\".", (object) value, (object) key, (object) valueName));
        }
        catch (UnauthorizedAccessException ex)
        {
          UserInfo.logger.Error(string.Format("Unable to save \"{0}\" to registry value \"{1}\\{2}\". Access denied.", (object) value, (object) key, (object) valueName));
        }
        catch
        {
          UserInfo.logger.Error(string.Format("Unable to save \"{0}\" to registry value \"{1}\\{2}\"", (object) value, (object) key, (object) valueName));
        }
      }
    }

    private static void GetCachedAd()
    {
      RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Lightspeed Systems\\Classroom Agent");
      if (key == null)
        return;
      UserInfo._adEmail = UserInfo.GetRegString(key, "Mail");
      UserInfo._adUpn = UserInfo.GetRegString(key, "UPN");
      UserInfo.logger.Info("Using cached Active Directory data\n\tAD email = " + UserInfo._adEmail + "\n\tUPN = " + UserInfo._adUpn);
    }

    private static void CacheAd()
    {
      RegistryKey subKey = Registry.CurrentUser.CreateSubKey("Software\\Lightspeed Systems\\Classroom Agent");
      if (subKey == null)
        return;
      UserInfo.SetRegString(subKey, "Mail", UserInfo._adEmail);
      UserInfo.SetRegString(subKey, "UPN", UserInfo._adUpn);
      UserInfo.logger.Info("Caching Active Directory data\n\temail = " + UserInfo._adEmail + "\n\tUPN = " + UserInfo._adUpn);
    }

    public static void Get()
    {
      UtilLibrary utilLibrary = new UtilLibrary();
      string microsoftAccountEmail;
      if (utilLibrary.Load() && utilLibrary.GetMicrosoftAccountEmail(out microsoftAccountEmail))
      {
        UserInfo.logger.Debug("Microsoft account email = " + microsoftAccountEmail);
        UserInfo._msaEmail = microsoftAccountEmail;
      }
      if (!UserInfo.IsDomainAccount())
        return;
      try
      {
        UserPrincipal current = UserPrincipal.Current;
        if (current != null)
        {
          UserInfo._adEmail = current.EmailAddress;
          UserInfo._adUpn = current.UserPrincipalName;
          UserInfo.CacheAd();
          return;
        }
      }
      catch (PrincipalServerDownException ex)
      {
        Console.WriteLine("Unable to get Active Directory information; server not available. " + ex.Message);
      }
      catch (NoMatchingPrincipalException ex)
      {
        Console.WriteLine("Unable to get Active Directory information; no matching principal. " + ex.Message);
      }
      catch (InvalidOperationException ex)
      {
        Console.WriteLine("Unable to get Active Directory information; invalid operation. " + ex.Message);
      }
      catch
      {
        Console.WriteLine("Unable to get Active Directory information; unexpected exception.");
      }
      UserInfo.GetCachedAd();
    }
  }
}
