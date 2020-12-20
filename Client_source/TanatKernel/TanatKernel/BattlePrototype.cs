using System;
using System.Collections.Generic;
using System.Xml;
using AMF;

namespace TanatKernel
{
	public class BattlePrototype : Prototype
	{
		public class ExistenceIndicator : IXmlLoadable
		{
			public bool mValue;

			public void Load(XmlNode _node)
			{
				mValue = XmlUtil.SafeReadBool("value", _node);
			}
		}

		public class PBattleDesc : PDesc, IXmlLoadable
		{
			public void Load(XmlNode _node)
			{
				mName = XmlUtil.SafeReadText("value", _node.SelectSingleNode("Name"));
				mDesc = XmlUtil.SafeReadText("value", _node.SelectSingleNode("Short"));
				mLongDesc = XmlUtil.SafeReadText("value", _node.SelectSingleNode("Long"));
				mIcon = XmlUtil.SafeReadText("value", _node.SelectSingleNode("Icon"));
			}
		}

		public class PPrefab : IXmlLoadable
		{
			public string mValue;

			public void Load(XmlNode _node)
			{
				mValue = XmlUtil.SafeReadText("value", _node);
			}
		}

		public class PDestructible : IXmlLoadable
		{
			public float mHealth;

			public void Load(XmlNode _node)
			{
				mHealth = XmlUtil.SafeReadFloat("value", _node.SelectSingleNode("Health"));
			}
		}

		public class PCaster : IXmlLoadable
		{
			public float mMana;

			public void Load(XmlNode _node)
			{
				mMana = XmlUtil.SafeReadFloat("value", _node.SelectSingleNode("Mana"));
			}
		}

		public class PExperiencer : IXmlLoadable
		{
			public float[] mLevelsXP;

			public void Load(XmlNode _node)
			{
				mLevelsXP = XmlUtil.SafeReadFloatArray("value", _node.SelectSingleNode("XP"));
			}
		}

		public class PItem : IXmlLoadable
		{
			public BattleItemType mBattleItemType;

			public int mBuyCost;

			public int mSellCost;

			public int mMinLevel;

			public int mArticle;

			public SkillTarget mTarget;

			public float mAoeRadius;

			public void Load(XmlNode _node)
			{
				mBattleItemType = XmlUtil.SafeReadEnum<BattleItemType>("value", _node.SelectSingleNode("Type"));
				mBuyCost = XmlUtil.SafeReadInt("value", _node.SelectSingleNode("BuyCost"));
				mSellCost = XmlUtil.SafeReadInt("value", _node.SelectSingleNode("SellCost"));
				mMinLevel = XmlUtil.SafeReadInt("value", _node.SelectSingleNode("Level"));
				mArticle = XmlUtil.SafeReadInt("value", _node.SelectSingleNode("Article"));
			}
		}

		public class PShopSeller : IXmlLoadable
		{
			public int[] mItems;

			public void Load(XmlNode _node)
			{
				mItems = XmlUtil.SafeReadIntArray("value", _node.SelectSingleNode("Items"));
			}
		}

		public class PShop : IXmlLoadable
		{
			public int[] mSellers;

			public float mBuyCoef;

			public float mSellCoef;

			public void Load(XmlNode _node)
			{
				mBuyCoef = XmlUtil.SafeReadFloat("value", _node.SelectSingleNode("BuyCoef"));
				mSellCoef = XmlUtil.SafeReadFloat("value", _node.SelectSingleNode("SellCoef"));
				mSellers = XmlUtil.SafeReadIntArray("value", _node.SelectSingleNode("Sellers"));
			}
		}

		public class PEffectDesc : IXmlLoadable
		{
			public class Desc : PDesc
			{
				public SkillType mType;
			}

			public Desc mDesc;

			public Dictionary<string, Variable> mAttribs;

