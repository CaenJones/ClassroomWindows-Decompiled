
using NLog;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

#nullable disable
namespace ClassroomWindows
{
  public class WebRtcNative : IDisposable
  {
    private const string dllName = "WebRtcNative.dll";
    private IntPtr conductor = IntPtr.Zero;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private WebRtcNative._OnRenderCallback _onRenderLocal;
    private WebRtcNative._OnRenderCallback _onRenderRemote;
    private WebRtcNative._OnErrorCallback _onError;
    private WebRtcNative._OnSuccessCallback _onSuccess;
    private WebRtcNative._OnFailureCallback _onFailure;
    private WebRtcNative._OnDataMessageCallback _onDataMessage;
    private WebRtcNative._OnDataBinaryMessageCallback _onDataBinaryMessage;
    private WebRtcNative._OnIceCandidateCallback _onIceCandidate;
    private WebRtcNative._OnSignalingChangeCallback _onSignalingChange;
    private WebRtcNative._OnIceConnectionChangeCallback _onIceConnectionChange;

    [DllImport("WebRtcNative.dll")]
    private static extern IntPtr NewConductor();

    [DllImport("WebRtcNative.dll")]
    private static extern void DeleteConductor(IntPtr conductor);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("WebRtcNative.dll")]
    public static extern void InitializeSSL();

    [SuppressUnmanagedCodeSecurity]
    [DllImport("WebRtcNative.dll")]
    public static extern void CleanupSSL();

    [DllImport("WebRtcNative.dll")]
    private static extern void onRenderLocal(IntPtr conductor, IntPtr callback);

    [DllImport("WebRtcNative.dll")]
    private static extern void onRenderRemote(IntPtr conductor, IntPtr callback);

    [DllImport("WebRtcNative.dll")]
    private static extern void onError(IntPtr conductor, IntPtr callback);

    [DllImport("WebRtcNative.dll")]
    private static extern void onSuccess(IntPtr conductor, IntPtr callback);

    [DllImport("WebRtcNative.dll")]
    private static extern void onFailure(IntPtr conductor, IntPtr callback);

    [DllImport("WebRtcNative.dll")]
    private static extern void onDataMessage(IntPtr conductor, IntPtr callback);

    [DllImport("WebRtcNative.dll")]
    private static extern void onDataBinaryMessage(IntPtr conductor, IntPtr callback);

    [DllImport("WebRtcNative.dll")]
    private static extern void onIceCandidate(IntPtr conductor, IntPtr callback);

    [DllImport("WebRtcNative.dll")]
    private static extern void onSignalingChange(IntPtr conductor, IntPtr callback);

    [DllImport("WebRtcNative.dll")]
    private static extern void onIceConnectionChange(IntPtr conductor, IntPtr callback);

    [DllImport("WebRtcNative.dll")]
    private static extern bool InitializePeerConnection(IntPtr conductor);

    public bool InitializePeerConnection() => WebRtcNative.InitializePeerConnection(this.conductor);

    [DllImport("WebRtcNative.dll")]
    private static extern void CreateOffer(IntPtr conductor);

    public void CreateOffer() => WebRtcNative.CreateOffer(this.conductor);

    [DllImport("WebRtcNative.dll")]
    private static extern void OnOfferReply(IntPtr conductor, string type, string sdp);

    public void OnOfferReply(string type, string sdp)
    {
      WebRtcNative.OnOfferReply(this.conductor, type, sdp);
    }

    [DllImport("WebRtcNative.dll")]
    private static extern bool AddIceCandidate(
      IntPtr conductor,
      string sdp_mid,
      int sdp_mlineindex,
      string sdp);

    public bool AddIceCandidate(string sdp_mid, int sdp_mlineindex, string sdp)
    {
      return WebRtcNative.AddIceCandidate(this.conductor, sdp_mid, sdp_mlineindex, sdp);
    }

