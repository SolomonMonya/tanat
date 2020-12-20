using System.Collections.Generic;
using UnityEngine;

public class TipMgr : GuiElement
{
	public class Tip
	{
		public string mTitle;

		public string mData;

		public Rect mDrawRect;

		public Rect mTitleRect;

		public Rect mDataRect;

		public Texture2D mFrame;

		public Vector2 mPos = Vector2.zero;

		public Tip()
		{
			mTitle = string.Empty;
			mData = string.Empty;
			mDrawRect = default(Rect);
			mTitleRect = default(Rect);
			mDataRect = default(Rect);
			mFrame = null;
		}
	}

	private Dictionary<int, Tip> mTips;

	private List<int> mTipsForRemove;

	private int mLastTipId;

	private float mMinDeltaPos;

	public override void Init()
	{
		mTips = new Dictionary<int, Tip>();
		mTipsForRemove = new List<int>();
		mMinDeltaPos = 4f;
	}

	public override void Update()
	{
		foreach (int item in mTipsForRemove)
		{
			mTips.Remove(item);
			if (mTips.Count == 0)
			{
				mLastTipId = 0;
			}
		}
		mTipsForRemove.Clear();
	}

	public override void RenderElement()
	{
		foreach (Tip value in mTips.Values)
		{
			GuiSystem.DrawImage(value.mFrame, value.mDrawRect, 4, 4, 4, 4);
			GuiSystem.DrawString(value.mTitle, value.mTitleRect);
			GuiSystem.DrawString(value.mData, value.mDataRect);
		}
	}

	public int ShowTip(Vector2 _pos, string _title, string _data)
	{
		if (mTips == null)
		{
			return -1;
		}
		Tip tip = new Tip();
		tip.mTitle = _title;
		tip.mData = _data;
		tip.mFrame = GuiSystem.GetImage("Gui/misc/popup_frame1");
		int num = GenerateTipId();
		mTips.Add(num, tip);
		SetTipPos(num, _pos);
		return num;
	}

	public void HideTip(int _tipId)
	{
		mTipsForRemove.Add(_tipId);
	}

	public void SetTipPos(int _tipId, Vector2 _pos)
	{
		if (mTips.ContainsKey(_tipId))
		{
			Tip tip = mTips[_tipId];
			if (!((tip.mPos - _pos).magnitude < mMinDeltaPos))
			{
				tip.mPos = _pos;
				_pos.y += 10f;
				float num = 100f;
				Vector2 vector = new Vector2(10f, 10f);
				tip.mDrawRect = new Rect(_pos.x - num / 2f, (float)Screen.height - _pos.y, num, num / 2f);
				tip.mTitleRect = new Rect(tip.mDrawRect.x + vector.x, tip.mDrawRect.y + vector.y, tip.mDrawRect.width - vector.x * 2f, 0f);
				tip.mTitleRect.height = GuiSystem.mGuiSystem.mCurSkin.label.CalcHeight(new GUIContent(tip.mTitle), tip.mTitleRect.width);
				tip.mDataRect = new Rect(tip.mTitleRect.x, tip.mTitleRect.y + tip.mTitleRect.height, tip.mTitleRect.width, 0f);
				tip.mDataRect.height = GuiSystem.mGuiSystem.mCurSkin.label.CalcHeight(new GUIContent(tip.mData), tip.mDataRect.width);
				tip.mDrawRect.height = tip.mTitleRect.height + tip.mDataRect.height + vector.y * 2f;
				tip.mDrawRect.y -= tip.mDrawRect.height;
				tip.mDataRect.y -= tip.mDrawRect.height;
				tip.mTitleRect.y -= tip.mDrawRect.height;
			}
		}
	}

	private int GenerateTipId()
	{
		return ++mLastTipId;
	}
}
