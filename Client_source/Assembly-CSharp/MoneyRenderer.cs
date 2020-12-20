using UnityEngine;

public class MoneyRenderer
{
	private string mDiamonds;

	private string mGold;

	private string mSilver;

	private string mBronze;

	private Rect mGoldRect;

	private Rect mSilverRect;

	private Rect mBronzeRect;

	private Rect mDiamondsRect;

	private Texture2D mMoneyImage;

	private Rect mMoneyImageRect;

	private bool mRenderMoneyImage;

	private bool mIsDiamonds;

	public MoneyRenderer(bool _renderMoneyImage, bool _diamonds)
	{
		mRenderMoneyImage = _renderMoneyImage;
		mIsDiamonds = _diamonds;
		if (mRenderMoneyImage)
		{
			if (mIsDiamonds)
			{
				mMoneyImage = GuiSystem.GetImage("Gui/misc/diamond");
			}
			else
			{
				mMoneyImage = GuiSystem.GetImage("Gui/misc/money");
			}
		}
	}

	public void SetOffset(Vector2 _offset)
	{
		mGoldRect.x += _offset.x;
		mSilverRect.x += _offset.x;
		mBronzeRect.x += _offset.x;
		mDiamondsRect.x += _offset.x;
		mGoldRect.y += _offset.y;
		mSilverRect.y += _offset.y;
		mBronzeRect.y += _offset.y;
		mDiamondsRect.y += _offset.y;
		mMoneyImageRect.x += _offset.x;
		mMoneyImageRect.y += _offset.y;
	}

	public void SetSize(Rect _zoneRect)
	{
		mGoldRect = new Rect(13f, 2f, 22f, 10f);
		mSilverRect = new Rect(45f, 2f, 22f, 10f);
		mBronzeRect = new Rect(77f, 2f, 22f, 10f);
		if (mRenderMoneyImage)
		{
			mMoneyImageRect = new Rect(0f, 4f, mMoneyImage.width, mMoneyImage.height);
			mDiamondsRect = new Rect(mMoneyImageRect.width + 2f, 4f, 64f, 10f);
			GuiSystem.SetChildRect(_zoneRect, ref mDiamondsRect);
			GuiSystem.SetChildRect(_zoneRect, ref mMoneyImageRect);
		}
		GuiSystem.SetChildRect(_zoneRect, ref mGoldRect);
		GuiSystem.SetChildRect(_zoneRect, ref mSilverRect);
		GuiSystem.SetChildRect(_zoneRect, ref mBronzeRect);
	}

	public void SetMoney(int _money)
	{
		if (mIsDiamonds)
		{
			mDiamonds = ((float)_money / 100f).ToString("0.##");
			return;
		}
		int _gold = 0;
		int _silver = 0;
		int _bronze = 0;
		ShopVendor.SetMoney(_money, ref _gold, ref _silver, ref _bronze);
		mGold = _gold.ToString();
		if (_money < 0)
		{
			mGold = "-" + mGold;
		}
		mSilver = _silver.ToString();
		mBronze = _bronze.ToString();
	}

	public void Render()
	{
		if (mRenderMoneyImage)
		{
			GuiSystem.DrawImage(mMoneyImage, mMoneyImageRect);
		}
		if (mIsDiamonds)
		{
			GuiSystem.DrawString(mDiamonds, mDiamondsRect, "middle_left");
			return;
		}
		GuiSystem.DrawString(mGold, mGoldRect, "middle_left");
		GuiSystem.DrawString(mSilver, mSilverRect, "middle_left");
		GuiSystem.DrawString(mBronze, mBronzeRect, "middle_left");
	}
}
