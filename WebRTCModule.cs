
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace ClassroomWindows
{
  public sealed class WebRTCModule : IDisposable, IModule
  {
    private static WebRTCModule s_instance = (WebRTCModule) null;
    private static readonly object LOCK = new object();
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private readonly ConcurrentDictionary<string, WebRTCModule.WebRtcSession> sessions = new ConcurrentDictionary<string, WebRTCModule.WebRtcSession>();
    private double _stateTime;
    private const int screenWidth = 1280;
    private const int screenHeight = 720;
    private const int captureFps = 5;
    private readonly byte[] imageBuffer = new byte[2764800];
    private IntPtr imageBufferPtr = IntPtr.Zero;
    private Bitmap image;
    private Graphics graphics;
    private int captureWidth;
    private int captureHeight;
    private readonly Dictionary<IntPtr, Bitmap> imgCapture = new Dictionary<IntPtr, Bitmap>();
    private WebRtcNative.OnCallbackRender OnRenderRemote;
    private WebRtcNative.OnCallbackRender OnRenderLocal;

    public static WebRTCModule Instance
    {
      get
      {
        lock (WebRTCModule.LOCK)
        {
          if (WebRTCModule.s_instance == null)
            WebRTCModule.s_instance = new WebRTCModule();
          return WebRTCModule.s_instance;
        }
      }
    }

    public void CreateSession(List<WebRTCModule.CustomIceParam> customIceParams, string sessionId)
    {
      WebRTCModule.logger.Debug("Session ID = " + sessionId);
      WebRTCModule.WebRtcSession webRtcSession = new WebRTCModule.WebRtcSession(sessionId, customIceParams);
      this.sessions[sessionId] = webRtcSession;
      webRtcSession.InitializePeerConnection();
    }

    public void SetAnswer(string sessionId, string sdp)
    {
      if (!this.sessions.ContainsKey(sessionId))
        return;
      this.sessions[sessionId].WebRtc.OnOfferReply("answer", sdp);
    }

    public void AddIceCandidate(string sessionId, string sdp_mid, int sdp_mline_index, string sdp)
    {
      if (!this.sessions.ContainsKey(sessionId))
        return;
      this.sessions[sessionId].WebRtc.AddIceCandidate(sdp_mid, sdp_mline_index, sdp);
    }

    public void Shutdown(string sessionId)
    {
      foreach (KeyValuePair<string, WebRTCModule.WebRtcSession> session in this.sessions)
      {
        if (sessionId.Equals(session.Key) && !session.Value.Cancel.IsCancellationRequested)
          session.Value.Cancel.Cancel();
      }
      this._stateTime = 0.0;
    }

    public void Update(BackgroundWorker bgw, double deltaTime)
    {
      if (this.sessions.Count == 0)
        return;
      this._stateTime += deltaTime;
      if (this._stateTime < 0.5)
        return;
      this._stateTime = 0.0;
      this.RemoveClosedSessions();
      this.PushFrame();
    }

    private void RemoveClosedSessions()
    {
      List<string> stringList = new List<string>();
      WebRTCModule.WebRtcSession webRtcSession;
      foreach (KeyValuePair<string, WebRTCModule.WebRtcSession> session in this.sessions)
      {
        webRtcSession = session.Value;
        if (webRtcSession.iceConnectionState >= WebRTCModule.IceConnectionState.Disconnected)
        {
          WebRTCModule.logger.Debug(string.Format("Session ID = {0}, {1}, removing ...", (object) webRtcSession.SessionId, (object) webRtcSession.iceConnectionState));
          stringList.Add(webRtcSession.SessionId);
        }
      }
      foreach (string key in stringList)
      {
        if (this.sessions.TryRemove(key, out webRtcSession))
        {
          WebRTCModule.logger.Debug("Session ID = " + webRtcSession.SessionId + " removed");
          if (!webRtcSession.Cancel.IsCancellationRequested)
          {
            WebRTCModule.logger.Debug("Session ID = " + webRtcSession.SessionId + " canceled");
            webRtcSession.Cancel.Cancel();
          }
        }
        else
          WebRTCModule.logger.Debug("Session ID = " + webRtcSession.SessionId + " already removed");
      }
    }

    private void PushFrame()
    {
      int count = this.sessions.Count;
      switch (count)
      {
        case 0:
          break;
        case 1:
          try
          {
            if (this.image == null)
            {
              this.imageBufferPtr = GCHandle.Alloc((object) this.imageBuffer, GCHandleType.Pinned).AddrOfPinnedObject();
              this.image = new Bitmap(1280, 720, 3840, PixelFormat.Format24bppRgb, this.imageBufferPtr);
              this.graphics = Graphics.FromImage((Image) this.image);
            }
            this.graphics.Clear(Color.DarkBlue);
            using (IEnumerator<KeyValuePair<string, WebRTCModule.WebRtcSession>> enumerator = this.sessions.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                WebRTCModule.WebRtcSession webRtcSession = enumerator.Current.Value;
                int width1 = -1;
                int height1 = -1;
                IntPtr num1 = webRtcSession.WebRtc.CaptureFrameBGRX(ref width1, ref height1);
                if (num1 != IntPtr.Zero)
                {
                  Bitmap bitmap = (Bitmap) null;
                  if (width1 != this.captureWidth || height1 != this.captureHeight || !this.imgCapture.TryGetValue(num1, out bitmap))
                  {
                    bitmap?.Dispose();
                    this.imgCapture[num1] = bitmap = new Bitmap(width1, height1, width1 * 4, PixelFormat.Format32bppRgb, num1);
                    this.captureWidth = width1;
                    this.captureHeight = height1;
                  }
                  float num2 = Math.Min(1280f / (float) this.captureWidth, 720f / (float) this.captureHeight);
                  int width2 = (int) ((double) this.captureWidth * (double) num2);
                  int height2 = (int) ((double) this.captureHeight * (double) num2);
                  this.graphics.DrawImage((Image) bitmap, (1280 - width2) / 2, (720 - height2) / 2, width2, height2);
                }
                WebRTCModule.logger.Trace("Session ID = " + webRtcSession.SessionId);
                webRtcSession.WebRtc.PushFrame(this.imageBufferPtr);
              }
              break;
            }
          }
          catch (Exception ex)
          {
            WebRTCModule.logger.Error<Exception>(ex);
            break;
          }
        default:
          WebRTCModule.logger.Debug(string.Format("{0} sessions", (object) count));
          goto case 1;
      }
    }

    public void Dispose()
    {
      try
      {
        foreach (KeyValuePair<string, WebRTCModule.WebRtcSession> session in this.sessions)
        {
          if (!session.Value.Cancel.IsCancellationRequested)
            session.Value.Cancel.Cancel();
        }
        this.sessions.Clear();
      }
      catch (Exception ex)
      {
        WebRTCModule.logger.Error<Exception>(ex);
      }
    }

    public void OnRenderRemoteDoNothing(IntPtr BGR24, uint w, uint h)
    {
    }

    public void OnRenderLocalDoNothing(IntPtr BGR24, uint w, uint h)
    {
    }

    private WebRTCModule()
    {
      this.OnRenderRemote = new WebRtcNative.OnCallbackRender(this.OnRenderRemoteDoNothing);
      this.OnRenderLocal = new WebRtcNative.OnCallbackRender(this.OnRenderLocalDoNothing);
    }

    public static string IceConnectionStateText(int state)
    {
      WebRTCModule.IceConnectionState iceConnectionState = (WebRTCModule.IceConnectionState) state;
      return iceConnectionState < WebRTCModule.IceConnectionState.Max ? iceConnectionState.ToString() : string.Format("Unknown ICE connection state = {0}", (object) state);
    }

    public static string SignalingStateText(int state)
    {
      WebRTCModule.SignalingState signalingState = (WebRTCModule.SignalingState) state;
      return signalingState < WebRTCModule.SignalingState.Max ? signalingState.ToString() : string.Format("Unknown signaling state = {0}", (object) state);
    }

    public static string IceGatheringStateText(int state)
    {
      WebRTCModule.IceGatheringState iceGatheringState = (WebRTCModule.IceGatheringState) state;
      return iceGatheringState < WebRTCModule.IceGatheringState.Max ? iceGatheringState.ToString() : string.Format("Unknown gathering state = {0}", (object) state);
    }

    public sealed class CustomIceParam
    {
      public string url;
      public string username;
      public string credential;
    }

    public enum SignalingState
    {
      Stable,
      HaveLocalOffer,
      HaveLocalPrAnswer,
      HaveRemoteOffer,
      HaveRemotePrAnswer,
      Closed,
      Max,
    }

    public enum IceGatheringState
    {
      New,
      Gathering,
      Complete,
      Max,
    }

    public enum IceConnectionState
    {
      New,
      Checking,
      Connected,
      Completed,
      Failed,
      Disconnected,
      Closed,
      Max,
    }

    public class WebRtcSession
    {
      private const int peerConnectTimeOut = 30000;
      private const int processMessagesDelay = 1000;
      public readonly WebRtcNative WebRtc;
      public readonly CancellationTokenSource Cancel;
      private string sessionId;
      private ManualResetEvent peerConnected;
      private List<WebRTCModule.CustomIceParam> customIceParams;
      public WebRTCModule.SignalingState signalingState;
      public WebRTCModule.IceConnectionState iceConnectionState;

      public string SessionId => this.sessionId;

      public WebRtcSession(string sessionId, List<WebRTCModule.CustomIceParam> customIceParams)
      {
        this.customIceParams = customIceParams;
        this.sessionId = sessionId;
        this.peerConnected = new ManualResetEvent(false);
        this.WebRtc = new WebRtcNative();
        this.Cancel = new CancellationTokenSource();
      }

      public void InitializePeerConnection()
      {
        WebRTCModule.logger.Debug("Session ID = " + this.SessionId + ", starting connect ...");
        Task.Factory.StartNew(new Action(this.PeerConnect), this.Cancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        WebRTCModule.logger.Debug("Session ID = " + this.SessionId + ", waiting for connection ...");
        if (this.peerConnected.WaitOne(30000))
        {
          WebRTCModule.logger.Debug("Session ID = " + this.SessionId + ", connected");
          this.WebRtc.OnSignalingChange += (WebRtcNative.OnCallbackSignalingChange) (state =>
          {
            this.signalingState = (WebRTCModule.SignalingState) state;
            WebRTCModule.logger.Debug("OnSignalingChange: Session ID = " + this.sessionId + ", " + WebRTCModule.SignalingStateText(state));
          });
          this.WebRtc.OnIceConnectionChange += (WebRtcNative.OnCallbackIceConnectionChange) (state =>
          {
            this.iceConnectionState = (WebRTCModule.IceConnectionState) state;
            WebRTCModule.logger.Debug("OnIceConnectionChange: Session ID = " + this.sessionId + ", " + WebRTCModule.IceConnectionStateText(state));
          });
          this.WebRtc.OnIceCandidate += (WebRtcNative.OnCallbackIceCandidate) ((sdpMid, sdpMLineIndex, sdp) =>
          {
            WebRTCModule.logger.Debug("OnIceCandidate: Session ID = " + this.sessionId + "," + Global.newLine + "SDP = \"" + sdp + "\"," + Global.newLine + "SDP media stream ID = " + sdpMid + "," + Global.newLine + string.Format("SDP m-line index = {0}", (object) sdpMLineIndex));
            AblyConnectionManager.Instance.offer_rtc_ice(this.sessionId, sdp, sdpMid, sdpMLineIndex);
          });
          this.WebRtc.OnSuccessAnswer += (WebRtcNative.OnCallbackSdp) (sdp => WebRTCModule.logger.Debug("OnSuccessAnswer: Session ID = " + this.sessionId + ", SDP:" + Global.newLine + Global.IndentLines(sdp)));
          this.WebRtc.OnSuccessOffer += (WebRtcNative.OnCallbackSdp) (sdp =>
          {
            WebRTCModule.logger.Debug("OnSuccessOffer: Session ID = " + this.sessionId + ", SDP:" + Global.newLine + Global.IndentLines(sdp));
            AblyConnectionManager.Instance.offer_rtc(this.sessionId, sdp);
          });
          this.WebRtc.OnFailure += (WebRtcNative.OnCallbackError) (error => WebRTCModule.logger.Error("OnFailure: Session ID = " + this.sessionId + ", failure = " + error));
          this.WebRtc.OnError += (Action<string>) (error =>
          {
            WebRTCModule.logger.Error("OnError: Session ID = " + this.sessionId + ", error = " + error);
            WebRTCModule.Instance.Shutdown(this.sessionId);
          });
          this.WebRtc.OnDataMessage += (WebRtcNative.OnCallbackDataMessage) (message => WebRTCModule.logger.Trace("OnDataMessage " + message));
          this.WebRtc.OnDataBinaryMessage += (WebRtcNative.OnCallbackDataBinaryMessage) (binaryMessage => WebRTCModule.logger.Trace("OnDataBinaryMessage: Session ID = " + this.sessionId + ", " + string.Format("length = {0}", (object) binaryMessage.Length)));
          this.WebRtc.OnRenderRemote += (WebRtcNative.OnCallbackRender) ((BGR24, w, h) => WebRTCModule.logger.Trace("OnRenderRemote: Session ID = " + this.sessionId));
          this.WebRtc.OnRenderLocal += (WebRtcNative.OnCallbackRender) ((BGR24, w, h) => WebRTCModule.logger.Trace("OnRenderLocal: Session ID = " + this.sessionId));
          WebRTCModule.logger.Debug("Session ID = " + this.SessionId + ", CreateOffer");
          this.WebRtc.CreateOffer();
        }
        else
          WebRTCModule.logger.Warn("Session ID = " + this.sessionId + ", connect time-out");
      }

      private void PeerConnect()
      {
        using (this.WebRtc)
        {
          foreach (WebRTCModule.CustomIceParam customIceParam in this.customIceParams)
            this.WebRtc.AddServerConfig(customIceParam.url, customIceParam.username, customIceParam.credential);
          WebRTCModule.logger.Debug(string.Format("Session ID = {0}, ICE servers added = {1}", (object) this.sessionId, (object) this.customIceParams.Count));
          this.WebRtc.SetAudio(false);
          WebRTCModule.logger.Debug("Session ID = " + this.sessionId + ", video capture ...");
          this.WebRtc.SetVideoCapturer(1280, 720, 5);
          WebRTCModule.logger.Debug("Session ID = " + this.sessionId + ", initializing ...");
          if (this.WebRtc.InitializePeerConnection())
          {
            WebRTCModule.logger.Debug("Session ID = " + this.sessionId + ", initialized");
            this.peerConnected.Set();
            while (!this.Cancel.Token.IsCancellationRequested && this.WebRtc.ProcessMessages(1000))
              WebRTCModule.logger.Trace("Session ID = " + this.sessionId + ", message processed");
            this.WebRtc.ProcessMessages(1000);
          }
          else
            WebRTCModule.logger.Warn("Session ID = " + this.sessionId + ", failure");
        }
      }
    }
  }
}
