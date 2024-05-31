// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.SystemTrayManager
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using ClassroomWindows.Properties;
using NLog;
using System;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace ClassroomWindows
{
  public sealed class SystemTrayManager
  {
    private static SystemTrayManager s_instance = (SystemTrayManager) null;
    private static readonly object LOCK = new object();
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private NotifyIcon _ni;
    private NotificationType _notificationType;
    private bool _hasRightClickedAtLeastOnce;

    public static SystemTrayManager Instance
    {
      get
      {
        lock (SystemTrayManager.LOCK)
        {
          if (SystemTrayManager.s_instance == null)
            SystemTrayManager.s_instance = new SystemTrayManager();
          return SystemTrayManager.s_instance;
        }
      }
    }

    public void Display()
    {
      this._ni.MouseClick += new MouseEventHandler(this.Ni_MouseClick);
      this._ni.Icon = Resources.icon_working;
      this._ni.Text = "Lightspeed Classroom";
      this._ni.Visible = true;
      this._ni.ContextMenuStrip = this.CreateContextMenuStrip();
    }

    public void DeleteIcon() => this._ni.Dispose();

    public void UpdateIcon(Icon icon) => this._ni.Icon = icon;

    public void DisplayBalloonTip(string tipText, NotificationType notificationType)
    {
      this._notificationType = notificationType;
      this._ni.BalloonTipClicked += new EventHandler(this.Ni_BalloonTipClicked);
      this._ni.BalloonTipText = tipText;
      this._ni.BalloonTipTitle = "Lightspeed Classroom";
      this._ni.ShowBalloonTip(60);
    }

    private ContextMenuStrip CreateContextMenuStrip()
    {
      ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
      ToolStripMenuItem toolStripMenuItem1 = new ToolStripMenuItem();
      toolStripMenuItem1.Text = "Done";
      ToolStripMenuItem toolStripMenuItem2 = toolStripMenuItem1;
      toolStripMenuItem2.Click += new EventHandler(this.Done_Click);
      toolStripMenuItem2.Image = (Image) Resources.Done;
      contextMenuStrip.Items.Add((ToolStripItem) toolStripMenuItem2);
      ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
      toolStripMenuItem3.Text = "Need Help";
      ToolStripMenuItem toolStripMenuItem4 = toolStripMenuItem3;
      toolStripMenuItem4.Click += new EventHandler(this.NeedHelp_Click);
      toolStripMenuItem4.Image = (Image) Resources.NeedHelp;
      contextMenuStrip.Items.Add((ToolStripItem) toolStripMenuItem4);
      ToolStripMenuItem toolStripMenuItem5 = new ToolStripMenuItem();
      toolStripMenuItem5.Text = "Working";
      ToolStripMenuItem toolStripMenuItem6 = toolStripMenuItem5;
      toolStripMenuItem6.Click += new EventHandler(this.Working_Click);
      toolStripMenuItem6.Image = (Image) Resources.Working;
      contextMenuStrip.Items.Add((ToolStripItem) toolStripMenuItem6);
      contextMenuStrip.Items.Add((ToolStripItem) new ToolStripSeparator());
      ToolStripMenuItem toolStripMenuItem7 = new ToolStripMenuItem();
      toolStripMenuItem7.Text = StudentPolicyApi.Instance.StudentPolicy.username;
      contextMenuStrip.Items.Add((ToolStripItem) toolStripMenuItem7);
      return contextMenuStrip;
    }

    private void Working_Click(object sender, EventArgs e)
    {
      if (AblyConnectionManager.Instance.CurrentPresenceState == PresenceState.NeedExtension)
        SystemTrayManager.logger.Debug("Cannot change Presence until missing Extension is installed");
      else
        AblyConnectionManager.Instance.SetPresenceStateFromString("working");
    }

    private void NeedHelp_Click(object sender, EventArgs e)
    {
      if (AblyConnectionManager.Instance.CurrentPresenceState == PresenceState.NeedExtension)
        SystemTrayManager.logger.Debug("Cannot change Presence until missing Extension is installed");
      else
        AblyConnectionManager.Instance.SetPresenceStateFromString("need help");
    }

    private void Done_Click(object sender, EventArgs e)
    {
      if (AblyConnectionManager.Instance.CurrentPresenceState == PresenceState.NeedExtension)
        SystemTrayManager.logger.Debug("Cannot change Presence until missing Extension is installed");
      else
        AblyConnectionManager.Instance.SetPresenceStateFromString("done");
    }

    private void Ni_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        this._hasRightClickedAtLeastOnce = true;
      }
      else
      {
        if (e.Button != MouseButtons.Left || !this._hasRightClickedAtLeastOnce)
          return;
        this._ni.ContextMenuStrip.Show();
      }
    }

    private void Ni_BalloonTipClicked(object sender, EventArgs e)
    {
      switch (this._notificationType)
      {
        case NotificationType.NeedHelp:
          AblyConnectionManager.Instance.SetPresenceStateFromString("need help");
          break;
        case NotificationType.Done:
          AblyConnectionManager.Instance.SetPresenceStateFromString("done");
          break;
        default:
          SystemTrayManager.logger.Warn<NotificationType>("No Handler for NotificationType {0}", this._notificationType);
          break;
      }
    }

    private SystemTrayManager() => this._ni = new NotifyIcon();

    ~SystemTrayManager() => this._ni.Dispose();
  }
}
