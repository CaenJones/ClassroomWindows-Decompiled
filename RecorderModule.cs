// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.RecorderModule
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;

#nullable disable
namespace ClassroomWindows
{
  public sealed class RecorderModule : IModule
  {
    private static RecorderModule s_instance = (RecorderModule) null;
    private static readonly object LOCK = new object();
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static readonly int NUM_SECONDS_UNTIL_PRESIGNED_URL_EXPIRES = 3600;
    private static readonly string SESSION_ID = Guid.NewGuid().ToString();
    private static readonly string CHARS_FOR_BASE_36_ENCODE = "0123456789abcdefghijklmnopqrstuvwxyz";
    private static readonly string IMAGE_FILE_NAME = "image.jpg";
    private static readonly string TABS_FILE_NAME = "tabs.json";
    private static readonly string META_FILE_NAME = "meta.json";
    private double _stateTime;
    private double _timeRemainingToRecord;
    private double _timeRemainingUntilPresignedUrlExpires;
    private RecorderApiDetails _recorderApiDetails;

    public static RecorderModule Instance
    {
      get
      {
        lock (RecorderModule.LOCK)
        {
          if (RecorderModule.s_instance == null)
            RecorderModule.s_instance = new RecorderModule();
          return RecorderModule.s_instance;
        }
      }
    }

    public void StartRecording(int howManySeconds)
    {
      this._stateTime = 0.0;
      this._timeRemainingToRecord = 0.0;
      if (howManySeconds <= 0)
      {
        AblyConnectionManager.Instance.Recording = false;
      }
      else
      {
        this._timeRemainingToRecord = (double) howManySeconds;
        this._stateTime = 10.0;
        AblyConnectionManager.Instance.Recording = true;
      }
    }

    public void Update(BackgroundWorker backgroundWorker, double deltaTime)
    {
      this._stateTime += deltaTime;
      this._timeRemainingToRecord -= deltaTime;
      this._timeRemainingUntilPresignedUrlExpires -= deltaTime;
      if (!AblyConnectionManager.Instance.Recording)
        return;
      if (this._timeRemainingUntilPresignedUrlExpires <= 0.0)
        this.FetchPresignedUrl();
      if (this._stateTime >= 10.0)
      {
        this._stateTime = 0.0;
        this.UploadScreenshotAndTabs();
      }
      if (this._timeRemainingToRecord > 0.0)
        return;
      AblyConnectionManager.Instance.Recording = false;
    }

    private RecorderModule()
    {
    }

    private void FetchPresignedUrl()
    {
      this._recorderApiDetails = (RecorderApiDetails) null;
      RecorderModule.logger.Debug("Request URL " + ConfigManager.Instance.Config.recorderApiURL);
      RestClient restClient = new RestClient(ConfigManager.Instance.Config.recorderApiURL);
      RestRequest restRequest1 = new RestRequest((Method) 1);
      restRequest1.AddHeader("Cache-Control", "no-cache");
      restRequest1.AddHeader("customerId", ConfigManager.Instance.Config.customerID);
      restRequest1.AddHeader("version", Global.versionHeader);
      restRequest1.AddHeader("x-api-key", ConfigManager.Instance.Config.apiKey);
      restRequest1.AddHeader("Content-Type", "application/json");
      RestRequest restRequest2 = restRequest1;
      IRestResponse irestResponse = restClient.Execute((IRestRequest) restRequest2);
      if (!irestResponse.IsSuccessful || irestResponse.StatusCode != HttpStatusCode.OK)
      {
        RecorderModule.logger.Warn("Unable to get presigned URL for the record API, " + Global.HttpStatusText(irestResponse.StatusCode));
        AblyConnectionManager.Instance.Recording = false;
      }
      else
      {
        this._recorderApiDetails = JsonConvert.DeserializeObject<RecorderApiDetails>(irestResponse.Content);
        this._timeRemainingUntilPresignedUrlExpires = (double) RecorderModule.NUM_SECONDS_UNTIL_PRESIGNED_URL_EXPIRES;
      }
    }

