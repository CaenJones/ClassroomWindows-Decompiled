// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.ConfigManager
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using Encryption_XOR;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Reflection;

#nullable disable
namespace ClassroomWindows
{
  public sealed class ConfigManager
  {
    private const string altCfgEnvVar = "CW_ALTERNATE_CFG2_FILE";
    private const string cfgFile = "classroom.cfg";
    private static ConfigManager instance = (ConfigManager) null;
    private static readonly object LOCK = new object();
    private Config _config;
    private string blockUrlPage;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public Config Config
    {
      get => this._config;
      set => this._config = value;
    }

    public static ConfigManager Instance
    {
      get
      {
        lock (ConfigManager.LOCK)
        {
          if (ConfigManager.instance == null)
            ConfigManager.instance = new ConfigManager();
          return ConfigManager.instance;
        }
      }
    }

    public bool Init()
    {
      if (this._config != null)
        return true;
      bool flag = false;
      string environmentVariable = Environment.GetEnvironmentVariable("CW_ALTERNATE_CFG2_FILE");
      string path;
      if (!string.IsNullOrEmpty(environmentVariable))
      {
        flag = true;
        char[] chArray = new char[1]{ '"' };
        string str = environmentVariable.Trim(chArray);
        ConfigManager.logger.Debug("Environment variable CW_ALTERNATE_CFG2_FILE = \"" + str + "\"");
        path = !Path.IsPathRooted(str) ? Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), str) : str;
      }
      else
        path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "classroom.cfg");
      if (File.Exists(path))
      {
        ConfigManager.logger.Debug("Configuration path = \"" + path + "\"");
        using (StreamReader streamReader = new StreamReader(path))
        {
          string end = streamReader.ReadToEnd();
          try
          {
            this._config = JsonConvert.DeserializeObject<Config>(end);
          }
          catch
          {
            ConfigManager.logger.Error("Invalid json \"" + end + "\"");
            return false;
          }
          if (this._config == null)
          {
            ConfigManager.logger.Error("Unable parse json \"" + end + "\"");
            return false;
          }
          ConfigManager.logger.Debug(Global.newLine + (this._config.useLogonID ? "Using ID the user logged on with" : "Using configuration user ID") + "," + Global.newLine + "Environment = " + this._config.environment);
          if (!flag)
          {
            this._config.apiKey = this.DecodeData(this._config.apiKey);
            this._config.customerID = this.DecodeData(this._config.customerID);
            this._config.auth = this.DecodeData(this._config.auth);
          }
          this.blockUrlPage = this._config.blockURL + "/block";
          return true;
        }
      }
      else
      {
        ConfigManager.logger.Debug("Configuration file \"" + path + "\" does not exist");
        return false;
      }
    }

    public bool IsBlockPage(string url) => url.StartsWith(this.blockUrlPage);

    public string GetBlockPage(string encrypted_customer_id, string group_guid)
    {
      if (string.IsNullOrEmpty(encrypted_customer_id))
        return this.blockUrlPage;
      return this.blockUrlPage + "?guid=" + group_guid + "&cId=" + encrypted_customer_id;
    }

    public string GetLockPage() => this._config.blockURL + "/lock";

    private string DecodeData(string encodedData) => EncryptorDecryptor.EncoderOne(encodedData);

    private ConfigManager()
    {
    }
  }
}
