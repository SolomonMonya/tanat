using System;

namespace TanatKernel
{
	public class BattleThing : Thing
	{
		private BattlePrototype mBattleProto;

		private CtrlPrototype mCtrlProto;

		private bool mAutoUse;

		public BattlePrototype BattleProto => mBattleProto;

		public override CtrlPrototype CtrlProto => mCtrlProto;

		public override PlaceType Place => mBattleProto.Item.mBattleItemType switch
		{
			BattleItemType.HERO => PlaceType.HERO, 
			BattleItemType.QUEST => PlaceType.QUEST, 
			_ => PlaceType.AVATAR, 
		};

		public bool AutoUse
		{
			get
			{
				return mAutoUse;
			}
			set
			{
				mAutoUse = value;
				if (!IsAutoUsePossible() && mAutoUse)
				{
					mAutoUse = false;
				}
			}
		}

		public BattleThing(int _id, BattlePrototype _battleProto, IStoreContentProvider<CtrlPrototype> _ctrlProtoProv)
			: base(_id)
		{
			if (_battleProto == null)
			{
				throw new NullReferenceException("_battleProto");
			}
			if (_ctrlProtoProv == null)
			{
				throw new NullReferenceException("_ctrlProtoProv");
			}
			mBattleProto = _battleProto;
			BattlePrototype.PItem item = _battleProto.Item;
			if (item == null)
			{
				throw new ArgumentException("prototype " + _battleProto.Id + " doesn't contain Item property");
			}
			mCtrlProto = _ctrlProtoProv.Get(item.mArticle);
		}

		public bool IsAutoUsePossible()
		{
			return mCtrlProto.Article.mActivators.Count > 0;
		}
	}
}