    private void UploadScreenshotAndTabs()
    {
      if (!IpApi.Instance.OnCampus)
        return;
      if (this._recorderApiDetails == null)
      {
        RecorderModule.logger.Warn("Presigned URL for the record API is missing, unable to upload screenshot and tabs");
      }
      else
      {
        DateTime dateTime = DateTime.Now;
        string str1 = string.Format("{0}{1}/{2}/{3}", (object) this._recorderApiDetails.prefix, (object) this.EncodeToBase36(dateTime.Ticks / 10000000L), (object) StudentPolicyApi.Instance.StudentPolicy.username, (object) RecorderModule.SESSION_ID);
        string key1 = string.Format("{0}/{1}", (object) str1, (object) RecorderModule.IMAGE_FILE_NAME);
        string key2 = string.Format("{0}/{1}", (object) str1, (object) RecorderModule.TABS_FILE_NAME);
        string key3 = string.Format("{0}/{1}", (object) str1, (object) RecorderModule.META_FILE_NAME);
        string str2 = JsonConvert.SerializeObject((object) AblyConnectionManager.Instance.GetBrowserTabs());
        RecorderModule.logger.Debug("Recorder tab data:\n" + Global.PrettyJson(str2));
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        dictionary["un"] = StudentPolicyApi.Instance.StudentPolicy.username;
        dateTime = DateTime.UtcNow;
        dictionary["at"] = dateTime.ToString("o");
        dictionary["tKey"] = key2;
        dictionary["iKey"] = key1;
        dictionary["sId"] = RecorderModule.SESSION_ID;
        string str3 = JsonConvert.SerializeObject((object) dictionary);
        RecorderModule.logger.Debug("Recorder meta data:\n" + Global.PrettyJson(str3));
        using (MemoryStream memoryStream = new MemoryStream())
        {
          using (Image image = this.CaptureScreen())
          {
            image.Save((Stream) memoryStream, ImageFormat.Jpeg);
            byte[] array = memoryStream.ToArray();
            this.UploadFileToServer(new Uri(this._recorderApiDetails.presigned.url), this.CreateDictionaryForKey(key1), array, (string) null);
          }
        }
        this.UploadFileToServer(new Uri(this._recorderApiDetails.presigned.url), this.CreateDictionaryForKey(key2), (byte[]) null, str2);
        this.UploadFileToServer(new Uri(this._recorderApiDetails.presigned.url), this.CreateDictionaryForKey(key3), (byte[]) null, str3);
      }
    }

    private Dictionary<string, string> CreateDictionaryForKey(string key)
    {
      return new Dictionary<string, string>()
      {
        [nameof (key)] = key,
        ["X-Amz-Credential"] = this._recorderApiDetails.presigned.fields.XAmzCredential,
        ["X-Amz-Security-Token"] = this._recorderApiDetails.presigned.fields.XAmzSecurityToken,
        ["Policy"] = this._recorderApiDetails.presigned.fields.Policy,
        ["X-Amz-Signature"] = this._recorderApiDetails.presigned.fields.XAmzSignature,
        ["bucket"] = this._recorderApiDetails.presigned.fields.bucket,
        ["X-Amz-Algorithm"] = this._recorderApiDetails.presigned.fields.XAmzAlgorithm,
        ["X-Amz-Date"] = this._recorderApiDetails.presigned.fields.XAmzDate
      };
    }

