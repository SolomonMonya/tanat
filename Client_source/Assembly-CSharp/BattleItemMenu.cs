using System;
using System.Collections.Generic;
using Log4Tanat;
using TanatKernel;
using UnityEngine;

public class BattleItemMenu : GuiElement
{
	private enum TreeType
	{
		NONE,
		DEFENCE,
		ATTACK,
		MAGIC,
		CONTROL,
		SUPPORT
	}

	private class BattleItemHolder : GuiElement
	{
		public delegate void OnBattleItem(CtrlPrototype _itemData, BattleItemHolder _itemHolder, bool _inlay);

		public delegate void OnBattleItemInlay(int _itemId, int _childId);

		public OnBattleItem mOnBattleItem;

		public OnBattleItemInlay mOnBattleItemInlay;

		private GuiButton mItemButton;

		private GuiButton mInlayButton;

		private Texture2D mInlayDoneImg;

		private Rect mInlayDoneImgRect;

		private InstanceData mAvatarData;

		private CtrlPrototype mItemData;

		private CtrlPrototype mInlayItemData;

		private BattlePrototype mBattleItemData;

		private BattlePrototype mBattleInlayItemData;

		private IStoreContentProvider<CtrlPrototype> mItemsData;

		private FormatedTipMgr mFormatedTipMgr;

		private int mLastTipId;

		private IEnumerable<MainInfoWindow.CooldownView> mCooldownViews;

		private IItemUsageMgr mItemUsageMgr;

		public override void Init()
		{
			mItemButton = GuiSystem.CreateButton("Gui/MainInfo/btn_norm", "Gui/MainInfo/btn_over", "Gui/MainInfo/btn_press", string.Empty, string.Empty);
			mItemButton.mElementId = "ACTIVE_ITEM_BUTTON";
			mItemButton.mIconOnTop = false;
			GuiButton guiButton = mItemButton;
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			GuiButton guiButton2 = mItemButton;
			guiButton2.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton2.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
			GuiButton guiButton3 = mItemButton;
			guiButton3.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton3.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
			mItemButton.mIconImg = GuiSystem.GetImage("Gui/BattleItemMenu/free_item");
			mItemButton.RegisterAction(UserActionType.PLACE_FOR_EQUIPMENT_CLICK);
			mItemButton.Init();
			mInlayButton = GuiSystem.CreateButton("Gui/BattleItemMenu/button_1_norm", "Gui/BattleItemMenu/button_1_over", "Gui/BattleItemMenu/button_1_press", string.Empty, string.Empty);
			mInlayButton.mElementId = "INLAY_BUTTON";
			GuiButton guiButton4 = mInlayButton;
			guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
			GuiButton guiButton5 = mInlayButton;
			guiButton5.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton5.mOnMouseEnter, new OnMouseEnter(OnInlayItemMouseEnter));
			GuiButton guiButton6 = mInlayButton;
			guiButton6.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton6.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
			mInlayButton.Init();
			mInlayButton.SetActive(_active: false);
			mInlayDoneImg = GuiSystem.GetImage("Gui/misc/star");
			GuiSystem.GuiSet guiSet = GuiSystem.mGuiSystem.GetGuiSet("battle");
			mFormatedTipMgr = guiSet.GetElementById<FormatedTipMgr>("FORMATED_TIP");
		}

		public override void SetSize()
		{
			mInlayButton.mZoneRect = new Rect(35f, 33f, 21f, 23f);
			GuiSystem.SetChildRect(mZoneRect, ref mInlayButton.mZoneRect);
			mInlayDoneImgRect = new Rect(33f, 30f, 24f, 28f);
			GuiSystem.SetChildRect(mZoneRect, ref mInlayDoneImgRect);
		}

		public override void CheckEvent(Event _curEvent)
		{
			if (mInlayButton.Active)
			{
				mInlayButton.CheckEvent(_curEvent);
			}
			mItemButton.CheckEvent(_curEvent);
		}

		public override void RenderElement()
		{
			mItemButton.RenderElement();
			if (mItemUsageMgr != null && mCooldownViews != null)
			{
				double num = 0.0;
				double num2 = 0.0;
				double num3 = 0.0;
				if (mItemData != null)
				{
					num2 = mItemUsageMgr.GetCooldownProgress(mItemData);
				}
				if (mInlayItemData != null)
				{
					num3 = mItemUsageMgr.GetCooldownProgress(mInlayItemData);
				}
				num = ((!(num2 >= num3)) ? num3 : num2);
				if (num > 0.0 && num <= 1.0)
				{
					foreach (MainInfoWindow.CooldownView mCooldownView in mCooldownViews)
					{
						if (num <= (double)mCooldownView.mUpperBound)
						{
							GuiSystem.DrawImage(mCooldownView.mTex, mItemButton.mZoneRect);
							break;
						}
					}
				}
			}
			if (mInlayButton.Active)
			{
				mInlayButton.RenderElement();
			}
			if (mInlayItemData != null)
			{
				GuiSystem.DrawImage(mInlayDoneImg, mInlayDoneImgRect);
			}
		}

		public void Clear()
		{
			mAvatarData = null;
			mItemData = null;
			mInlayItemData = null;
			mBattleItemData = null;
			mBattleInlayItemData = null;
			mItemsData = null;
			mItemButton.mIconImg = GuiSystem.GetImage("Gui/BattleItemMenu/free_item");
			mItemButton.mEffectImg = null;
			mItemButton.mId = -1;
			mInlayButton.mId = -1;
			mInlayButton.SetActive(_active: false);
			Close();
		}

		public void Close()
		{
			HideLastTip();
		}

		public void SetItemSize(Rect _size)
		{
			if (mItemButton != null)
			{
				mZoneRect = _size;
				mItemButton.mZoneRect = _size;
				SetSize();
			}
		}

		public void SetData(InstanceData _avaData, IStoreContentProvider<CtrlPrototype> _itemsData, IEnumerable<MainInfoWindow.CooldownView> _cdView, IItemUsageMgr _mgr)
		{
			mAvatarData = _avaData;
			mItemsData = _itemsData;
			mCooldownViews = _cdView;
			mItemUsageMgr = _mgr;
		}

