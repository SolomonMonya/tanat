using System;
using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	public class CtrlPrototype : Prototype
	{
		public class PCtrlDesc : PDesc, IAmfLoadable
		{
			public void Load(MixedArray _data)
			{
				mName = _data["title"];
				mDesc = _data["short"];
				mLongDesc = _data["long"];
				mIcon = _data["icon"];
			}
		}

		public class Action
		{
			private int mId;

			private string mTitle;

			private string mDescription;

			public int Id => mId;

			public string Title => mTitle;

			public string Description => mDescription;

			public Action(MixedArray _data)
			{
				if (_data == null)
				{
					throw new ArgumentNullException("_data");
				}
				mId = _data.TryGet("id", -1);
				mTitle = _data.TryGet("title", "");
				mDescription = _data.TryGet("description", "");
			}
		}

		public enum ArticleMask
		{
			NO_SELL = 0x20,
			SALE = 0x2000,
			POPULAR = 0x4000,
			NEW = 0x8000
		}

		public class PArticle : IAmfLoadable
		{
			public int mBuyCost;

			public int mSellCost;

			public int mTypeId;

			public int mKindId;

			public int mMinHeroLvl;

			public int mMinAvaLvl;

			public int[] mConsumableGroups = new int[0];

			public float mCooldown;

			public Action mAction;

			public int mPriceType;

			public int mCount;

			public int mWeigth;

			public int mFlags;

			public int mCountCoef;

			public Dictionary<string, float> mActivators = new Dictionary<string, float>();

			public Dictionary<string, AddStat> mParams = new Dictionary<string, AddStat>();

			public int mTreeId;

			public int mTreeSlot;

			public int[] mTreeParents;

			public int mInlayId;

			public void Load(MixedArray _data)
			{
				mBuyCost = _data.TryGet("price", 0);
				mSellCost = _data.TryGet("sell_price", 0);
				mTypeId = _data.TryGet("type_id", 0);
				mKindId = _data.TryGet("kind_id", 0);
				mMinHeroLvl = _data.TryGet("min_hero_level", 0);
				mMinAvaLvl = _data.TryGet("min_ava_level", 0);
				mCount = _data.TryGet("cnt", 1);
				mPriceType = _data.TryGet("price_type", 1);
				mCooldown = _data.TryGet("cooldown", 0f);
				mWeigth = _data.TryGet("sort", 0);
				mFlags = _data.TryGet("flags", 0);
				mCountCoef = _data.TryGet("cnt_buy_coef", 1);
				mTreeId = _data.TryGet("tree_id", -1);
				mTreeSlot = _data.TryGet("tree_slot", -1);
				mInlayId = _data.TryGet("inlay_id", -1);
				MixedArray mixedArray = _data.TryGet("consumable_groups", new MixedArray());
				mConsumableGroups = new int[mixedArray.Dense.Count];
				for (int i = 0; i < mConsumableGroups.Length; i++)
				{
					mConsumableGroups[i] = mixedArray.Dense[i];
				}
				if (_data.Associative.TryGetValue("action", out var value))
				{
					mAction = new Action(value);
				}
				MixedArray mixedArray2 = _data.TryGet("activators", new MixedArray());
				foreach (KeyValuePair<string, Variable> item2 in mixedArray2.Associative)
				{
					float value2 = item2.Value;
					mActivators.Add(item2.Key, value2);
				}
				MixedArray mixedArray3 = _data.TryGet("params", new MixedArray());
				foreach (Variable item3 in mixedArray3.Dense)
				{
					if (item3.ValueType == typeof(MixedArray))
					{
						MixedArray mixedArray4 = item3;
						string key = mixedArray4.TryGet("skill_id", "undefined");
						int num = mixedArray4.TryGet("impact", 0);
						float num2 = mixedArray4.TryGet("value", 0f);
						AddStat addStat = new AddStat();
						switch (num)
						{
						case 0:
							addStat.mAdd = num2;
							break;
						case 1:
							addStat.mMul = num2;
							break;
						}
						mParams[key] = addStat;
					}
				}
				List<int> list = new List<int>();
				MixedArray mixedArray5 = _data.TryGet("tree_parents", new MixedArray());
				foreach (Variable item4 in mixedArray5.Dense)
				{
					int item = item4;
					list.Add(item);
				}
				mTreeParents = list.ToArray();
			}

			public bool CheckFlag(ArticleMask _mask)
			{
				return ((uint)mFlags & (uint)_mask) != 0;
			}

			public bool ContainsFlags()
			{
				foreach (object value in Enum.GetValues(typeof(ArticleMask)))
				{
					if (CheckFlag((ArticleMask)value))
					{
						return true;
					}
				}
				return false;
			}

			public bool IsConsumable()
			{
				return mTreeId > 0;
			}
		}

		public class PPrefab : IAmfLoadable
		{
			public string mValue;

			public void Load(MixedArray _data)
			{
				mValue = _data["prefab"];
			}
		}

		private PCtrlDesc mDesc;

		private PArticle mArticle;

		private PPrefab mPrefab;

		public override PDesc Desc => GetProperty(ref mDesc);

		public PArticle Article => GetProperty(ref mArticle);

		public PPrefab Prefab => GetProperty(ref mPrefab);

		public CtrlPrototype(int _id, PropertyHolder _propHolder)
			: base(_id, _propHolder)
		{
		}
	}
}