    private void UploadFileToServer(
      Uri uri,
      Dictionary<string, string> data,
      byte[] binaryFileData,
      string stringFileData)
    {
      string boundary = "Boundary-" + RecorderModule.SESSION_ID;
      HttpWebRequest state = (HttpWebRequest) WebRequest.Create(uri);
      state.ContentType = "multipart/form-data; boundary=" + boundary;
      state.Method = "POST";
      state.BeginGetRequestStream((AsyncCallback) (result =>
      {
        try
        {
          HttpWebRequest request = (HttpWebRequest) result.AsyncState;
          using (Stream requestStream = request.EndGetRequestStream(result))
            this.WriteMultipartForm(requestStream, boundary, data, binaryFileData, stringFileData);
          request.BeginGetResponse((AsyncCallback) (a =>
          {
            try
            {
              WebResponse response = request.EndGetResponse(a);
              using (new StreamReader(response.GetResponseStream()))
              {
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                  string end = streamReader.ReadToEnd();
                  RecorderModule.logger.Debug("File Upload Response {0}", end);
                }
              }
            }
            catch (Exception ex)
            {
              RecorderModule.logger.Error<Exception>(ex);
            }
          }), (object) null);
        }
        catch (Exception ex)
        {
          RecorderModule.logger.Error<Exception>(ex);
        }
      }), (object) state);
    }

    private void WriteMultipartForm(
      Stream stream,
      string boundary,
      Dictionary<string, string> data,
      byte[] binaryFileData,
      string stringFileData)
    {
      if (data == null)
        throw new ArgumentNullException(nameof (data));
      byte[] bytes = Encoding.UTF8.GetBytes("--" + boundary);
      string format1 = "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
      string format2 = "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n";
      foreach (string key in data.Keys)
      {
        this.WriteToStream(stream, bytes);
        this.WriteToStream(stream, string.Format(format1, (object) key, (object) data[key]));
        this.WriteToStream(stream, "\r\n");
      }
      this.WriteToStream(stream, bytes);
      if (binaryFileData != null)
      {
        this.WriteToStream(stream, string.Format(format2, (object) "file"));
        this.WriteToStream(stream, binaryFileData);
      }
      else
        this.WriteToStream(stream, string.Format(format1, (object) "file", (object) stringFileData));
      this.WriteToStream(stream, "\r\n");
      this.WriteToStream(stream, bytes);
    }

    private void WriteToStream(Stream stream, string text)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(text);
      stream.Write(bytes, 0, bytes.Length);
    }

    private void WriteToStream(Stream stream, byte[] bytes) => stream.Write(bytes, 0, bytes.Length);

    private byte[] GetBytes(string filePath) => System.IO.File.ReadAllBytes(filePath);

    private string EncodeToBase36(long input)
    {
      if (input < 0L)
        throw new ArgumentOutOfRangeException(nameof (input), (object) input, "input cannot be negative");
      char[] charArray = RecorderModule.CHARS_FOR_BASE_36_ENCODE.ToCharArray();
      Stack<char> charStack = new Stack<char>();
      for (; input != 0L; input /= 36L)
        charStack.Push(charArray[input % 36L]);
      return new string(charStack.ToArray());
    }

    private Image CaptureScreen() => this.CaptureWindow(User32.GetDesktopWindow());

    private void GetWindowSize(IntPtr window, out int width, out int height)
    {
      User32.RECT rect;
      User32.GetWindowRect(window, out rect);
      width = rect.right - rect.left;
      height = rect.bottom - rect.top;
    }

    private Image CaptureWindow(IntPtr window)
    {
      int width;
      int height;
      Win32.GetDesktopSize(out width, out height);
      IntPtr windowDc = User32.GetWindowDC(window);
      IntPtr compatibleDc = Gdi32.CreateCompatibleDC(windowDc);
      IntPtr compatibleBitmap = Gdi32.CreateCompatibleBitmap(windowDc, width, height);
      IntPtr objectHandle = Gdi32.SelectObject(compatibleDc, compatibleBitmap);
      Gdi32.BitBlt(compatibleDc, 0, 0, width, height, windowDc, 0, 0, 13369376);
      Image image = (Image) Image.FromHbitmap(compatibleBitmap);
      Gdi32.SelectObject(compatibleDc, objectHandle);
      Gdi32.DeleteDC(compatibleDc);
      User32.ReleaseDC(window, windowDc);
      Gdi32.DeleteObject(compatibleBitmap);
      return image;
    }
  }
}
