using TanatKernel;
using UnityEngine;

[AddComponentMenu("Tanat/Info/FpsCounter")]
public class FpsCounter : MonoBehaviour
{
	private float mUpdateInterval;

	private float mLastInterval;

	private int mLastFrame;

	private int mLastFrameRender;

	private string mFPS;

	private string mRFPS;

	private BattlePacketManager mPacketMgr;

	private Rect mFPSRect;

	private Rect mSFPSRect;

	private Rect mPingRect;

	public void Init(BattlePacketManager _packetMgr)
	{
		mPacketMgr = _packetMgr;
	}

	public void Uninit()
	{
		mPacketMgr = null;
	}

	public void Start()
	{
		mUpdateInterval = 0.5f;
		mLastInterval = Time.realtimeSinceStartup;
		mFPS = "0.0";
		mRFPS = "0.0";
		mFPSRect = new Rect(2f, 256f, 64f, 16f);
		mSFPSRect = new Rect(2f, 272f, 64f, 16f);
		mPingRect = new Rect(2f, 288f, 64f, 16f);
	}

	public void Update()
	{
		if (Time.realtimeSinceStartup > mLastInterval + mUpdateInterval)
		{
			mFPS = "fps: " + ((float)(Time.frameCount - mLastFrame) / (Time.realtimeSinceStartup - mLastInterval)).ToString("#");
			mRFPS = "rfps: " + ((float)(Time.renderedFrameCount - mLastFrameRender) / (Time.realtimeSinceStartup - mLastInterval)).ToString("#");
			mLastInterval = Time.realtimeSinceStartup;
			mLastFrameRender = Time.renderedFrameCount;
			mLastFrame = Time.frameCount;
		}
	}

	public void Render()
	{
		GUI.color = Color.green;
		GuiSystem.DrawString(mFPS, mFPSRect, "log");
		GuiSystem.DrawString(mRFPS, mSFPSRect, "log");
		if (mPacketMgr != null)
		{
			GuiSystem.DrawString("ping: " + mPacketMgr.Ping.ToString("0.###"), mPingRect, "log");
		}
	}
}
