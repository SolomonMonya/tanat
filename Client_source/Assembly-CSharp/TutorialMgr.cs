using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class TutorialMgr
{
	public enum ActionType
	{
		NONE,
		ON_BUTTON,
		ON_MOVE,
		ON_SKILL_UPGRADE,
		ON_SKILL_USE,
		ON_SELF_AVATAR_DIE,
		ON_SELF_AVATAR_LVL_UP,
		ON_KILL,
		ON_NPC_CLICK,
		ON_BUILDING_CLICK,
		BATTLE_START,
		TELEPORT,
		CHANGE_LOCATION
	}

	public class Hint
	{
		public TutorialWindow.HintType mHintType;

		public Vector2 mPosition;

		public UIelement mElement;

		public float mScale = 1f;
	}

	public class UIelement
	{
		public string mParentId;

		public string mElementId;

		public int mKeyId;
	}

	public class BlockAction
	{
		public ActionType mActionType;

		public List<UIelement> mUIelements;
	}

	public class Block
	{
		public string mTextId;

		public List<Hint> mHints;

		public List<UIelement> mEnableAction;

		public List<UIelement> mDisableAction;

		public BlockAction mAction;
	}

	public class TutorialSet
	{
		public string mArea;

		public List<Block> mLinear;

		public List<Block> mNoneLinear;

		public List<UIelement> mEnabled;
	}

	public delegate T Parser<T>(XmlNode _node);

	private List<TutorialSet> mSets = new List<TutorialSet>();

	private TutorialWindow mWindow;

	private IStoreContentProvider<Effector> mEffectorHolder;

	private int mPlayerId;

	private string mCurrentScreen = "central_square";

	private int mSetNumber;

	private static Block mCurrentBlock;

	private static List<UIelement> mUiEnabled;

	public TutorialMgr(string _config)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(_config);
			XmlNode xmlNode = xmlDocument.SelectSingleNode("Tutorial");
			foreach (XmlNode item in xmlNode.SelectNodes("Set"))
			{
				mSets.Add(ParseSet(item));
			}
		}
		catch (Exception ex)
		{
			Log.Error("Tutorial loading error. Message " + GetExceptionMessage(ex));
		}
		mSetNumber = OptionsMgr.TutorialSetNum;
		OptionsMgr.mOnTutorialSetChanged = (OptionsMgr.OnTutorialSetChanged)Delegate.Combine(OptionsMgr.mOnTutorialSetChanged, new OptionsMgr.OnTutorialSetChanged(TutorialSetChanged));
	}

	private string GetExceptionMessage(Exception _ex)
	{
		string text = _ex.Message;
		for (Exception innerException = _ex.InnerException; innerException != null; innerException = innerException.InnerException)
		{
			text = text + "; " + innerException.Message;
		}
		return text;
	}

	public bool IsPortalEnabled(int _location)
	{
		if (mCurrentBlock == null)
		{
			return true;
		}
		if (mCurrentBlock.mAction != null && mCurrentBlock.mAction.mActionType == ActionType.CHANGE_LOCATION)
		{
			return true;
		}
		return false;
	}

	public bool IsBuildingEnabled(int _buildingId)
	{
		if (mCurrentBlock == null)
		{
			return true;
		}
		if (mCurrentBlock.mAction != null && mCurrentBlock.mAction.mActionType == ActionType.ON_BUILDING_CLICK)
		{
			foreach (UIelement mUIelement in mCurrentBlock.mAction.mUIelements)
			{
				if (_buildingId == mUIelement.mKeyId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SetEffectorData(IStoreContentProvider<Effector> _store, int _playerId)
	{
		mEffectorHolder = _store;
		mPlayerId = _playerId;
	}

	public void UnInit()
	{
		mEffectorHolder = null;
		mPlayerId = -1;
		if (mWindow != null)
		{
			mWindow.TutorialDone();
			TutorialWindow tutorialWindow = mWindow;
			tutorialWindow.mBlockDone = (TutorialWindow.BlockDone)Delegate.Remove(tutorialWindow.mBlockDone, new TutorialWindow.BlockDone(OnBlockDone));
			TutorialWindow tutorialWindow2 = mWindow;
			tutorialWindow2.mOnTutorialDone = (TutorialWindow.OnTutorialDone)Delegate.Remove(tutorialWindow2.mOnTutorialDone, new TutorialWindow.OnTutorialDone(OnTutorialDone));
			mWindow = null;
			mCurrentBlock = null;
			mUiEnabled = null;
		}
	}

	public void SetWindow(TutorialWindow _tutorialWindow)
	{
		if (_tutorialWindow == null)
		{
			Log.Warning("Tutorial window is null");
			return;
		}
		if (mWindow != null)
		{
			mWindow.TutorialDone();
			TutorialWindow tutorialWindow = mWindow;
			tutorialWindow.mBlockDone = (TutorialWindow.BlockDone)Delegate.Remove(tutorialWindow.mBlockDone, new TutorialWindow.BlockDone(OnBlockDone));
			TutorialWindow tutorialWindow2 = mWindow;
			tutorialWindow2.mOnTutorialDone = (TutorialWindow.OnTutorialDone)Delegate.Remove(tutorialWindow2.mOnTutorialDone, new TutorialWindow.OnTutorialDone(OnTutorialDone));
		}
		mWindow = _tutorialWindow;
		TutorialWindow tutorialWindow3 = mWindow;
		tutorialWindow3.mBlockDone = (TutorialWindow.BlockDone)Delegate.Combine(tutorialWindow3.mBlockDone, new TutorialWindow.BlockDone(OnBlockDone));
		TutorialWindow tutorialWindow4 = mWindow;
		tutorialWindow4.mOnTutorialDone = (TutorialWindow.OnTutorialDone)Delegate.Combine(tutorialWindow4.mOnTutorialDone, new TutorialWindow.OnTutorialDone(OnTutorialDone));
		mCurrentBlock = null;
		mUiEnabled = null;
	}

	private void OnBlockDone(int _blockNumber)
	{
		if (mSets[mSetNumber].mLinear.Count > _blockNumber)
		{
			mCurrentBlock = mSets[mSetNumber].mLinear[_blockNumber];
			mUiEnabled.AddRange(mCurrentBlock.mEnableAction);
		}
		else
		{
			mCurrentBlock = null;
			mUiEnabled = null;
		}
	}

	private void OnTutorialDone(bool _canseled)
	{
		if (_canseled)
		{
			OptionsMgr.TutorialSetNum = -1;
		}
		else
		{
			OptionsMgr.TutorialSetNum = ++mSetNumber;
		}
	}

	public static GuiElement GetGuiElement(UIelement _element)
	{
		if (_element == null)
		{
			return null;
		}
		if (_element.mKeyId > 0)
		{
			return new GuiElement();
		}
		GuiSystem.GuiSet guiSet = GuiSystem.mGuiSystem.GetGuiSet(GuiSystem.mGuiSystem.GetCurGuiSetId());
		GuiElement elementById = guiSet.GetElementById(_element.mParentId);
		if (elementById == null)
		{
			return null;
		}
		if (_element.mElementId.Length > 0)
		{
			return elementById.GetChild(_element.mElementId);
		}
		return elementById;
	}

	public static bool IsGuiEnabled(GuiElement _element)
	{
		if (mCurrentBlock == null)
		{
			return true;
		}
		foreach (UIelement item in mUiEnabled)
		{
			if (_element == GetGuiElement(item))
			{
				return true;
			}
		}
		if (mCurrentBlock.mAction == null)
		{
			return false;
		}
		foreach (UIelement mUIelement in mCurrentBlock.mAction.mUIelements)
		{
			if (_element == GetGuiElement(mUIelement))
			{
				return true;
			}
		}
		return false;
	}

	public void SetScreenId(string _screenId)
	{
		mCurrentScreen = _screenId;
		SetScreen(mSetNumber, mCurrentScreen);
	}

	private void TutorialSetChanged(int _setNumber)
	{
		if (mWindow != null)
		{
			mWindow.TutorialDone();
		}
		mSetNumber = _setNumber;
		if (mSetNumber == -1)
		{
			UserLog.AddAction(UserActionType.TUTORIAL_CLOSE_ACCEPT);
			mCurrentBlock = null;
			mUiEnabled = null;
		}
		else if (mSetNumber >= mSets.Count)
		{
			UserLog.AddAction(UserActionType.TUTORIAL_DONE);
			mWindow.TutorialFinished();
			OptionsMgr.TutorialSetNum = -1;
			mUiEnabled = null;
			mCurrentBlock = null;
		}
		else
		{
			SetScreen(mSetNumber, mCurrentScreen);
		}
	}

	private void SetScreen(int _setId, string _screenId)
	{
		if (_setId >= mSets.Count || _setId < 0)
		{
			return;
		}
		if (mWindow == null)
		{
			Log.Warning("Try to setup data for TutorialWindow which is null");
			return;
		}
		TutorialSet tutorialSet = mSets[_setId];
		if (tutorialSet.mArea == mCurrentScreen)
		{
			if (_setId == 0)
			{
				UserLog.AddAction(UserActionType.TUTORIAL_START);
			}
			mUiEnabled = new List<UIelement>();
			mUiEnabled.AddRange(tutorialSet.mEnabled);
			if (tutorialSet.mLinear.Count > 0)
			{
				mCurrentBlock = tutorialSet.mLinear[0];
			}
			mWindow.TutorialDone();
			mWindow.SetTutorialData(tutorialSet);
			Log.Debug("window tutorial show");
			mWindow.TutorialStart();
		}
		else
		{
			mWindow.TutorialDone();
		}
	}

	public void OnMoveSendDone()
	{
		mWindow.OnTutorialEvent(ActionType.ON_MOVE);
	}

	public void OnSkillUp()
	{
		mWindow.OnTutorialEvent(ActionType.ON_SKILL_UPGRADE);
	}

	public bool OnNpcClicked(int _npcId)
	{
		if (mCurrentBlock == null)
		{
			return false;
		}
		return mWindow.OnTutorialEvent(ActionType.ON_NPC_CLICK, _npcId);
	}

	public bool OnBuildingClicked(int _npcId)
	{
		if (mCurrentBlock == null)
		{
			return false;
		}
		return mWindow.OnTutorialEvent(ActionType.ON_BUILDING_CLICK, _npcId);
	}

	public void OnSkillUse(DoActionArg _action)
	{
		if (mEffectorHolder == null)
		{
			return;
		}
		Effector effector = null;
		foreach (Effector item in mEffectorHolder.Content)
		{
			if (item.Proto.Id == _action.mActionId)
			{
				effector = item;
				break;
			}
		}
		if (effector != null && effector.SkillType == SkillType.SKILL)
		{
			mWindow.OnTutorialEvent(ActionType.ON_SKILL_USE);
		}
	}

	public void OnKill(KillArg _arg)
	{
		if (mPlayerId == _arg.mVictimId)
		{
			UserLog.AddAction(UserActionType.SELF_DIE);
			mWindow.OnTutorialEvent(ActionType.ON_SELF_AVATAR_DIE);
		}
		else if (mPlayerId == _arg.mKillerId)
		{
			mWindow.OnTutorialEvent(ActionType.ON_KILL);
		}
	}

	public void OnLevelUp(LevelUpArg _arg)
	{
		if (_arg.mObjId == mPlayerId)
		{
			UserLog.AddAction(UserActionType.LEVEL_UP);
			mWindow.OnTutorialEvent(ActionType.ON_SELF_AVATAR_LVL_UP);
		}
	}

	private TutorialSet ParseSet(XmlNode _node)
	{
		TutorialSet tutorialSet = new TutorialSet();
		tutorialSet.mArea = XmlUtil.SafeReadText("area", _node);
		XmlNode node = _node.SelectSingleNode("Linear");
		XmlNode node2 = _node.SelectSingleNode("NoneLinear");
		tutorialSet.mLinear = ParseBlocks(node);
		tutorialSet.mNoneLinear = ParseBlocks(node2);
		tutorialSet.mEnabled = ParseList(_node.SelectSingleNode("Enabled"), ParseUIElement);
		return tutorialSet;
	}

	private List<Block> ParseBlocks(XmlNode _node)
	{
		List<Block> list = new List<Block>();
		if (_node == null)
		{
			return list;
		}
		foreach (XmlNode item in _node.SelectNodes("Block"))
		{
			Block block = new Block();
			block.mTextId = XmlUtil.SafeReadText("id", item);
			block.mEnableAction = ParseList(item.SelectSingleNode("UIEnable"), ParseUIElement);
			block.mDisableAction = ParseList(item.SelectSingleNode("UIDisable"), ParseUIElement);
			block.mHints = ParseList(item.SelectSingleNode("Hints"), ParseHint);
			XmlNode xmlNode2 = item.SelectSingleNode("Action");
			if (xmlNode2 != null)
			{
				block.mAction = new BlockAction();
				Type typeFromHandle = typeof(ActionType);
				try
				{
					block.mAction.mActionType = (ActionType)(int)Enum.Parse(typeFromHandle, XmlUtil.SafeReadText("type", xmlNode2), ignoreCase: true);
				}
				catch (Exception)
				{
					Log.Warning("Unknown Hint type: " + XmlUtil.SafeReadText("type", _node));
					block.mAction.mActionType = ActionType.NONE;
				}
				block.mAction.mUIelements = ParseList(xmlNode2, ParseUIElement);
			}
			if (block.mTextId != string.Empty)
			{
				list.Add(block);
			}
		}
		return list;
	}

	public static List<T> ParseList<T>(XmlNode _node, Parser<T> _parser)
	{
		List<T> list = new List<T>();
		if (_node == null)
		{
			return list;
		}
		foreach (XmlNode childNode in _node.ChildNodes)
		{
			if (!(childNode.OuterXml.Trim().Substring(0, 4) == "<!--"))
			{
				T val = _parser(childNode);
				if (val != null)
				{
					list.Add(val);
				}
			}
		}
		return list;
	}

	private Hint ParseHint(XmlNode _node)
	{
		Hint hint = null;
		if (_node != null)
		{
			hint = new Hint();
			try
			{
				Type typeFromHandle = typeof(TutorialWindow.HintType);
				hint.mHintType = (TutorialWindow.HintType)(int)Enum.Parse(typeFromHandle, XmlUtil.SafeReadText("type", _node), ignoreCase: true);
			}
			catch (Exception)
			{
				Log.Warning("Unknown Hint type: " + XmlUtil.SafeReadText("type", _node));
				hint.mHintType = TutorialWindow.HintType.NONE;
			}
			hint.mElement = ParseUIElement(_node.SelectSingleNode("UIElement"));
			XmlNode xmlNode = _node.SelectSingleNode("Position");
			if (xmlNode != null)
			{
				string[] array = xmlNode.InnerText.Trim().Split(',');
				if (array.Length == 2)
				{
					if (!TryParse(array[0], out var _result))
					{
						Log.Warning("Wrong value for position x element: " + array[0]);
					}
					if (!TryParse(array[1], out var _result2))
					{
						Log.Warning("Wrong value for position y element: " + array[1]);
					}
					hint.mPosition = new Vector2(_result, _result2);
				}
				else
				{
					Log.Error("Wrong value for position element: " + xmlNode.InnerText.Trim());
				}
			}
			XmlNode xmlNode2 = _node.SelectSingleNode("Scale");
			if (xmlNode2 != null)
			{
				string text = xmlNode2.InnerText.Trim();
				if (!TryParse(text, out var _result3))
				{
					Log.Warning("Wrong value for scale element: " + text);
					_result3 = 1f;
				}
				hint.mScale = _result3;
			}
		}
		return hint;
	}

	public static bool TryParse(string _data, out float _result)
	{
		float result = -1f;
		bool result2 = true;
		NumberFormatInfo numberFormat = new CultureInfo("en-US", useUserOverride: false).NumberFormat;
		_data = _data.Replace(",", numberFormat.CurrencyDecimalSeparator);
		_data = _data.Replace(".", numberFormat.CurrencyDecimalSeparator);
		if (!float.TryParse(_data, NumberStyles.Float, numberFormat, out result))
		{
			result2 = false;
		}
		_result = result;
		return result2;
	}

	private UIelement ParseUIElement(XmlNode _node)
	{
		if (_node == null)
		{
			return null;
		}
		UIelement uIelement = new UIelement();
		uIelement.mParentId = XmlUtil.SafeReadText("parentId", _node);
		uIelement.mElementId = XmlUtil.SafeReadText("elementId", _node);
		uIelement.mKeyId = XmlUtil.SafeReadInt("keyId", _node);
		return uIelement;
	}
}
