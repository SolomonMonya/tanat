using System;
using System.IO;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	public class CtrlAvatarStore : Store<AvatarData>
	{
		public CtrlAvatarStore()
			: base("AvatarPrototype")
		{
		}

		public void Retrieve(Stream _stream)
		{
			if (_stream == null)
			{
				throw new ArgumentNullException("_stream");
			}
			Formatter formatter = new Formatter();
			_stream.Position = 0L;
			Variable variable = formatter.Deserialize(_stream);
			if (variable == null || variable.ValueType != typeof(MixedArray))
			{
				Log.Warning("invalid root amf element");
				return;
			}
			MixedArray mixedArray = variable;
			foreach (Variable item2 in mixedArray.Dense)
			{
				MixedArray mixedArray2 = item2;
				AvatarData avatarData = new AvatarData();
				avatarData.mId = mixedArray2.TryGet("id", 0);
				avatarData.mName = mixedArray2.TryGet("name", "");
				avatarData.mShortDesc = mixedArray2.TryGet("short", "");
				avatarData.mLongDesc = mixedArray2.TryGet("long", "");
				avatarData.mImg = mixedArray2.TryGet("icon", "");
				avatarData.mType = mixedArray2.TryGet("type", -1);
				avatarData.mArticleId = mixedArray2.TryGet("artifact", -1);
				if (mixedArray2.Associative.ContainsKey("restiction_desc"))
				{
					avatarData.mDescRestriction = mixedArray2.TryGet("restiction_desc", "");
				}
				avatarData.mHealth = mixedArray2.TryGet("Health", 0f);
				avatarData.mHealthRegen = mixedArray2.TryGet("HealthRegen", 0f);
				avatarData.mMana = mixedArray2.TryGet("Mana", 0f);
				avatarData.mManaRegen = mixedArray2.TryGet("ManaRegen", 0f);
				avatarData.mAttackSpeed = mixedArray2.TryGet("AttackSpeed", 0f);
				avatarData.mMinDmg = mixedArray2.TryGet("DamageMin", 0);
				avatarData.mMaxDmg = mixedArray2.TryGet("DamageMax", 0);
				avatarData.mArmour = mixedArray2.TryGet("PhysArmor", 0f);
				avatarData.mMagicArmour = mixedArray2.TryGet("MagicArmor", 0f);
				MixedArray mixedArray3 = mixedArray2.TryGet("skills", new MixedArray());
				foreach (Variable item3 in mixedArray3.Dense)
				{
					MixedArray mixedArray4 = item3;
					SkillData item = default(SkillData);
					item.mName = mixedArray4.TryGet("title", "");
					item.mDesc = mixedArray4.TryGet("desc", "");
					item.mIcon = mixedArray4.TryGet("icon", "");
					avatarData.mSkills.Add(item);
				}
				Add(avatarData);
			}
		}
	}
}
