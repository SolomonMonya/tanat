using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class PopupInfo : GuiElement
{
	private class TipElement
	{
		public GuiElement mElement;

		public string mText;

		public Rect mZoneRect;

		public float mStartTime;

		public TipElement(string _text, Rect _zone)
		{
			mText = _text;
			mZoneRect = _zone;
			mStartTime = 0f;
			mElement = null;
		}

		public TipElement(string _text, GuiElement _element)
		{
			mText = _text;
			mElement = _element;
			mStartTime = 0f;
			mZoneRect = default(Rect);
		}
	}

	public Dictionary<string, string> mHints = new Dictionary<string, string>();

	private Texture2D mBgImage;

	private string mInfo;

	private float mBorder;

	private float mYOffset;

	private Rect mDrawRect;

	private Rect mTextRect;

	private static Dictionary<GuiElement, Dictionary<string, TipElement>> mTipElements = new Dictionary<GuiElement, Dictionary<string, TipElement>>();

	private static TipElement mTipElement;

	private static TipElement mTipElementToHide;

	private static float mTipDelay;

	public override void Init()
	{
		mBgImage = GuiSystem.GetImage("Gui/misc/popup_frame1");
		mBorder = 10f;
		mTipDelay = 0.6f;
		mHints["AVATAR_MODE_BUTTON-0"] = "GUI_AVATAR_ACTION_TEXT_4";
		mHints["AVATAR_MODE_BUTTON-1"] = "GUI_AVATAR_ACTION_TEXT_3";
		mHints["AVATAR_MODE_BUTTON-2"] = "GUI_AVATAR_ACTION_TEXT_2";
		mHints["AVATAR_ATTACK_BUTTON-3"] = "GUI_AVATAR_ACTION_TEXT_1";
		mHints["PET_ATTACK_BUTTON-1"] = "GUI_SUMMON_ACTION_TEXT_1";
		mHints["PET_STATE_BUTTON-0"] = "GUI_SUMMON_ACTION_TEXT_2";
		mHints["PET_STATE_BUTTON-3"] = "GUI_SUMMON_ACTION_TEXT_3";
	}

	public override void SetSize()
	{
		mDrawRect = new Rect(0f, 0f, 200f, 0f);
		GuiSystem.GetRectScaled(ref mDrawRect);
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (mTipElement == null)
		{
			foreach (KeyValuePair<GuiElement, Dictionary<string, TipElement>> mTipElement2 in mTipElements)
			{
				if (!mTipElement2.Key.Active || (mTipElement2.Key.mGuiSetId != string.Empty && GuiSystem.mGuiSystem.GetCurGuiSetId() != mTipElement2.Key.mGuiSetId))
				{
					continue;
				}
				foreach (KeyValuePair<string, TipElement> item in mTipElement2.Value)
				{
					if (item.Value.mElement != null)
					{
						if (item.Value.mElement.Active && item.Value.mElement.mZoneRect.Contains(_curEvent.mousePosition))
						{
							mTipElement = item.Value;
							mTipElement.mStartTime = Time.time;
							mTipElement.mZoneRect = item.Value.mElement.mZoneRect;
							return;
						}
					}
					else if (item.Value.mZoneRect.Contains(_curEvent.mousePosition))
					{
						mTipElement = item.Value;
						mTipElement.mStartTime = Time.time;
						return;
					}
				}
			}
		}
		else
		{
			if (mTipElement.mStartTime > 0f && Time.time - mTipElement.mStartTime >= mTipDelay)
			{
				Vector2 pos = new Vector2(_curEvent.mousePosition.x - mDrawRect.width / 2f, _curEvent.mousePosition.y);
				mTipElement.mStartTime = 0f;
				ShowInfo(mTipElement.mText, pos);
			}
			if ((!mTipElement.mZoneRect.Contains(_curEvent.mousePosition) && mTipElement.mStartTime == 0f) || mTipElement == mTipElementToHide)
			{
				Hide();
			}
		}
	}

	public override void RenderElement()
	{
		if (string.IsNullOrEmpty(mInfo))
		{
			return;
		}
		if (mDrawRect.height == 0f)
		{
			mTextRect.height = GuiSystem.mGuiSystem.mCurSkin.label.CalcHeight(new GUIContent(mInfo), mTextRect.width);
			mDrawRect.height = mTextRect.height + mBorder * 2f;
			mDrawRect.y = mYOffset - mDrawRect.height;
			if (mDrawRect.y < 0f)
			{
				mDrawRect.y = 0f;
			}
			mTextRect.y = mDrawRect.y + mBorder;
		}
		if (null != mBgImage)
		{
			GuiSystem.DrawImage(mBgImage, mDrawRect, 4, 4, 4, 4);
		}
		GuiSystem.DrawString(mInfo, mTextRect);
	}

	public void ShowInfo(string _txt)
	{
		SetPos(new Vector2(590f, 780f) * GuiSystem.mYRate);
		mInfo = _txt;
		mDrawRect.height = 0f;
	}

	public void ShowInfo(string _txt, Vector2 _pos)
	{
		SetPos(_pos);
		mInfo = _txt;
		mDrawRect.height = 0f;
	}

	public void ShowDesc(Prototype _proto)
	{
		if (_proto == null)
		{
			return;
		}
		if (_proto is BattlePrototype)
		{
			BattlePrototype battlePrototype = _proto as BattlePrototype;
			if (battlePrototype.EffectDesc != null)
			{
				ShowDesc(battlePrototype.EffectDesc.mDesc);
				return;
			}
		}
		ShowDesc(_proto.Desc);
	}

	public void ShowDesc(Prototype.PDesc _desc)
	{
		if (_desc != null)
		{
			string localeText = GuiSystem.GetLocaleText(_desc.mName);
			string localeText2 = GuiSystem.GetLocaleText(_desc.mLongDesc);
			string txt = localeText + "\n\n" + localeText2;
			ShowInfo(txt);
		}
	}

	public void ShowHint(string _hintId)
	{
		if (mHints.TryGetValue(_hintId, out var value))
		{
			ShowInfo(GuiSystem.GetLocaleText(value));
		}
	}

	public void Hide()
	{
		mInfo = null;
		mTipElement = null;
		mTipElementToHide = null;
	}

	private void SetPos(Vector2 _pos)
	{
		if (_pos.x < 0f)
		{
			_pos.x = 0f;
		}
		if (_pos.x + mDrawRect.width > (float)OptionsMgr.mScreenWidth)
		{
			_pos.x = (float)OptionsMgr.mScreenWidth - mDrawRect.width;
		}
		mDrawRect.x = _pos.x;
		mDrawRect.y = _pos.y;
		mYOffset = mDrawRect.y;
		mTextRect.x = mDrawRect.x + mBorder;
		mTextRect.width = mDrawRect.width - mBorder * 2f;
	}

	public static void AddTip(GuiElement _parent, string _textId, Rect _zone)
	{
		if (_parent != null && !string.IsNullOrEmpty(_textId) && _zone.width != 0f && _zone.height != 0f)
		{
			TipElement value = new TipElement(GuiSystem.GetLocaleText(_textId), _zone);
			if (!mTipElements.ContainsKey(_parent))
			{
				mTipElements.Add(_parent, new Dictionary<string, TipElement>());
			}
			mTipElements[_parent].TryGetValue(_textId, out var value2);
			if (value2 == null)
			{
				mTipElements[_parent].Add(_textId, value);
			}
			else
			{
				value2.mZoneRect = _zone;
			}
		}
	}

	public static void AddTip(GuiElement _parent, string _textId, GuiElement _element)
	{
		if (_parent != null && _element != null && !string.IsNullOrEmpty(_textId))
		{
			TipElement value = new TipElement(GuiSystem.GetLocaleText(_textId), _element);
			if (!mTipElements.ContainsKey(_parent))
			{
				mTipElements.Add(_parent, new Dictionary<string, TipElement>());
			}
			mTipElements[_parent].TryGetValue(_textId, out var value2);
			if (value2 == null)
			{
				mTipElements[_parent].Add(_textId, value);
			}
			else
			{
				value2.mElement = _element;
			}
		}
	}

	public static void CloseParentTip(GuiElement _parent)
	{
		if (_parent == null || mTipElement == null || !mTipElements.ContainsKey(_parent))
		{
			return;
		}
		foreach (KeyValuePair<string, TipElement> item in mTipElements[_parent])
		{
			if (item.Value == mTipElement)
			{
				mTipElementToHide = mTipElement;
				break;
			}
		}
	}
}
