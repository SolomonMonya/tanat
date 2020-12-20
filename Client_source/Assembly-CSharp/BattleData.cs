using System;
using System.Xml;
using TanatKernel;

[Serializable]
public class BattleData
{
	[Serializable]
	public class Effect
	{
		public string mName;

		public float mTime;

		public FxSkillTarget mTarget;

		public bool mOnDone;

		public virtual void Load(XmlNode _node)
		{
			mName = XmlUtil.SafeReadText("id", _node);
			mTime = XmlUtil.SafeReadFloat("time", _node);
			mTarget = XmlUtil.SafeReadEnum<FxSkillTarget>("target", _node);
			mOnDone = XmlUtil.SafeReadBool("on_done", _node);
		}
	}

	[Serializable]
	public class SoundEffect : Effect
	{
		public string mPath;

		public SoundSystem.SoundOptions mOptions;

		public override void Load(XmlNode _node)
		{
			base.Load(_node);
			mPath = XmlUtil.SafeReadText("path", _node);
			mOptions = new SoundSystem.SoundOptions();
			mOptions.mVolume = XmlUtil.SafeReadFloat("volume", _node);
			mOptions.mWaitLoadingTime = XmlUtil.SafeReadFloat("wait_loading_time", _node);
			mOptions.mReplacePlaying = XmlUtil.SafeReadBool("replace_playing", _node);
		}
	}

	[Serializable]
	public class GraphicsEffect : Effect
	{
		public bool mStopOnDone = true;

		public override void Load(XmlNode _node)
		{
			base.Load(_node);
			mStopOnDone = XmlUtil.SafeReadBool("stop_on_done", _node);
		}
	}

	[Serializable]
	public class EffectHolder
	{
		public string mName;

		public SoundEffect[] mSFX;

		public GraphicsEffect[] mGFX;

		public string mAnimation;

		public bool mLoopAnimation;

		public bool mLockSkills;

		public bool mPreventDisable;

		public void Load(XmlNode _node)
		{
			Load(ref mSFX, _node.SelectNodes("sfx"));
			Load(ref mGFX, _node.SelectNodes("gfx"));
		}

		private void Load<T>(ref T[] _fxs, XmlNodeList _nodes) where T : Effect, new()
		{
			if (_nodes.Count != 0)
			{
				_fxs = new T[_nodes.Count];
				for (int i = 0; i < _nodes.Count; i++)
				{
					_fxs[i] = new T();
					_fxs[i].Load(_nodes[i]);
				}
			}
		}
	}
}
