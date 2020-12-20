using System;
using System.Collections.Generic;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	public class Effector : IStorable
	{
		private int mId;

		private BattlePrototype mProto;

		private MixedArray mArgs;

		private int mOwnerId;

		private Effector mParent;

		private Effector mChild;

		private SyncedParams mSyncedParams;

		private int mLevel;

		public int Id => mId;

		public BattlePrototype Proto => mProto;

		public SkillType SkillType => mProto.EffectDesc.mDesc.mType;

		public SyncedParams SyncedParams => mSyncedParams;

		public int OwnerId => mOwnerId;

		public Effector Parent => mParent;

		public Effector Child => mChild;

		public float CurManaCost => (float)GetLeveledValue("manacost") * mSyncedParams.mCastCostCoef;

		public float Cooldown => (float)GetLeveledValue("cooldown") * mSyncedParams.mCastCooldownCoef;

		public int Level
		{
			get
			{
				return mLevel;
			}
			set
			{
				mLevel = value;
				if (mLevel < 0)
				{
					Log.Warning("affector " + mId + " level " + mLevel + " < 0");
				}
				else
				{
					Log.Debug("affector " + mId + " level " + mLevel);
				}
			}
		}

		public Effector(int _id, BattlePrototype _proto, MixedArray _args, SyncedParams _params, int _ownerId)
		{
			mId = _id;
			mProto = _proto;
			mArgs = _args;
			mSyncedParams = _params;
			mOwnerId = _ownerId;
		}

		public static void Bind(Effector _parent, Effector _child)
		{
			if (_parent == null || _child == null)
			{
				throw new ArgumentNullException();
			}
			_parent.mChild = _child;
			_child.mParent = _parent;
		}

		public static void Unbind(Effector _affector)
		{
			if (_affector == null)
			{
				throw new ArgumentNullException();
			}
			if (_affector.mParent != null)
			{
				_affector.mParent.mChild = null;
				_affector.mParent = null;
			}
		}

		public Variable GetAttrib(string _attribName)
		{
			if (string.IsNullOrEmpty(_attribName))
			{
				return null;
			}
			Dictionary<string, Variable> mAttribs = mProto.EffectDesc.mAttribs;
			if (mAttribs == null)
			{
				return null;
			}
			mAttribs.TryGetValue(_attribName, out var value);
			return value;
		}

		public Variable GetArg(string _argName)
		{
			if (string.IsNullOrEmpty(_argName))
			{
				return null;
			}
			if (mArgs == null)
			{
				return null;
			}
			mArgs.Associative.TryGetValue(_argName, out var value);
			return value;
		}

		public Variable GetParam(string _name)
		{
			Variable variable = GetArg(_name);
			if (variable == null)
			{
				variable = GetAttrib(_name);
			}
			return variable;
		}

		public object GetValue(string _name, int _level)
		{
			Variable param = GetParam(_name);
			if (param == null)
			{
				Log.Warning(_name + " not found");
				return 0;
			}
			if (param.ValueType == typeof(MixedArray))
			{
				MixedArray mixedArray = param;
				int count = mixedArray.Dense.Count;
				if (_level >= 0 && _level < count)
				{
					return mixedArray.Dense[_level].Value;
				}
				Log.Warning("invalid level " + _level + ", available " + count + " values");
				return 0;
			}
			return param.Value;
		}

		public T[] GetValues<T>(string _paramName)
		{
			Variable param = GetParam(_paramName);
			if (param == null)
			{
				return new T[0];
			}
			List<T> list = new List<T>();
			if (param.ValueType == typeof(MixedArray))
			{
				MixedArray mixedArray = param;
				foreach (Variable item3 in mixedArray.Dense)
				{
					T item = item3.Cast<T>();
					list.Add(item);
				}
			}
			else
			{
				T item2 = param.Cast<T>();
				list.Add(item2);
			}
			return list.ToArray();
		}

		public int GetLeveledValue(string _paramName)
		{
			Variable param = GetParam(_paramName);
			if (param == null)
			{
				return 0;
			}
			int result = 0;
			if (param.ValueType == typeof(MixedArray))
			{
				MixedArray mixedArray = param;
				int num = GetLevel() - 1;
				if (num >= 0 && num < mixedArray.Dense.Count)
				{
					result = mixedArray.Dense[num];
				}
			}
			else
			{
				result = param;
			}
			return result;
		}

		public float[] GetAllManaCosts()
		{
			float[] values = GetValues<float>("manacost");
			int i = 0;
			for (int num = values.Length; i < num; i++)
			{
				values[i] *= mSyncedParams.mCastCostCoef;
			}
			return values;
		}

		public float[] GetAllCooldowns()
		{
			float[] values = GetValues<float>("cooldown");
			int i = 0;
			for (int num = values.Length; i < num; i++)
			{
				values[i] *= mSyncedParams.mCastCooldownCoef;
			}
			return values;
		}

		public bool IsCooldown()
		{
			return false;
		}

		public bool IsEnoughMana(SyncedParams _params)
		{
			if (_params == null)
			{
				throw new ArgumentNullException();
			}
			return (float)_params.Mana >= CurManaCost;
		}

		public int[] GetUpgradeLevels()
		{
			Variable attrib = GetAttrib("levels");
			if (attrib == null || attrib.ValueType != typeof(MixedArray))
			{
				return new int[0];
			}
			MixedArray mixedArray = attrib;
			int[] array = new int[mixedArray.Dense.Count];
			for (int i = 0; i < mixedArray.Dense.Count; i++)
			{
				int num = (array[i] = mixedArray.Dense[i]);
			}
			return array;
		}

		public int GetLevel()
		{
			return GetArg("level")?.Cast<int>() ?? 0;
		}
	}
}
