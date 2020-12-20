using System;
using UnityEngine;

public class TransparencyTrigger : MonoBehaviour
{
	[Serializable]
	public class HideOptions
	{
		public GameObject mObj;

		public string mShader;

		public float mHideAlpha = 0.3f;

		public float mHideSpeed = 0.5f;

		public float mShowSpeed = 0.8f;

		private Shader mBackupShader;

		public void Hide()
		{
			if (!(null == mObj) && !(null != mObj.GetComponent<SmoothTransparency>()))
			{
				mBackupShader = mObj.renderer.material.shader;
				Shader shader = Shader.Find(mShader);
				mObj.renderer.material.shader = shader;
				SmoothTransparency smoothTransparency = mObj.AddComponent<SmoothTransparency>();
				smoothTransparency.mEnd = mHideAlpha;
				smoothTransparency.mSpeed = mHideSpeed;
				smoothTransparency.Init();
			}
		}

		public void Show()
		{
			if (!(null == mObj))
			{
				SmoothTransparency smoothTransparency = mObj.GetComponent<SmoothTransparency>();
				if (null == smoothTransparency)
				{
					smoothTransparency = mObj.AddComponent<SmoothTransparency>();
				}
				smoothTransparency.mEnd = 1f;
				smoothTransparency.mSpeed = mShowSpeed;
				smoothTransparency.mFinalShader = mBackupShader;
				mBackupShader = null;
				smoothTransparency.Init();
			}
		}
	}

	public HideOptions[] mHideOpts;

	private bool IsValid(Collider _collider)
	{
		return false;
	}

	public void OnTriggerEnter(Collider _collider)
	{
		if (IsValid(_collider))
		{
			HideOptions[] array = mHideOpts;
			foreach (HideOptions hideOptions in array)
			{
				hideOptions.Hide();
			}
		}
	}

	public void OnTriggerExit(Collider _collider)
	{
		if (IsValid(_collider))
		{
			HideOptions[] array = mHideOpts;
			foreach (HideOptions hideOptions in array)
			{
				hideOptions.Show();
			}
		}
	}
}
