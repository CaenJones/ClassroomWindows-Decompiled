
using ClassroomWindows.Properties;
using NLog;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

#nullable disable
namespace ClassroomWindows
{
  internal class LockScreen : Form
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static LockScreen lockScreen;
    private static string lockMessage;
    private bool locked = true;
    private IContainer components;
    private WebBrowser webBrowser;

    public static void Lock(string lockMessage)
    {
      if (LockScreen.lockScreen != null)
        LockScreen.logger.Debug("Screen is already locked, ignoring Lock command...");
      else if (!LockSync.Acquire())
      {
        LockScreen.logger.Debug("Already locked, ignoring command ...");
      }
      else
      {
        LockScreen.lockMessage = lockMessage;
        LockScreen.logger.Debug("Creating lock screen thread ...");
        Thread thread = new Thread((ThreadStart) (() =>
        {
          LockScreen.lockScreen = new LockScreen();
          Application.Run((Form) LockScreen.lockScreen);
        }));
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
      }
    }

    public static void Unlock()
    {
      if (LockScreen.lockScreen == null || !LockScreen.lockScreen.locked)
        return;
      LockScreen.logger.Debug("Unlocking & closing form");
      LockScreen.lockScreen.Invoke((Delegate) (() =>
      {
        LockScreen.lockScreen.locked = false;
        LockScreen.lockScreen.Close();
        LockScreen.lockScreen = (LockScreen) null;
        LockSync.Release();
      }));
    }

    private string GetHtmlFromUrl(string url)
    {
      HttpWebResponse httpWebResponse = (HttpWebResponse) null;
      Stream stream = (Stream) null;
      StreamReader streamReader = (StreamReader) null;
      try
      {
        HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
        if (httpWebRequest != null)
        {
          httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
          try
          {
            httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse();
            if (httpWebResponse != null)
            {
              if (httpWebResponse.StatusCode == HttpStatusCode.OK)
              {
                try
                {
                  stream = httpWebResponse.GetResponseStream();
                  if (stream != null)
                  {
                    try
                    {
                      streamReader = new StreamReader(stream);
                      return streamReader.ReadToEnd();
                    }
                    catch (Exception ex)
                    {
                      LockScreen.logger.Debug("Unable to get stream reader. " + ex.Message);
                    }
                    finally
                    {
                      streamReader?.Dispose();
                    }
                  }
                }
                catch (Exception ex)
                {
                  LockScreen.logger.Debug("Unable to get response stream. " + ex.Message);
                }
                finally
                {
                  stream?.Dispose();
                }
              }
              else
                LockScreen.logger.Debug(string.Format("Unable to get \"{0}\". HTTP status = {1}", (object) url, (object) httpWebResponse.StatusCode));
            }
          }
          catch (WebException ex)
          {
            LockScreen.logger.Debug("Unable to get \"" + url + "\". " + ex.Message);
          }
          catch (Exception ex)
          {
            LockScreen.logger.Debug("Unable to get \"" + url + "\". " + ex.Message);
          }
          finally
          {
            httpWebResponse?.Dispose();
          }
        }
      }
      catch (Exception ex)
      {
        LockScreen.logger.Debug(string.Format("Unable to create web request. {0}", (object) ex));
      }
      return (string) null;
    }

    private string GetHtml()
    {
      return string.IsNullOrEmpty(LockScreen.lockMessage) ? Resources.lock_screen_html.Replace(Resources.message_token, Resources.lock_screen_message) : Resources.lock_screen_html.Replace(Resources.message_token, LockScreen.lockMessage);
    }

    public LockScreen()
    {
      this.InitializeComponent();
      this.ControlBox = false;
      this.FormClosing += new FormClosingEventHandler(this.FormClosingEvent);
      this.webBrowser.DocumentText = this.GetHtml();
      System.Threading.Timer timer = (System.Threading.Timer) null;
      timer = new System.Threading.Timer((TimerCallback) (state =>
      {
        LockScreen.Unlock();
        timer.Dispose();
      }), (object) null, 300000, -1);
    }

    private void LoadEvent(object sender, EventArgs e) => LockScreen.logger.Debug("");

    private void FormClosingEvent(object sender, FormClosingEventArgs eventArgs)
    {
      if (this.locked)
      {
        LockScreen.logger.Debug("Locked; Ignorning");
        eventArgs.Cancel = true;
      }
      else
        LockScreen.logger.Debug("Unlocked; Closing");
    }

    private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
    {
      LockScreen.logger.Debug("");
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (LockScreen));
      this.webBrowser = new WebBrowser();
      this.SuspendLayout();
      this.webBrowser.AllowNavigation = false;
      this.webBrowser.AllowWebBrowserDrop = false;
      this.webBrowser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.webBrowser.IsWebBrowserContextMenuEnabled = false;
      this.webBrowser.Location = new Point(0, 0);
      this.webBrowser.Margin = new Padding(0);
      this.webBrowser.MinimumSize = new Size(20, 20);
      this.webBrowser.Name = "webBrowser";
      this.webBrowser.ScriptErrorsSuppressed = true;
      this.webBrowser.ScrollBarsEnabled = false;
      this.webBrowser.Size = new Size(1366, 768);
      this.webBrowser.TabIndex = 0;
      this.webBrowser.WebBrowserShortcutsEnabled = false;
      this.webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(this.webBrowser_DocumentCompleted);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.BackColor = Color.Maroon;
      this.BackgroundImageLayout = ImageLayout.Stretch;
      this.ClientSize = new Size(1366, 768);
      this.Controls.Add((Control) this.webBrowser);
      this.DoubleBuffered = true;
      this.Font = new Font("Segoe UI", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.FormBorderStyle = FormBorderStyle.None;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (LockScreen);
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = "Lock Screen";
      this.TopMost = true;
      this.TransparencyKey = Color.Maroon;
      this.WindowState = FormWindowState.Maximized;
      this.Load += new EventHandler(this.LoadEvent);
      this.ResumeLayout(false);
    }
  }
}
