using System;
using System.Text;
using Log4Tanat;

namespace TanatKernel
{
	public class SyncedParams
	{
		private int mTeam;

		private bool mTeamInited;

		private float mHealth;

		private float mMaxHealth;

		private float mMana;

		private float mMaxMana;

		private float mExp;

		public float mX;

		public float mY;

		public float mVelX;

		public float mVelY;

		public float mAngle;

		public float mPosSnapshotTime;

		public float mDamageMin;

		public float mDamageMax;

		public float mArmorPhys;

		public float mAntiArmorPhys;

		public float mArmorMagic;

		public float mAntiArmorMagic;

		public float mAttackSpeed;

		public float mCritChance;

		public float mAntiCritChance;

		public float mCritValue;

		public float mDodge;

		public float mAntiDodge;

		public float mAntiBlockChance;

		public float mBlockChance;

		public float mBlockValue;

		public float mHealthRegen;

		public float mManaRegen;

		public float mSpeed;

		public float mAttackRange;

		public float mViewRadius;

		public float mRadius;

		public int mSilence;

		public int mPhysImm;

		public int mMagImm;

		public float mCastCostCoef;

		public float mCastStrengthCoef;

		public float mCastCooldownCoef;

		public int Team
		{
			get
			{
				if (!mTeamInited)
				{
					throw new InvalidOperationException("invalid team");
				}
				return mTeam;
			}
		}

		public bool IsTeamInited => mTeamInited;

		public float HealthProgress => mHealth;

		public int Health => (int)Math.Round(mHealth * mMaxHealth);

		public int MaxHealth => (int)Math.Round(mMaxHealth);

		public float ManaProgress => mMana;

		public int Mana => (int)Math.Round(mMana * mMaxMana);

		public int MaxMana => (int)Math.Round(mMaxMana);

		public float CurExp => mExp;

		public int Exp => (int)Math.Round(mExp);

		public void GetPosition(float _battleTime, out float _x, out float _y)
		{
			float num = _battleTime - mPosSnapshotTime;
			_x = mX + mVelX * num;
			_y = mY + mVelY * num;
		}

