using System.Collections.Generic;

namespace TanatKernel
{
	public class AvatarData : IStorable
	{
		public int mId;

		public int mType;

		public string mName;

		public string mLongDesc;

		public string mShortDesc;

		public string mImg;

		public int mArticleId;

		public int mMinDmg;

		public int mMaxDmg;

		public float mArmour;

		public float mMagicArmour;

		public float mAttackSpeed;

		public float mHealth;

		public float mHealthRegen;

		public float mMana;

		public float mManaRegen;

		public List<SkillData> mSkills = new List<SkillData>();

		public string mDescRestriction = "";

		public int Id => mId;
	}
}