    [DllImport("WebRtcNative.dll")]
    private static extern void CreateDataChannel(IntPtr conductor, string label);

    public void CreateDataChannel(string label)
    {
      WebRtcNative.CreateDataChannel(this.conductor, label);
    }

    [DllImport("WebRtcNative.dll")]
    private static extern void DataChannelSendText(IntPtr conductor, string text);

    public void DataChannelSendText(string text)
    {
      WebRtcNative.DataChannelSendText(this.conductor, text);
    }

    [DllImport("WebRtcNative.dll")]
    private static extern void DataChannelSendData(IntPtr conductor, byte[] data, int length);

    public void DataChannelSendData(byte[] data, int length)
    {
      WebRtcNative.DataChannelSendData(this.conductor, data, length);
    }

    [DllImport("WebRtcNative.dll")]
    [return: MarshalAs(UnmanagedType.SafeArray)]
    private static extern string[] GetVideoDevices();

    public static string[] VideoDevices() => WebRtcNative.GetVideoDevices();

    [DllImport("WebRtcNative.dll")]
    private static extern void SetVideoCapturer(
      IntPtr conductor,
      int width,
      int height,
      int caputureFps);

    public void SetVideoCapturer(int width, int height, int caputureFps)
    {
      WebRtcNative.SetVideoCapturer(this.conductor, width, height, caputureFps);
    }

    [SuppressUnmanagedCodeSecurity]
    [DllImport("WebRtcNative.dll")]
    private static extern void PushFrame(IntPtr conductor, IntPtr img, int type);

    public void PushFrame(IntPtr img, TJPF type = TJPF.TJPF_BGR)
    {
      WebRtcNative.PushFrame(this.conductor, img, (int) type);
    }

    [SuppressUnmanagedCodeSecurity]
    [DllImport("WebRtcNative.dll")]
    private static extern IntPtr CaptureFrameBGRX(IntPtr conductor, ref int width, ref int height);

    public IntPtr CaptureFrameBGRX(ref int width, ref int height)
    {
      return WebRtcNative.CaptureFrameBGRX(this.conductor, ref width, ref height);
    }

    [SuppressUnmanagedCodeSecurity]
    [DllImport("WebRtcNative.dll")]
    private static extern void CaptureFrameAndPush(IntPtr conductor);

    public void CaptureFrameAndPush() => WebRtcNative.CaptureFrameAndPush(this.conductor);

    private static string HexPointer(IntPtr pointer)
    {
      return string.Format("0x{0:x}", (object) pointer.ToInt64());
    }

    [DllImport("WebRtcNative.dll")]
    private static extern void SetAudio(IntPtr conductor, bool enable);

    public void SetAudio(bool enable) => WebRtcNative.SetAudio(this.conductor, enable);

    [DllImport("WebRtcNative.dll")]
    private static extern void AddServerConfig(
      IntPtr conductor,
      string uri,
      string username,
      string password);

    public void AddServerConfig(string uri, string username, string password)
    {
      WebRtcNative.AddServerConfig(this.conductor, uri, username, password);
    }

    [SuppressUnmanagedCodeSecurity]
    [DllImport("WebRtcNative.dll")]
    private static extern bool ProcessMessages(IntPtr conductor, int delay);

    public bool ProcessMessages(int delay) => WebRtcNative.ProcessMessages(this.conductor, delay);

    public event WebRtcNative.OnCallbackSdp OnSuccessOffer;

    public event WebRtcNative.OnCallbackSdp OnSuccessAnswer;

    public event WebRtcNative.OnCallbackSignalingChange OnSignalingChange;

    public event WebRtcNative.OnCallbackIceConnectionChange OnIceConnectionChange;

    public event WebRtcNative.OnCallbackIceCandidate OnIceCandidate;

    public event Action<string> OnError;

    public event WebRtcNative.OnCallbackError OnFailure;