			public void Load(XmlNode _node)
			{
				XmlNode xmlNode = _node.SelectSingleNode("Desc");
				if (xmlNode != null)
				{
					mDesc = new Desc();
					mDesc.mName = XmlUtil.SafeReadText("value", xmlNode.SelectSingleNode("name"));
					mDesc.mDesc = XmlUtil.SafeReadText("value", xmlNode.SelectSingleNode("short"));
					mDesc.mLongDesc = XmlUtil.SafeReadText("value", xmlNode.SelectSingleNode("long"));
					mDesc.mIcon = XmlUtil.SafeReadText("value", xmlNode.SelectSingleNode("icon"));
					mDesc.mType = XmlUtil.SafeReadEnum<SkillType>("value", xmlNode.SelectSingleNode("type"));
				}
				mAttribs = new Dictionary<string, Variable>();
				XmlNode xmlNode2 = _node.SelectSingleNode("Attribs");
				if (xmlNode2 == null)
				{
					return;
				}
				for (XmlNode xmlNode3 = xmlNode2.FirstChild; xmlNode3 != null; xmlNode3 = xmlNode3.NextSibling)
				{
					string text = XmlUtil.SafeReadText("name", xmlNode3);
					string text2 = XmlUtil.SafeReadText("value", xmlNode3);
					Variable variable = null;
					if (xmlNode3.Name == "enum" && text == "target")
					{
						string text3 = XmlUtil.SafeReadText("value", xmlNode3);
						int num = 0;
						string[] array = text3.Split(new char[1]
						{
							'+'
						}, StringSplitOptions.RemoveEmptyEntries);
						string[] array2 = array;
						foreach (string value in array2)
						{
							object obj = null;
							try
							{
								obj = Enum.Parse(typeof(SkillTarget), value);
							}
							catch (ArgumentException)
							{
							}
							if (obj != null)
							{
								int num2 = (int)obj;
								num |= num2;
							}
						}
						variable = new Variable(num);
					}
					else if (text2.Contains(";"))
					{
						string[] array3 = XmlUtil.SafeReadTextArray("value", xmlNode3);
						MixedArray mixedArray = new MixedArray();
						string[] array4 = array3;
						foreach (string text4 in array4)
						{
							mixedArray.Dense.Add(text4);
						}
						variable = mixedArray;
					}
					else
					{
						variable = text2;
					}
					if (variable != null)
					{
						mAttribs.Add(text, variable);
					}
				}
			}

			private Type ParseType(string _str)
			{
				return _str switch
				{
					"int" => typeof(int), 
					"float" => typeof(float), 
					"string" => typeof(string), 
					_ => null, 
				};
			}
		}

		public class PAvatar : ExistenceIndicator
		{
		}

		public class PBuilding : ExistenceIndicator
		{
		}

		public class PTouchable : ExistenceIndicator
		{
		}

		public class PItemContainer : ExistenceIndicator
		{
		}

		public class PProjectile : ExistenceIndicator
		{
		}

		public class PTool : IXmlLoadable
		{
			public int mAction;

			public void Load(XmlNode _node)
			{
				mAction = XmlUtil.SafeReadInt("value", _node.SelectSingleNode("Action"));
			}
		}

		private IDictionary<string, float> mCoers;

		private PBattleDesc mDesc;

		private PPrefab mPrefab;

		private PDestructible mDestructible;

		private PCaster mCaster;

		private PExperiencer mExperiencer;

		private PItem mItem;

		private PShopSeller mShopSeller;

		private PShop mShop;

		private PEffectDesc mEffectDesc;

		private PAvatar mAvatar;

		private PBuilding mBuilding;

		private PTouchable mTouchable;

		private PItemContainer mItemContainer;

		private PProjectile mProjectile;

		private PTool mTool;

		public IDictionary<string, float> Coefs => mCoers;

		public override PDesc Desc => GetProperty(ref mDesc);

		public PPrefab Prefab => GetProperty(ref mPrefab);

		public PDestructible Destructible => GetProperty(ref mDestructible);

		public PCaster Caster => GetProperty(ref mCaster);

		public PExperiencer Experiencer => GetProperty(ref mExperiencer);

		public PItem Item => GetProperty(ref mItem);

		public PShopSeller ShopSeller => GetProperty(ref mShopSeller);

		public PShop Shop => GetProperty(ref mShop);

		public PEffectDesc EffectDesc => GetProperty(ref mEffectDesc);

		public PAvatar Avatar => GetProperty(ref mAvatar);

		public PBuilding Building => GetProperty(ref mBuilding);

		public PTouchable Touchable => GetProperty(ref mTouchable);

		public PItemContainer ItemContainer => GetProperty(ref mItemContainer);

		public PProjectile Projectile => GetProperty(ref mProjectile);

		public PTool Tool => GetProperty(ref mTool);

		public BattlePrototype(int _id, PropertyHolder _propHolder, IDictionary<string, float> _coers)
			: base(_id, _propHolder)
		{
			mCoers = _coers;
		}
	}
}
