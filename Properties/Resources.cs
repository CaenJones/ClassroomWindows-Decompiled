using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

#nullable disable
namespace ClassroomWindows.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  public class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static ResourceManager ResourceManager
    {
      get
      {
        if (ClassroomWindows.Properties.Resources.resourceMan == null)
          ClassroomWindows.Properties.Resources.resourceMan = new ResourceManager("ClassroomWindows.Properties.Resources", typeof (ClassroomWindows.Properties.Resources).Assembly);
        return ClassroomWindows.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static CultureInfo Culture
    {
      get => ClassroomWindows.Properties.Resources.resourceCulture;
      set => ClassroomWindows.Properties.Resources.resourceCulture = value;
    }

    public static Bitmap Done
    {
      get => (Bitmap) ClassroomWindows.Properties.Resources.ResourceManager.GetObject(nameof (Done), ClassroomWindows.Properties.Resources.resourceCulture);
    }

    public static Icon icon_app
    {
      get
      {
        return (Icon) ClassroomWindows.Properties.Resources.ResourceManager.GetObject(nameof (icon_app), ClassroomWindows.Properties.Resources.resourceCulture);
      }
    }

    public static Icon icon_done
    {
      get
      {
        return (Icon) ClassroomWindows.Properties.Resources.ResourceManager.GetObject(nameof (icon_done), ClassroomWindows.Properties.Resources.resourceCulture);
      }
    }

    public static Icon icon_need_help
    {
      get
      {
        return (Icon) ClassroomWindows.Properties.Resources.ResourceManager.GetObject(nameof (icon_need_help), ClassroomWindows.Properties.Resources.resourceCulture);
      }
    }

    public static Icon icon_working
    {
      get
      {
        return (Icon) ClassroomWindows.Properties.Resources.ResourceManager.GetObject(nameof (icon_working), ClassroomWindows.Properties.Resources.resourceCulture);
      }
    }

    public static string lock_screen_html
    {
      get
      {
        return ClassroomWindows.Properties.Resources.ResourceManager.GetString(nameof (lock_screen_html), ClassroomWindows.Properties.Resources.resourceCulture);
      }
    }

    public static string lock_screen_message
    {
      get
      {
        return ClassroomWindows.Properties.Resources.ResourceManager.GetString(nameof (lock_screen_message), ClassroomWindows.Properties.Resources.resourceCulture);
      }
    }

    public static Bitmap LockScreen
    {
      get
      {
        return (Bitmap) ClassroomWindows.Properties.Resources.ResourceManager.GetObject(nameof (LockScreen), ClassroomWindows.Properties.Resources.resourceCulture);
      }
    }

    public static Bitmap LockScreenBG
    {
      get
      {
        return (Bitmap) ClassroomWindows.Properties.Resources.ResourceManager.GetObject(nameof (LockScreenBG), ClassroomWindows.Properties.Resources.resourceCulture);
      }
    }

    public static string message_token
    {
      get => ClassroomWindows.Properties.Resources.ResourceManager.GetString(nameof (message_token), ClassroomWindows.Properties.Resources.resourceCulture);
    }

    public static Bitmap NeedHelp
    {
      get
      {
        return (Bitmap) ClassroomWindows.Properties.Resources.ResourceManager.GetObject(nameof (NeedHelp), ClassroomWindows.Properties.Resources.resourceCulture);
      }
    }

    public static Bitmap Working
    {
      get
      {
        return (Bitmap) ClassroomWindows.Properties.Resources.ResourceManager.GetObject(nameof (Working), ClassroomWindows.Properties.Resources.resourceCulture);
      }
    }
  }
}