    public event WebRtcNative.OnCallbackDataMessage OnDataMessage;

    public event WebRtcNative.OnCallbackDataBinaryMessage OnDataBinaryMessage;

    public event WebRtcNative.OnCallbackRender OnRenderLocal;

    public event WebRtcNative.OnCallbackRender OnRenderRemote;

    static WebRtcNative()
    {
      string str = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "WebRtcNative.dll");
      if (!File.Exists(str))
      {
        string message = "WebRtcNative: Cannot find DLL \"" + str + "\"";
        WebRtcNative.logger.Error(message);
        throw new Exception(message);
      }
      IntPtr pointer = Win32.LoadLibrary(str);
      if (pointer == IntPtr.Zero)
      {
        string message = "WebRtcNative: Unable to load \"" + str + "\". " + Global.Win32ErrorMessage();
        WebRtcNative.logger.Error(message);
        throw new Exception(message);
      }
      WebRtcNative.logger.Debug("\"" + str + "\" module = " + WebRtcNative.HexPointer(pointer));
    }

    public WebRtcNative()
    {
      this.conductor = WebRtcNative.NewConductor();
      WebRtcNative.logger.Trace("NewConductor = " + WebRtcNative.HexPointer(this.conductor));
      this._onRenderLocal = new WebRtcNative._OnRenderCallback(this._OnRenderLocal);
      WebRtcNative.onRenderLocal(this.conductor, Marshal.GetFunctionPointerForDelegate<WebRtcNative._OnRenderCallback>(this._onRenderLocal));
      this._onRenderRemote = new WebRtcNative._OnRenderCallback(this._OnRenderRemote);
      WebRtcNative.onRenderRemote(this.conductor, Marshal.GetFunctionPointerForDelegate<WebRtcNative._OnRenderCallback>(this._onRenderRemote));
      this._onError = new WebRtcNative._OnErrorCallback(this._OnError);
      WebRtcNative.onError(this.conductor, Marshal.GetFunctionPointerForDelegate<WebRtcNative._OnErrorCallback>(this._onError));
      this._onSuccess = new WebRtcNative._OnSuccessCallback(this._OnSuccess);
      WebRtcNative.onSuccess(this.conductor, Marshal.GetFunctionPointerForDelegate<WebRtcNative._OnSuccessCallback>(this._onSuccess));
      this._onFailure = new WebRtcNative._OnFailureCallback(this._OnFailure);
      WebRtcNative.onFailure(this.conductor, Marshal.GetFunctionPointerForDelegate<WebRtcNative._OnFailureCallback>(this._onFailure));
      this._onDataMessage = new WebRtcNative._OnDataMessageCallback(this._OnDataMessage);
      WebRtcNative.onDataMessage(this.conductor, Marshal.GetFunctionPointerForDelegate<WebRtcNative._OnDataMessageCallback>(this._onDataMessage));
      this._onDataBinaryMessage = new WebRtcNative._OnDataBinaryMessageCallback(this._OnDataBinaryMessage);
      WebRtcNative.onDataBinaryMessage(this.conductor, Marshal.GetFunctionPointerForDelegate<WebRtcNative._OnDataBinaryMessageCallback>(this._onDataBinaryMessage));
      this._onIceCandidate = new WebRtcNative._OnIceCandidateCallback(this._OnIceCandidate);
      WebRtcNative.onIceCandidate(this.conductor, Marshal.GetFunctionPointerForDelegate<WebRtcNative._OnIceCandidateCallback>(this._onIceCandidate));
      this._onSignalingChange = new WebRtcNative._OnSignalingChangeCallback(this._OnSignalingChange);
      WebRtcNative.onSignalingChange(this.conductor, Marshal.GetFunctionPointerForDelegate<WebRtcNative._OnSignalingChangeCallback>(this._onSignalingChange));
      this._onIceConnectionChange = new WebRtcNative._OnIceConnectionChangeCallback(this._OnIceConnectionChange);
      WebRtcNative.onIceConnectionChange(this.conductor, Marshal.GetFunctionPointerForDelegate<WebRtcNative._OnIceConnectionChangeCallback>(this._onIceConnectionChange));
    }

    public void Dispose()
    {
      WebRtcNative.logger.Trace("Conductor = " + WebRtcNative.HexPointer(this.conductor));
      WebRtcNative.DeleteConductor(this.conductor);
    }

    private void _OnRenderLocal(IntPtr BGR24, uint w, uint h) => this.OnRenderLocal(BGR24, w, h);

    private void _OnRenderRemote(IntPtr BGR24, uint w, uint h) => this.OnRenderRemote(BGR24, w, h);

    private void _OnError()
    {
      WebRtcNative.logger.Error("");
      this.OnError("webrtc error");
    }

    private void _OnSuccess(string type, string sdp)
    {
      WebRtcNative.logger.Debug("Type = " + type + ", SDP:" + Global.newLine + Global.IndentLines(sdp));
      switch (type)
      {
        case "offer":
          this.OnSuccessOffer(sdp);
          break;
        case "answer":
          this.OnSuccessAnswer(sdp);
          break;
      }
    }

    private void _OnSignalingChange(int state)
    {
      WebRtcNative.logger.Trace(WebRTCModule.SignalingStateText(state));
      this.OnSignalingChange(state);
    }

    private void _OnIceConnectionChange(int state)
    {
      WebRtcNative.logger.Trace(WebRTCModule.IceConnectionStateText(state));
      this.OnIceConnectionChange(state);
    }

    private void _OnIceCandidate(string sdpMid, int sdpMLineIndex, string sdp)
    {
      WebRtcNative.logger.Trace(Global.newLine + "SDP = \"" + sdp + "\"," + Global.newLine + "SDP media stream ID = " + sdpMid + "," + Global.newLine + string.Format("SDP m-line index = {0}", (object) sdpMLineIndex));
      this.OnIceCandidate(sdpMid, sdpMLineIndex, sdp);
    }

    private void _OnFailure(string error)
    {
      WebRtcNative.logger.Error("Error = " + error);
      this.OnFailure(error);
    }

    private void _OnDataMessage(string msg)
    {
      WebRtcNative.logger.Trace("");
      this.OnDataMessage(msg);
    }

    private void _OnDataBinaryMessage(IntPtr msg, uint size)
    {
      WebRtcNative.logger.Trace(string.Format("Size = {0}", (object) size));
      byte[] numArray = new byte[(int) size];
      Marshal.Copy(msg, numArray, 0, (int) size);
      this.OnDataBinaryMessage(numArray);
    }

    private delegate void _OnRenderCallback(IntPtr BGR24, uint w, uint h);

    private delegate void _OnErrorCallback();

    private delegate void _OnSuccessCallback(string type, string sdp);

    private delegate void _OnFailureCallback(string error);

    private delegate void _OnDataMessageCallback(string msg);

    private delegate void _OnDataBinaryMessageCallback(IntPtr msg, uint size);

    private delegate void _OnIceCandidateCallback(string sdp_mid, int sdp_mline_index, string sdp);

    private delegate void _OnSignalingChangeCallback(int state);

    private delegate void _OnIceConnectionChangeCallback(int state);

    public delegate void OnCallbackSdp(string sdp);

    public delegate void OnCallbackSignalingChange(int state);

    public delegate void OnCallbackIceConnectionChange(int state);

    public delegate void OnCallbackIceCandidate(string sdp_mid, int sdp_mline_index, string sdp);

    public delegate void OnCallbackError(string error);

    public delegate void OnCallbackDataMessage(string msg);

    public delegate void OnCallbackDataBinaryMessage(byte[] msg);

    public delegate void OnCallbackRender(IntPtr BGR24, uint w, uint h);
  }
}