		public void SetItemData(CtrlPrototype _data, BattlePrototype _battleData)
		{
			mItemData = _data;
			mBattleItemData = _battleData;
			mInlayItemData = null;
			mBattleInlayItemData = null;
			if (mItemData != null)
			{
				mItemButton.mId = mItemData.Id;
				mItemButton.mIconImg = GuiSystem.GetImage("Gui/Icons/Items/" + mItemData.Desc.mIcon);
				mInlayButton.mId = mItemData.Article.mInlayId;
			}
			else
			{
				mItemButton.mId = -1;
				mItemButton.mIconImg = GuiSystem.GetImage("Gui/BattleItemMenu/free_item");
				mInlayButton.mId = -1;
			}
			InitInlayData();
		}

		public void SetInlayItemData(CtrlPrototype _inlayData, BattlePrototype _battleData)
		{
			mInlayItemData = _inlayData;
			mBattleInlayItemData = _battleData;
			InitInlayData();
		}

		public int GetItemId()
		{
			if (mItemData == null)
			{
				return -1;
			}
			return mItemData.Id;
		}

		public int GetItemInlayId()
		{
			if (mItemData == null)
			{
				return -1;
			}
			return mItemData.Article.mInlayId;
		}

		public int GetCurItemInlayId()
		{
			if (mInlayItemData == null)
			{
				return -1;
			}
			return mInlayItemData.Id;
		}

		public TreeType GetItemTreeType()
		{
			if (mItemData == null)
			{
				return TreeType.NONE;
			}
			return (TreeType)mItemData.Article.mTreeId;
		}

		public CtrlPrototype GetItemData()
		{
			return mItemData;
		}

		public void SetSelected(bool _selected)
		{
			mItemButton.mEffectImg = ((!_selected) ? null : GuiSystem.GetImage("Gui/misc/item_available"));
		}

		public GuiButton GetItemButton()
		{
			return mItemButton;
		}

		public void HideLastTip()
		{
			if (mFormatedTipMgr != null)
			{
				mFormatedTipMgr.Hide(mLastTipId);
				mLastTipId = 0;
			}
		}

		private void InitInlayData()
		{
			mInlayButton.SetActive(mInlayButton.mId != -1 && mInlayItemData == null);
		}

		private void OnButton(GuiElement _sender, int _buttonId)
		{
			if (_sender.mElementId == "ACTIVE_ITEM_BUTTON" && _buttonId == 0)
			{
				if (mOnBattleItem != null)
				{
					mOnBattleItem(mItemData, this, _inlay: false);
				}
			}
			else if (_sender.mElementId == "INLAY_BUTTON" && _buttonId == 0 && mOnBattleItemInlay != null && mOnBattleItem != null)
			{
				mOnBattleItemInlay(_sender.mId, GetItemId());
				mOnBattleItem(mItemData, this, _inlay: true);
			}
		}

		private void OnItemMouseEnter(GuiElement _sender)
		{
			if (mFormatedTipMgr != null && mAvatarData != null)
			{
				Vector2 pos = new Vector2(_sender.mZoneRect.x, _sender.mZoneRect.y);
				if (mItemData != null && mBattleItemData != null && mInlayItemData == null)
				{
					mFormatedTipMgr.SetPos(pos);
					mFormatedTipMgr.Show(mBattleItemData, mItemData, mAvatarData.Level + 1, -1, _sender.UId, false);
					mLastTipId = _sender.UId;
				}
				else if (mInlayItemData != null && mBattleInlayItemData != null)
				{
					mFormatedTipMgr.SetPos(pos);
					mFormatedTipMgr.Show(mBattleInlayItemData, mInlayItemData, mAvatarData.Level + 1, -1, _sender.UId, false);
					mLastTipId = _sender.UId;
				}
			}
		}

		private void OnInlayItemMouseEnter(GuiElement _sender)
		{
			if (mFormatedTipMgr != null && mAvatarData != null && mItemData != null && mBattleItemData != null)
			{
				CtrlPrototype upgradeArticle = mItemsData.TryGet(_sender.mId);
				Vector2 pos = new Vector2(_sender.mZoneRect.x, _sender.mZoneRect.y);
				mFormatedTipMgr.SetPos(pos);
				mFormatedTipMgr.Show(mBattleItemData, mItemData, upgradeArticle, mAvatarData.Level + 1, -1, _sender.UId, _bought: false);
				mLastTipId = _sender.UId;
			}
		}