		public void Update(SyncData _syncData, float _time)
		{
			Log.Info(delegate
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("type: ");
				stringBuilder.Append(_syncData.SyncType);
				stringBuilder.Append(", values: ");
				for (int i = 0; i < _syncData.ValuesCount; i++)
				{
					stringBuilder.Append(_syncData.GetValue<string>(i));
					stringBuilder.Append(" ");
				}
				return stringBuilder;
			});
			SyncType syncType = _syncData.SyncType;
			if ((long)syncType <= 131072L)
			{
				if ((long)syncType <= 512L)
				{
					if ((long)syncType <= 32L)
					{
						if ((long)syncType <= 8L)
						{
							if ((long)syncType <= 4L)
							{
								if ((long)syncType < 1L)
								{
									return;
								}
								switch (syncType - 1)
								{
								case (SyncType)0uL:
									mX = _syncData.GetValue<float>(0);
									mY = _syncData.GetValue<float>(1);
									mVelX = _syncData.GetValue<float>(2);
									mVelY = _syncData.GetValue<float>(3);
									mPosSnapshotTime = _syncData.GetValue<float>(4);
									return;
								case SyncType.POSITION:
									mHealth = _syncData.GetValue<float>(0);
									if (mHealth < 0f || mHealth > 1f)
									{
										Log.Error("synced health not at [0; 1]");
									}
									return;
								case (SyncType)3uL:
									mMana = _syncData.GetValue<float>(0);
									return;
								case SyncType.HEALTH:
									return;
								}
							}
							if (syncType == SyncType.EXPERIENCE)
							{
								mExp = _syncData.GetValue<float>(0);
							}
						}
						else
						{
							switch (syncType)
							{
							case SyncType.POS_ANGLE:
								mX = _syncData.GetValue<float>(0);
								mY = _syncData.GetValue<float>(1);
								mAngle = _syncData.GetValue<float>(2);
								break;
							case SyncType.DMG_MIN:
								mDamageMin = _syncData.GetValue<float>(0);
								break;
							}
						}
					}
					else
					{
						switch (syncType)
						{
						case SyncType.DMG_MAX:
							mDamageMax = _syncData.GetValue<float>(0);
							break;
						case SyncType.ATTACK_SPEED:
							mAttackSpeed = _syncData.GetValue<float>(0);
							break;
						case SyncType.CRIT_CHANCE:
							mCritChance = _syncData.GetValue<float>(0);
							break;
						case SyncType.CRIT_VALUE:
							mCritValue = _syncData.GetValue<float>(0);
							break;
						}
					}
				}
				else
				{
					switch (syncType)
					{
					case SyncType.MAX_HEALTH:
						mMaxHealth = _syncData.GetValue<float>(0);
						break;
					case SyncType.ANTICRIT_CHANCE:
						mAntiCritChance = _syncData.GetValue<float>(0);
						break;
					case SyncType.DODGE:
						mDodge = _syncData.GetValue<float>(0);
						break;
					case SyncType.ANTIDODGE:
						mAntiDodge = _syncData.GetValue<float>(0);
						break;
					case SyncType.ATTACK_RANGE:
						mAttackRange = _syncData.GetValue<float>(0);
						break;
					case SyncType.BLOCK_CHANCE:
						mBlockChance = _syncData.GetValue<float>(0);
						break;
					case SyncType.BLOCK_VALUE:
						mBlockValue = _syncData.GetValue<float>(0);
						break;
					case SyncType.HEALTH_REGEN:
						mHealthRegen = _syncData.GetValue<float>(0);
						break;
					}
				}
			}
			else
			{
				switch (syncType)
				{
				case SyncType.TEAM:
					mTeam = _syncData.GetValue<int>(0);
					mTeamInited = true;
					break;
				case SyncType.MAX_MANA:
					mMaxMana = _syncData.GetValue<float>(0);
					break;
				case SyncType.PHYS_ARMOR:
					mArmorPhys = _syncData.GetValue<float>(0);
					break;
				case SyncType.MAGIC_ARMOR:
					mArmorMagic = _syncData.GetValue<float>(0);
					break;
				case SyncType.MANA_REGEN:
					mManaRegen = _syncData.GetValue<float>(0);
					break;
				case SyncType.SPEED:
					mSpeed = _syncData.GetValue<float>(0);
					break;
				case SyncType.VIEW_RADIUS:
					mViewRadius = _syncData.GetValue<float>(0);
					break;
				case SyncType.ANTI_PHYS_ARMOR:
					mAntiArmorPhys = _syncData.GetValue<float>(0);
					break;
				case SyncType.ANTI_MAG_ARMOR:
					mAntiArmorMagic = _syncData.GetValue<float>(0);
					break;
				case SyncType.ANTI_BLOCK_CHANCE:
					mAntiBlockChance = _syncData.GetValue<float>(0);
					break;
				case SyncType.SILENCE:
					mSilence = _syncData.GetValue<int>(0);
					break;
				case SyncType.MAG_IMM:
					mMagImm = _syncData.GetValue<int>(0);
					break;
				case SyncType.PHYS_IMM:
					mPhysImm = _syncData.GetValue<int>(0);
					break;
				case SyncType.CAST_COST_COEF:
					mCastCostCoef = _syncData.GetValue<float>(0);
					break;
				case SyncType.CAST_STRENGTH_COEF:
					mCastStrengthCoef = _syncData.GetValue<float>(0);
					break;
				case SyncType.CAST_COOLDOWN_COEF:
					mCastCooldownCoef = _syncData.GetValue<float>(0);
					break;
				case SyncType.RADIUS:
					mRadius = _syncData.GetValue<float>(0);
					break;
				}
			}
		}
	}
}
