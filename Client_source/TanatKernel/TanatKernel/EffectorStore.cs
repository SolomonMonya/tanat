using AMF;
using Log4Tanat;

namespace TanatKernel
{
	public class EffectorStore : Store<Effector>
	{
		public EffectorStore()
			: base("Effectors")
		{
		}

		public override void Add(Effector _obj)
		{
			base.Add(_obj);
			if (_obj.SkillType != 0 && _obj.SkillType != SkillType.PARAMS)
			{
				return;
			}
			Variable attrib = _obj.GetAttrib("levels");
			if (attrib == null || attrib.ValueType != typeof(MixedArray))
			{
				return;
			}
			MixedArray mixedArray = attrib;
			foreach (Variable item in mixedArray.Dense)
			{
				if ((int)item == -1)
				{
					_obj.Level++;
					continue;
				}
				break;
			}
		}

		public override void Remove(int _id)
		{
			Effector effector = Get(_id);
			if (effector != null)
			{
				Effector.Unbind(effector);
				base.Remove(_id);
			}
		}

		public override void Clear()
		{
			foreach (Effector @object in base.Objects)
			{
				if (@object != null)
				{
					Effector.Unbind(@object);
				}
			}
			base.Clear();
		}

		public void Bind(int _parentId, int _childId)
		{
			Effector effector = Get(_parentId);
			if (effector == null)
			{
				Log.Error("invalid parent effector " + _parentId);
				return;
			}
			Effector effector2 = Get(_childId);
			if (effector2 == null)
			{
				Log.Error("invalid child effector " + _childId);
				return;
			}
			Effector.Bind(effector, effector2);
			if (effector.SkillType == SkillType.SKILL || effector.SkillType == SkillType.PARAMS)
			{
				Variable variable = effector2.GetLevel();
				if (variable != null)
				{
					effector.Level = variable;
				}
			}
		}
	}
}