		private void OnItemMouseLeave(GuiElement _sender)
		{
			if (mFormatedTipMgr != null)
			{
				mFormatedTipMgr.Hide(_sender.UId);
				if (mLastTipId != _sender.UId)
				{
					mFormatedTipMgr.Hide(mLastTipId);
				}
				mLastTipId = 0;
			}
		}
	}

	private class BattleItemTreeSelect : GuiElement
	{
		private class TextData
		{
			public string mData;

			public Rect mDrawRect;

			public TextData(string _data, Rect _drawRect)
			{
				mData = _data;
				mDrawRect = _drawRect;
			}
		}

		public delegate void OnBattleItemTreeType(TreeType _type);

		public delegate void OnVoidAction();

		private Texture2D mFrame;

		private List<GuiButton> mTreeTypeButtons;

		private Dictionary<TreeType, TextData> mTreeTypeTexts;

		private Rect mTreeTypeTextRect;

		private string mTreeTypeText;

		public GuiButton mCloseButton;

		public OnBattleItemTreeType mOnBattleItemTreeType;

		public OnVoidAction mOnClose;

		public override void Init()
		{
			mFrame = GuiSystem.GetImage("Gui/BattleItemMenu/frame1");
			mTreeTypeButtons = new List<GuiButton>();
			mTreeTypeTexts = new Dictionary<TreeType, TextData>();
			mTreeTypeText = GuiSystem.GetLocaleText("BATTLE_ITEM_TREE_TEXT");
			GuiButton guiButton = null;
			foreach (int value in Enum.GetValues(typeof(TreeType)))
			{
				if (value != 0)
				{
					guiButton = GuiSystem.CreateButton("Gui/MainInfo/btn_norm", "Gui/MainInfo/btn_over", "Gui/MainInfo/btn_press", string.Empty, string.Empty);
					guiButton.mId = value;
					guiButton.mElementId = "TREE_TYPE_BUTTON";
					guiButton.mIconImg = GuiSystem.GetImage("Gui/BattleItemMenu/item_tree_" + ((TreeType)value).ToString().ToLower());
					guiButton.mIconOnTop = false;
					GuiButton guiButton2 = guiButton;
					guiButton2.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton2.mOnMouseUp, new OnMouseUp(OnButton));
					guiButton.RegisterAction(UserActionType.EQUIPMENT_CLICK, "ITEM_TREE_TYPE_" + (TreeType)value);
					guiButton.Init();
					mTreeTypeTexts.Add((TreeType)value, new TextData(GuiSystem.GetLocaleText("ITEM_TREE_TYPE_" + (TreeType)value), default(Rect)));
					mTreeTypeButtons.Add(guiButton);
				}
			}
			mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
			mCloseButton.mElementId = "CLOSE_BUTTON";
			GuiButton guiButton3 = mCloseButton;
			guiButton3.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton3.mOnMouseUp, new OnMouseUp(OnButton));
			mCloseButton.Init();
		}

		public override void SetSize()
		{
			mZoneRect = new Rect(442f, 532f, mFrame.width, mFrame.height);
			GuiSystem.GetRectScaled(ref mZoneRect);
			mTreeTypeTextRect = new Rect(16f, 13f, 256f, 14f);
			GuiSystem.SetChildRect(mZoneRect, ref mTreeTypeTextRect);
			foreach (GuiButton mTreeTypeButton in mTreeTypeButtons)
			{
				switch (mTreeTypeButton.mId)
				{
				case 1:
					mTreeTypeButton.mZoneRect = new Rect(31f, 52f, 46f, 46f);
					break;
				case 2:
					mTreeTypeButton.mZoneRect = new Rect(121f, 52f, 46f, 46f);
					break;
				case 3:
					mTreeTypeButton.mZoneRect = new Rect(211f, 52f, 46f, 46f);
					break;
				case 4:
					mTreeTypeButton.mZoneRect = new Rect(76f, 145f, 46f, 46f);
					break;
				case 5:
					mTreeTypeButton.mZoneRect = new Rect(167f, 145f, 46f, 46f);
					break;
				}
				GuiSystem.SetChildRect(mZoneRect, ref mTreeTypeButton.mZoneRect);
			}
			Rect _rect = default(Rect);
			foreach (int value in Enum.GetValues(typeof(TreeType)))
			{
				if (value != 0)
				{
					switch (value)
					{
					case 1:
						_rect = new Rect(17f, 107f, 75f, 14f);
						break;
					case 2:
						_rect = new Rect(107f, 107f, 75f, 14f);
						break;
					case 3:
						_rect = new Rect(197f, 107f, 75f, 14f);
						break;
					case 4:
						_rect = new Rect(62f, 200f, 75f, 14f);
						break;
					case 5:
						_rect = new Rect(153f, 200f, 75f, 14f);
						break;
					}
					GuiSystem.SetChildRect(mZoneRect, ref _rect);
					mTreeTypeTexts[(TreeType)value].mDrawRect = _rect;
				}
			}
			mCloseButton.mZoneRect = new Rect(262f, 6f, 26f, 26f);
			GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			foreach (GuiButton mTreeTypeButton in mTreeTypeButtons)
			{
				mTreeTypeButton.RenderElement();
			}
			GuiSystem.DrawString(mTreeTypeText, mTreeTypeTextRect, "middle_center");
			foreach (KeyValuePair<TreeType, TextData> mTreeTypeText2 in mTreeTypeTexts)
			{
				GuiSystem.DrawString(mTreeTypeText2.Value.mData, mTreeTypeText2.Value.mDrawRect, "middle_center");
			}
			mCloseButton.RenderElement();
		}

		public override void CheckEvent(Event _curEvent)
		{
			foreach (GuiButton mTreeTypeButton in mTreeTypeButtons)
			{
				mTreeTypeButton.CheckEvent(_curEvent);
			}
			mCloseButton.CheckEvent(_curEvent);
			base.CheckEvent(_curEvent);
		}

		public void SetOffsetX(float _xOffset)
		{
			mZoneRect.x += _xOffset;
			foreach (GuiButton mTreeTypeButton in mTreeTypeButtons)
			{
				mTreeTypeButton.mZoneRect.x += _xOffset;
			}
			foreach (TextData value in mTreeTypeTexts.Values)
			{
				value.mDrawRect.x += _xOffset;
			}
			mCloseButton.mZoneRect.x += _xOffset;
			mTreeTypeTextRect.x += _xOffset;
		}

		public void SetData(CtrlPrototype _itemData, List<TreeType> _usedTreeTypes)
		{
			TreeType treeType = (TreeType)(_itemData?.Article.mTreeId ?? 0);
			SetSelectedTreeType(treeType);
			SetLockedTrees(_usedTreeTypes);
			if (mOnBattleItemTreeType != null)
			{
				mOnBattleItemTreeType(treeType);
			}
		}

		public void Clear()
		{
			Close();
		}

		public void Open()
		{
			SetActive(_active: true);
		}

		public void Close()
		{
			SetActive(_active: false);
		}

		public List<GuiButton> GetTreeTypeButtons()
		{
			return mTreeTypeButtons;
		}

		private void OnButton(GuiElement _sender, int _buttonId)
		{
			if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
			{
				Close();
				if (mOnClose != null)
				{
					mOnClose();
				}
			}
			else if (_sender.mElementId == "TREE_TYPE_BUTTON" && _buttonId == 0)
			{
				TreeType mId = (TreeType)_sender.mId;
				SetSelectedTreeType(mId);
				if (mOnBattleItemTreeType != null)
				{
					mOnBattleItemTreeType(mId);
				}
			}
		}

		private void SetSelectedTreeType(TreeType _type)
		{
			foreach (GuiButton mTreeTypeButton in mTreeTypeButtons)
			{
				mTreeTypeButton.mEffectImg = ((mTreeTypeButton.mId != (int)_type) ? null : GuiSystem.GetImage("Gui/misc/item_available"));
			}
		}

		private void SetLockedTrees(List<TreeType> _usedTreeTypes)
		{
			foreach (GuiButton mTreeTypeButton in mTreeTypeButtons)
			{
				mTreeTypeButton.mLocked = _usedTreeTypes.Contains((TreeType)mTreeTypeButton.mId);
			}
		}
	}

	private class BattleItemTree : GuiElement
	{
		private enum ItemStatus
		{
			NONE,
			AVAILABLE,
			NOT_ENOUGH_MONEY,
			LOCKED,
			USED
		}

		public GuiButton mCloseButton;

		public MainInfoWindow.BattleItemCallback mOnSelectBattleItem;

		private TreeType mTreeType;

		private Texture2D mFrame;

		private Texture2D mTreeFrame;

		private Texture2D mItemAvailable;

		private Texture2D mItemNotAvailable;

		private string mLabel;

		private string mTip;

		private Rect mTreeFrameRect;

		private Rect mLabelRect;

		private Rect mTipRect;

		private ICollection<CtrlPrototype> mTreeItems;

		private Dictionary<int, GuiButton> mTreeItemButtons;

		private InstanceData mAvatarData;

		private PlayerControl mPlayerControl;

		private CtrlPrototype mCurItemData;

		private Dictionary<int, List<CtrlPrototype>> mCurTreeItems;

		private FormatedTipMgr mFormatedTipMgr;

		private IStoreContentProvider<BattlePrototype> mBattleItems;

		private IDictionary<int, int> mItemsToBattleItems;

		private int mLastTipId;

		public override void Init()
		{
			mFrame = GuiSystem.GetImage("Gui/BattleItemMenu/frame2");
			mItemAvailable = GuiSystem.GetImage("Gui/misc/item_available");
			mItemNotAvailable = GuiSystem.GetImage("Gui/misc/item_not_available");
			mCloseButton = GuiSystem.CreateButton("Gui/misc/button_10_norm", "Gui/misc/button_10_over", "Gui/misc/button_10_press", string.Empty, string.Empty);
			mCloseButton.mElementId = "CLOSE_BUTTON";
			GuiButton guiButton = mCloseButton;
			guiButton.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton.mOnMouseUp, new OnMouseUp(OnButton));
			mCloseButton.Init();
			mTreeType = TreeType.NONE;
			mTip = GuiSystem.GetLocaleText("BATTLE_ITEM_TREE_TIP");
			mTreeItemButtons = new Dictionary<int, GuiButton>();
			GuiSystem.GuiSet guiSet = GuiSystem.mGuiSystem.GetGuiSet("battle");
			mFormatedTipMgr = guiSet.GetElementById<FormatedTipMgr>("FORMATED_TIP");
		}

		public override void SetSize()
		{
			mZoneRect = new Rect(440f, 48f, mFrame.width, mFrame.height);
			GuiSystem.GetRectScaled(ref mZoneRect);
			mCloseButton.mZoneRect = new Rect(262f, 6f, 26f, 26f);
			GuiSystem.SetChildRect(mZoneRect, ref mCloseButton.mZoneRect);
			mLabelRect = new Rect(25f, 8f, 244f, 14f);
			GuiSystem.SetChildRect(mZoneRect, ref mLabelRect);
			mTipRect = new Rect(35f, 37f, 223f, 42f);
			GuiSystem.SetChildRect(mZoneRect, ref mTipRect);
			mTreeFrameRect = new Rect(30f, 87f, 232f, 387f);
			GuiSystem.SetChildRect(mZoneRect, ref mTreeFrameRect);
			SetTreeItemButtonsRect();
		}

		public override void RenderElement()
		{
			GuiSystem.DrawImage(mFrame, mZoneRect);
			GuiSystem.DrawImage(mTreeFrame, mTreeFrameRect);
			foreach (KeyValuePair<int, GuiButton> mTreeItemButton in mTreeItemButtons)
			{
				mTreeItemButton.Value.RenderElement();
			}
			GuiSystem.DrawString(mLabel, mLabelRect, "middle_center");
			GuiSystem.DrawString(mTip, mTipRect, "middle_center");
			mCloseButton.RenderElement();
		}

		public override void CheckEvent(Event _curEvent)
		{
			foreach (KeyValuePair<int, GuiButton> mTreeItemButton in mTreeItemButtons)
			{
				mTreeItemButton.Value.CheckEvent(_curEvent);
			}
			mCloseButton.CheckEvent(_curEvent);
			base.CheckEvent(_curEvent);
		}

		public void Clear()
		{
			mTreeType = TreeType.NONE;
			mTreeItems = null;
			mAvatarData = null;
			mPlayerControl = null;
			mCurItemData = null;
			mCurTreeItems = null;
			mBattleItems = null;
			mItemsToBattleItems = null;
			mTreeItemButtons.Clear();
			Close();
		}

		public void SetOffsetX(float _xOffset)
		{
			mZoneRect.x += _xOffset;
			mCloseButton.mZoneRect.x += _xOffset;
			mLabelRect.x += _xOffset;
			mTipRect.x += _xOffset;
			mTreeFrameRect.x += _xOffset;
		}

		public void Open()
		{
			SetActive(_active: true);
		}

		public void Close()
		{
			SetActive(_active: false);
			HideLastTip();
		}

		public void SetData(TreeType _type, ICollection<CtrlPrototype> _treeItems, InstanceData _avaData, PlayerControl _playerCtrl, CtrlPrototype _curItemData, Dictionary<int, List<CtrlPrototype>> _curTreeItems, IStoreContentProvider<BattlePrototype> _battleItems, IDictionary<int, int> _itemsToBattleItems)
		{
			if (_type != 0 && _treeItems != null)
			{
				mBattleItems = _battleItems;
				mItemsToBattleItems = _itemsToBattleItems;
				mCurTreeItems = _curTreeItems;
				mCurItemData = _curItemData;
				mTreeType = _type;
				mTreeItems = _treeItems;
				mAvatarData = _avaData;
				mPlayerControl = _playerCtrl;
				mLabel = GuiSystem.GetLocaleText("ITEM_TREE_TYPE_" + mTreeType);
				mTreeFrame = GuiSystem.GetImage("Gui/BattleItemMenu/TreeFrames/" + _type.ToString().ToLower());
				SetTreeItems();
			}
		}

		public void UpdateData()
		{
			SetTreeItems();
		}

		public void HideLastTip()
		{
			if (mFormatedTipMgr != null)
			{
				mFormatedTipMgr.Hide(mLastTipId);
				mLastTipId = 0;
			}
		}

		private void OnButton(GuiElement _sender, int _buttonId)
		{
			if (_sender.mElementId == "CLOSE_BUTTON" && _buttonId == 0)
			{
				Close();
			}
			else if (_sender.mElementId == "ITEM_TREE_BUTTON" && _buttonId == 0 && mOnSelectBattleItem != null)
			{
				mOnSelectBattleItem(_sender.mId);
			}
		}

		private void OnItemMouseEnter(GuiElement _sender)
		{
			if (mFormatedTipMgr != null && mAvatarData != null)
			{
				CtrlPrototype itemDataById = GetItemDataById(_sender.mId);
				BattlePrototype battleItemDataByArticulId = GetBattleItemDataByArticulId(_sender.mId, mBattleItems, mItemsToBattleItems);
				if (itemDataById != null && battleItemDataByArticulId != null)
				{
					mFormatedTipMgr.SetPos(new Vector2(_sender.mZoneRect.x, _sender.mZoneRect.y));
					mFormatedTipMgr.Show(battleItemDataByArticulId, itemDataById, mAvatarData.Level + 1, -1, _sender.UId, false);
					mLastTipId = _sender.UId;
				}
			}
		}

		private void OnItemMouseLeave(GuiElement _sender)
		{
			if (mFormatedTipMgr != null)
			{
				mFormatedTipMgr.Hide(_sender.UId);
				if (mLastTipId == _sender.UId)
				{
					mLastTipId = 0;
				}
			}
		}

		private void SetTreeItems()
		{
			if (mTreeItems == null)
			{
				return;
			}
			mTreeItemButtons.Clear();
			GuiButton guiButton = null;
			foreach (CtrlPrototype mTreeItem in mTreeItems)
			{
				guiButton = new GuiButton();
				guiButton.mId = mTreeItem.Id;
				guiButton.mElementId = "ITEM_TREE_BUTTON";
				guiButton.mIconOnTop = false;
				GuiButton guiButton2 = guiButton;
				guiButton2.mOnMouseEnter = (OnMouseEnter)Delegate.Combine(guiButton2.mOnMouseEnter, new OnMouseEnter(OnItemMouseEnter));
				GuiButton guiButton3 = guiButton;
				guiButton3.mOnMouseLeave = (OnMouseLeave)Delegate.Combine(guiButton3.mOnMouseLeave, new OnMouseLeave(OnItemMouseLeave));
				guiButton.mIconImg = GuiSystem.GetImage("Gui/Icons/Items/" + mTreeItem.Desc.mIcon);
				switch (GetItemStatus(mTreeItem))
				{
				case ItemStatus.AVAILABLE:
				{
					guiButton.mNormImg = GuiSystem.GetImage("Gui/MainInfo/btn_norm");
					guiButton.mOverImg = GuiSystem.GetImage("Gui/MainInfo/btn_over");
					guiButton.mPressImg = GuiSystem.GetImage("Gui/MainInfo/btn_press");
					guiButton.mEffectImg = mItemAvailable;
					GuiButton guiButton4 = guiButton;
					guiButton4.mOnMouseUp = (OnMouseUp)Delegate.Combine(guiButton4.mOnMouseUp, new OnMouseUp(OnButton));
					break;
				}
				case ItemStatus.LOCKED:
					guiButton.mLocked = true;
					break;
				case ItemStatus.NOT_ENOUGH_MONEY:
					guiButton.mEffectImg = mItemNotAvailable;
					break;
				}
				guiButton.Init();
				if (!mTreeItemButtons.ContainsKey(mTreeItem.Article.mTreeSlot))
				{
					mTreeItemButtons.Add(mTreeItem.Article.mTreeSlot, guiButton);
					continue;
				}
				Log.Error("Item id " + mTreeItem.Id + " in slot " + mTreeItem.Article.mTreeSlot.ToString() + " already exist in tree " + mTreeType.ToString());
			}
			SetTreeItemButtonsRect();
		}

		private void SetTreeItemButtonsRect()
		{
			foreach (KeyValuePair<int, GuiButton> mTreeItemButton in mTreeItemButtons)
			{
				mTreeItemButton.Value.mZoneRect = GetTreeItemRect(mTreeItemButton.Key);
			}
		}

		private Rect GetTreeItemRect(int _slot)
		{
			Rect result = default(Rect);
			switch (_slot)
			{
			default:
				return result;
			case 0:
				result = new Rect(93f, 323f, 46f, 46f);
				GuiSystem.SetChildRect(mTreeFrameRect, ref result);
				break;
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 12:
			{
				_slot--;
				int num = 21;
				int num2 = 247;
				int num3 = 72;
				int num4 = 76;
				int num5 = Mathf.FloorToInt(_slot / 3);
				int num6 = _slot - num5 * 3;
				result = new Rect(num + num6 * num3, num2 - num5 * num4, 46f, 46f);
				GuiSystem.SetChildRect(mTreeFrameRect, ref result);
				break;
			}
			}
			return result;
		}

		private ItemStatus GetItemStatus(CtrlPrototype _itemData)
		{
			bool flag = _itemData.Article.mMinAvaLvl <= mAvatarData.Level + 1;
			bool flag2 = _itemData.Article.mBuyCost <= ((_itemData.Article.mPriceType != 1) ? mPlayerControl.SelfPlayer.RealMoney : mPlayerControl.SelfPlayer.VirtualMoney);
			bool flag3 = false;
			if (mCurItemData != null)
			{
				flag3 = mCurTreeItems[mCurItemData.Article.mTreeId].Contains(_itemData);
			}
			if (!flag)
			{
				return ItemStatus.LOCKED;
			}
			if (flag3)
			{
				return ItemStatus.USED;
			}
			if (mCurItemData != null && mCurItemData.Article.mTreeId == (int)mTreeType)
			{
				int mMinAvaLvl = _itemData.Article.mMinAvaLvl;
				int mMinAvaLvl2 = mCurItemData.Article.mMinAvaLvl;
				if (!flag3)
				{
					if (mMinAvaLvl <= mMinAvaLvl2)
					{
						return ItemStatus.LOCKED;
					}
					if (mMinAvaLvl > mMinAvaLvl2 && !IsInCurTree(_itemData))
					{
						return ItemStatus.LOCKED;
					}
				}
				CtrlPrototype ctrlPrototype = null;
				int[] mTreeParents = _itemData.Article.mTreeParents;
				foreach (int itemId in mTreeParents)
				{
					ctrlPrototype = GetItemDataById(itemId);
					if (ctrlPrototype == mCurItemData)
					{
						if (flag2)
						{
							return ItemStatus.AVAILABLE;
						}
						return ItemStatus.NOT_ENOUGH_MONEY;
					}
				}
				return ItemStatus.LOCKED;
			}
			if ((mCurItemData == null || mCurItemData.Article.mTreeId != (int)mTreeType) && _itemData.Article.mTreeParents.Length != 0)
			{
				return ItemStatus.LOCKED;
			}
			if (flag2)
			{
				return ItemStatus.AVAILABLE;
			}
			return ItemStatus.NOT_ENOUGH_MONEY;
		}

		private bool IsInCurTree(CtrlPrototype _itemData)
		{
			if (_itemData == null)
			{
				return false;
			}
			if (mCurItemData == null)
			{
				return true;
			}
			if (_itemData.Article.mTreeParents.Length != 0)
			{
				CtrlPrototype ctrlPrototype = null;
				int[] mTreeParents = _itemData.Article.mTreeParents;
				foreach (int itemId in mTreeParents)
				{
					ctrlPrototype = GetItemDataById(itemId);
					if (ctrlPrototype == mCurItemData)
					{
						return true;
					}
					if (IsInCurTree(ctrlPrototype))
					{
						return true;
					}
				}
			}
			return false;
		}

		private CtrlPrototype GetItemDataById(int _itemId)
		{
			foreach (CtrlPrototype mTreeItem in mTreeItems)
			{
				if (mTreeItem.Id == _itemId)
				{
					return mTreeItem;
				}
			}
			return null;
		}

		private BattlePrototype GetBattleItemDataById(int _itemId)
		{
			return null;
		}
	}

	public MainInfoWindow.BattleItemCallback mOnBuyItem;

	public MainInfoWindow.BattleItemCallback mOnRemoveItem;

	private int mMaxItemsCount;

	private List<BattleItemHolder> mActiveItems;

	private BattleItemTreeSelect mBattleItemTreeSelect;

	private BattleItemTree mBattleItemTree;

	private IDictionary<int, ICollection<CtrlPrototype>> mTreeItems;

	private Dictionary<int, List<CtrlPrototype>> mCurTreeItems;

	private IStoreContentProvider<CtrlPrototype> mItemsData;

	private BattleItemHolder mLastSelectedItem;

	private InstanceData mAvatarData;

	private PlayerControl mPlayerControl;

	private IStoreContentProvider<BattlePrototype> mBattleItems;

	private IDictionary<int, int> mItemsToBattleItems;

	private YesNoDialog mYesNoDialog;

	private OkDialog mOkDialog;

	private int mLastAvaLvl = -1;

	private IEnumerable<MainInfoWindow.CooldownView> mCooldownViews;

	public override void Init()
	{
		mMaxItemsCount = 3;
		mBattleItemTreeSelect = new BattleItemTreeSelect();
		mBattleItemTreeSelect.SetActive(_active: false);
		BattleItemTreeSelect battleItemTreeSelect = mBattleItemTreeSelect;
		battleItemTreeSelect.mOnBattleItemTreeType = (BattleItemTreeSelect.OnBattleItemTreeType)Delegate.Combine(battleItemTreeSelect.mOnBattleItemTreeType, new BattleItemTreeSelect.OnBattleItemTreeType(OnBattleItemTreeType));
		BattleItemTreeSelect battleItemTreeSelect2 = mBattleItemTreeSelect;
		battleItemTreeSelect2.mOnClose = (BattleItemTreeSelect.OnVoidAction)Delegate.Combine(battleItemTreeSelect2.mOnClose, new BattleItemTreeSelect.OnVoidAction(OnCloseBattleItemTreeSelect));
		mBattleItemTreeSelect.Init();
		mBattleItemTree = new BattleItemTree();
		BattleItemTree battleItemTree = mBattleItemTree;
		battleItemTree.mOnSelectBattleItem = (MainInfoWindow.BattleItemCallback)Delegate.Combine(battleItemTree.mOnSelectBattleItem, new MainInfoWindow.BattleItemCallback(OnSelectBattleItem));
		mBattleItemTree.SetActive(_active: false);
		mBattleItemTree.Init();
		mYesNoDialog = new YesNoDialog();
		mYesNoDialog.Init();
		mYesNoDialog.SetActive(_active: false);
		mOkDialog = new OkDialog();
		mOkDialog.Init();
		mOkDialog.SetActive(_active: false);
		mActiveItems = new List<BattleItemHolder>();
		mCurTreeItems = new Dictionary<int, List<CtrlPrototype>>();
		BattleItemHolder battleItemHolder = null;
		for (int i = 0; i < mMaxItemsCount; i++)
		{
			battleItemHolder = new BattleItemHolder();
			BattleItemHolder battleItemHolder2 = battleItemHolder;
			battleItemHolder2.mOnBattleItem = (BattleItemHolder.OnBattleItem)Delegate.Combine(battleItemHolder2.mOnBattleItem, new BattleItemHolder.OnBattleItem(OnBattleItem));
			BattleItemHolder battleItemHolder3 = battleItemHolder;
			battleItemHolder3.mOnBattleItemInlay = (BattleItemHolder.OnBattleItemInlay)Delegate.Combine(battleItemHolder3.mOnBattleItemInlay, new BattleItemHolder.OnBattleItemInlay(OnSelectBattleItemInlay));
			battleItemHolder.Init();
			mActiveItems.Add(battleItemHolder);
		}
	}

	public override void SetSize()
	{
		mBattleItemTreeSelect.SetSize();
		mBattleItemTree.SetSize();
		mYesNoDialog.SetSize();
		mOkDialog.SetSize();
		foreach (BattleItemHolder mActiveItem in mActiveItems)
		{
			mActiveItem.SetSize();
		}
	}

	public override void RenderElement()
	{
		foreach (BattleItemHolder mActiveItem in mActiveItems)
		{
			mActiveItem.RenderElement();
		}
		if (mBattleItemTreeSelect.Active)
		{
			mBattleItemTreeSelect.RenderElement();
		}
		if (mBattleItemTree.Active)
		{
			mBattleItemTree.RenderElement();
		}
		if (mYesNoDialog.Active)
		{
			mYesNoDialog.RenderElement();
		}
		if (mOkDialog.Active)
		{
			mOkDialog.RenderElement();
		}
	}

	public override void CheckEvent(Event _curEvent)
	{
		if (mYesNoDialog.Active)
		{
			mYesNoDialog.CheckEvent(_curEvent);
			return;
		}
		foreach (BattleItemHolder mActiveItem in mActiveItems)
		{
			mActiveItem.CheckEvent(_curEvent);
		}
		if (mBattleItemTreeSelect.Active)
		{
			mBattleItemTreeSelect.CheckEvent(_curEvent);
		}
		if (mBattleItemTree.Active)
		{
			mBattleItemTree.CheckEvent(_curEvent);
		}
		if (mOkDialog.Active)
		{
			mOkDialog.CheckEvent(_curEvent);
		}
	}

	public override void Update()
	{
		if (mAvatarData != null && mLastAvaLvl != mAvatarData.Level)
		{
			mLastAvaLvl = mAvatarData.Level;
			ReinitItemsTree();
		}
	}

	public static BattlePrototype GetBattleItemDataByArticulId(int _id, IStoreContentProvider<BattlePrototype> _battleItems, IDictionary<int, int> itemsToBattleItems)
	{
		if (!itemsToBattleItems.TryGetValue(_id, out var value))
		{
			return null;
		}
		return _battleItems.TryGet(value);
	}

	public void Clear()
	{
		mBattleItemTree.Clear();
		mBattleItemTreeSelect.Clear();
		mCurTreeItems.Clear();
		foreach (BattleItemHolder mActiveItem in mActiveItems)
		{
			mActiveItem.Clear();
		}
		SetSelectedBattleItem(null);
		mTreeItems = null;
		mItemsData = null;
		mAvatarData = null;
		mPlayerControl = null;
	}

	public bool Close()
	{
		if (mYesNoDialog != null && mYesNoDialog.Active)
		{
			mYesNoDialog.OnEscapeAction();
		}
		if (mOkDialog != null && mOkDialog.Active)
		{
			mOkDialog.OnEscapeAction();
		}
		if (mBattleItemTree.Active || mBattleItemTreeSelect.Active)
		{
			mBattleItemTree.Close();
			mBattleItemTreeSelect.Close();
			SetSelectedBattleItem(null);
			return true;
		}
		foreach (BattleItemHolder mActiveItem in mActiveItems)
		{
			mActiveItem.Close();
		}
		return false;
	}

	public void SetOffsetX(float _xOffset)
	{
		mBattleItemTreeSelect.SetOffsetX(_xOffset);
		mBattleItemTree.SetOffsetX(_xOffset);
	}

	public void SetBattleItemSize(Rect[] _sizes)
	{
		if (_sizes.Length != mMaxItemsCount)
		{
			Log.Error("_rects.Length != mMaxItemsCount. Wrong rect items count : " + _sizes.Length + " != " + mMaxItemsCount);
			return;
		}
		for (int i = 0; i < mMaxItemsCount; i++)
		{
			mActiveItems[i].SetItemSize(_sizes[i]);
		}
	}

	public void SetData(IDictionary<int, ICollection<CtrlPrototype>> _treeItems, IStoreContentProvider<CtrlPrototype> _itemsData, InstanceData _avaData, PlayerControl _playerCtrl, IStoreContentProvider<BattlePrototype> _battleItems, IDictionary<int, int> _itemsToBattleItems, IItemUsageMgr _mgr)
	{
		mTreeItems = _treeItems;
		mItemsData = _itemsData;
		mAvatarData = _avaData;
		mPlayerControl = _playerCtrl;
		mBattleItems = _battleItems;
		mItemsToBattleItems = _itemsToBattleItems;
		foreach (BattleItemHolder mActiveItem in mActiveItems)
		{
			mActiveItem.SetData(_avaData, mItemsData, mCooldownViews, _mgr);
		}
	}

	public void AddBattleItem(int _itemId)
	{
		CtrlPrototype ctrlPrototype = mItemsData.TryGet(_itemId);
		if (ctrlPrototype == null)
		{
			return;
		}
		TreeType mTreeId = (TreeType)ctrlPrototype.Article.mTreeId;
		if (mLastSelectedItem == null || (mLastSelectedItem.GetItemTreeType() != 0 && mLastSelectedItem.GetItemTreeType() != mTreeId))
		{
			SetSelectedBattleItem(null);
			BattleItemHolder battleItemHolder = null;
			foreach (BattleItemHolder mActiveItem in mActiveItems)
			{
				if (mActiveItem.GetItemData() != null && mActiveItem.GetItemTreeType() == mTreeId)
				{
					SetSelectedBattleItem(mActiveItem);
					break;
				}
				if (mActiveItem.GetItemData() == null && battleItemHolder == null)
				{
					battleItemHolder = mActiveItem;
				}
			}
			if (mLastSelectedItem == null)
			{
				SetSelectedBattleItem(battleItemHolder);
			}
		}
		if (mLastSelectedItem == null)
		{
			Log.Error("Some Logic error in AddBattleItem. There is no correct ItemHolder for item : " + _itemId);
			return;
		}
		TreeType itemTreeType = mLastSelectedItem.GetItemTreeType();
		BattlePrototype battleItemDataByArticulId = GetBattleItemDataByArticulId(ctrlPrototype.Id, mBattleItems, mItemsToBattleItems);
		int itemSlot = GetItemSlot(mLastSelectedItem);
		if (mLastSelectedItem.GetItemInlayId() == _itemId)
		{
			mActiveItems[itemSlot].SetInlayItemData(ctrlPrototype, battleItemDataByArticulId);
		}
		else if (ctrlPrototype.Article.mTreeId == (int)itemTreeType || itemTreeType == TreeType.NONE)
		{
			mActiveItems[itemSlot].SetItemData(ctrlPrototype, battleItemDataByArticulId);
		}
		if (!mCurTreeItems.ContainsKey(ctrlPrototype.Article.mTreeId))
		{
			mCurTreeItems.Add(ctrlPrototype.Article.mTreeId, new List<CtrlPrototype>());
		}
		mCurTreeItems[ctrlPrototype.Article.mTreeId].Add(ctrlPrototype);
		if (!mBattleItemTree.Active && !mBattleItemTreeSelect.Active)
		{
			SetSelectedBattleItem(null);
		}
		ReinitItemsTree();
	}

	public void RemoveBattleItem(int _itemId)
	{
		CtrlPrototype ctrlPrototype = mItemsData.TryGet(_itemId);
		if (ctrlPrototype == null)
		{
			return;
		}
		int mTreeId = ctrlPrototype.Article.mTreeId;
		foreach (BattleItemHolder mActiveItem in mActiveItems)
		{
			if (mActiveItem.GetItemId() == _itemId)
			{
				mActiveItem.SetItemData(null, null);
				break;
			}
		}
		if (!mCurTreeItems.ContainsKey(mTreeId))
		{
			Log.Error("Try to remove bad item : " + ctrlPrototype.Id + " tree id : " + mTreeId);
			return;
		}
		mCurTreeItems[mTreeId].Remove(ctrlPrototype);
		if (mCurTreeItems[mTreeId].Count == 0)
		{
			mCurTreeItems.Remove(mTreeId);
		}
	}

	public List<GuiButton> GetItemButtons()
	{
		List<GuiButton> list = new List<GuiButton>();
		foreach (BattleItemHolder mActiveItem in mActiveItems)
		{
			list.Add(mActiveItem.GetItemButton());
		}
		return list;
	}

	public List<GuiButton> GetTreeTypeButtons()
	{
		List<GuiButton> result = new List<GuiButton>();
		if (mBattleItemTreeSelect != null)
		{
			result = mBattleItemTreeSelect.GetTreeTypeButtons();
		}
		return result;
	}

	public List<GuiButton> GetCloseButtons()
	{
		List<GuiButton> list = new List<GuiButton>();
		list.Add(mBattleItemTreeSelect.mCloseButton);
		list.Add(mBattleItemTree.mCloseButton);
		return list;
	}

	public void SetCooldownViews(IEnumerable<MainInfoWindow.CooldownView> _views)
	{
		mCooldownViews = _views;
	}

	private void ReinitItemsTree()
	{
		if (mLastSelectedItem != null && mBattleItemTree != null && mBattleItemTree.Active && mTreeItems.TryGetValue((int)mLastSelectedItem.GetItemTreeType(), out var value))
		{
			mBattleItemTree.SetData(mLastSelectedItem.GetItemTreeType(), value, mAvatarData, mPlayerControl, mLastSelectedItem.GetItemData(), mCurTreeItems, mBattleItems, mItemsToBattleItems);
		}
	}

	private void SetSelectedBattleItem(BattleItemHolder _curItem)
	{
		if (mLastSelectedItem == _curItem)
		{
			return;
		}
		mLastSelectedItem = _curItem;
		foreach (BattleItemHolder mActiveItem in mActiveItems)
		{
			mActiveItem.SetSelected(mLastSelectedItem != null && mLastSelectedItem == mActiveItem);
		}
	}

	private void OnSelectBattleItem(int _itemId)
	{
		CtrlPrototype curItemData = mItemsData.TryGet(_itemId);
		if (curItemData == null || mLastSelectedItem == null)
		{
			return;
		}
		TreeType itemTreeType = mLastSelectedItem.GetItemTreeType();
		int lastId = mLastSelectedItem.GetItemId();
		int lastInlayId = mLastSelectedItem.GetCurItemInlayId();
		if (itemTreeType != 0 && curItemData.Article.mTreeId != (int)itemTreeType)
		{
			YesNoDialog.OnAnswer callback = delegate(bool _yes)
			{
				if (_yes)
				{
					if (mOnRemoveItem != null)
					{
						mOnRemoveItem((lastInlayId != -1) ? lastInlayId : lastId);
					}
					UserLog.AddAction(UserActionType.BUY_BATTLE_EQUIPMENT, GuiSystem.GetLocaleText(curItemData.Desc.mName));
					if (mOnBuyItem != null)
					{
						mOnBuyItem(_itemId);
					}
				}
			};
			mYesNoDialog.SetData(GuiSystem.GetLocaleText("GUI_SWITCH_ITEM_TREE_QUESTION"), "Switch_Button_Name", "Cancel_Button_Name", callback);
			HideFormatedTips();
		}
		else
		{
			UserLog.AddAction(UserActionType.BUY_BATTLE_EQUIPMENT, GuiSystem.GetLocaleText(curItemData.Desc.mName));
			if (mOnBuyItem != null)
			{
				mOnBuyItem(_itemId);
			}
		}
	}

	private void OnSelectBattleItemInlay(int _inlayId, int _childId)
	{
		CtrlPrototype curInlayData = mItemsData.TryGet(_inlayId);
		if (curInlayData == null)
		{
			return;
		}
		CtrlPrototype ctrlPrototype = mItemsData.TryGet(_childId);
		if (ctrlPrototype == null)
		{
			return;
		}
		if (curInlayData.Article.mBuyCost > ((curInlayData.Article.mPriceType != 1) ? mPlayerControl.SelfPlayer.RealMoney : mPlayerControl.SelfPlayer.VirtualMoney))
		{
			mOkDialog.SetData(GuiSystem.GetLocaleText("GUI_INLAY_NOT_ENOUGH_MONEY"));
			return;
		}
		string localeText = GuiSystem.GetLocaleText("GUI_INLAY_UPGRADE_QUESTION");
		localeText = localeText.Replace("{ITEM_NAME}", GuiSystem.GetLocaleText(ctrlPrototype.Desc.mName));
		YesNoDialog.OnAnswer callback = delegate(bool _yes)
		{
			if (_yes)
			{
				if (mLastSelectedItem != null && curInlayData.Article.mTreeId != (int)mLastSelectedItem.GetItemTreeType())
				{
					SetSelectedBattleItem(null);
				}
				UserLog.AddAction(UserActionType.UPGRADE_BATTLE_EQUIPMENT, GuiSystem.GetLocaleText(curInlayData.Desc.mName));
				if (mOnBuyItem != null)
				{
					mOnBuyItem(_inlayId);
				}
			}
		};
		mYesNoDialog.SetData(localeText, "Upgrade_Button_Name", "Cancel_Button_Name", callback);
		mYesNoDialog.SetMoneyData(curInlayData.Article.mBuyCost, curInlayData.Article.mPriceType == 2);
		HideFormatedTips();
	}

	private void HideFormatedTips()
	{
		if (mBattleItemTree != null)
		{
			mBattleItemTree.HideLastTip();
		}
		foreach (BattleItemHolder mActiveItem in mActiveItems)
		{
			mActiveItem.HideLastTip();
		}
	}

	private int GetItemSlot(BattleItemHolder _itemHolder)
	{
		for (int i = 0; i < mActiveItems.Count; i++)
		{
			if (mActiveItems[i] == _itemHolder)
			{
				return i;
			}
		}
		return -1;
	}

	private void OnBattleItem(CtrlPrototype _itemData, BattleItemHolder _itemHolder, bool _inlay)
	{
		List<TreeType> list = new List<TreeType>();
		foreach (BattleItemHolder mActiveItem in mActiveItems)
		{
			if (mActiveItem != _itemHolder)
			{
				list.Add(mActiveItem.GetItemTreeType());
			}
		}
		if (!_inlay || (_inlay && mBattleItemTreeSelect.Active))
		{
			SetSelectedBattleItem(_itemHolder);
			mBattleItemTreeSelect.SetData(_itemData, list);
			mBattleItemTreeSelect.Open();
		}
	}

	private void OnBattleItemTreeType(TreeType _type)
	{
		ICollection<CtrlPrototype> value;
		if (_type == TreeType.NONE)
		{
			if (mBattleItemTree.Active)
			{
				mBattleItemTree.Close();
			}
		}
		else if (mTreeItems.TryGetValue((int)_type, out value))
		{
			mBattleItemTree.SetData(_type, value, mAvatarData, mPlayerControl, mLastSelectedItem.GetItemData(), mCurTreeItems, mBattleItems, mItemsToBattleItems);
			mBattleItemTree.Open();
		}
	}

	private void OnCloseBattleItemTreeSelect()
	{
		if (mBattleItemTree.Active)
		{
			mBattleItemTree.Close();
		}
		SetSelectedBattleItem(null);
	}
}
