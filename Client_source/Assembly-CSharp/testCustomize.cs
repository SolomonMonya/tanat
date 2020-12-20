using UnityEngine;

public class testCustomize : MonoBehaviour
{
	public GameObject mTarget;

	public GameObject mCustom;

	public GameObject mHairs;

	private int mCurHairNum;

	private SkinnedMeshRenderer[] mCurHairs;

	private void Start()
	{
		mCurHairNum = 0;
		Animation component = mTarget.GetComponent<Animation>();
		component.Play();
		if (mHairs != null)
		{
			mCurHairs = mHairs.GetComponentsInChildren<SkinnedMeshRenderer>();
		}
		SetCurCustom();
		SetCurHairs();
	}

	public void OnGUI()
	{
		if (Event.current.type == EventType.MouseDown)
		{
			if (Event.current.button == 0)
			{
				mCurHairNum++;
			}
			else if (Event.current.button == 1)
			{
				mCurHairNum--;
			}
			mCurHairNum = ((mCurHairNum >= 0) ? mCurHairNum : 0);
			mCurHairNum = ((mCurHairNum < mCurHairs.Length) ? mCurHairNum : (mCurHairs.Length - 1));
			SetCurHairs();
		}
	}

	private void SetCurCustom()
	{
		if (mTarget == null || mCustom == null)
		{
			return;
		}
		Customize[] componentsInChildren = mTarget.GetComponentsInChildren<Customize>();
		SkinnedMeshRenderer[] componentsInChildren2 = mCustom.GetComponentsInChildren<SkinnedMeshRenderer>();
		bool flag = false;
		Customize[] array = componentsInChildren;
		foreach (Customize customize in array)
		{
			flag = false;
			SkinnedMeshRenderer[] array2 = componentsInChildren2;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in array2)
			{
				if (skinnedMeshRenderer.name.Contains(customize.gameObject.name))
				{
					flag = true;
					customize.SetMesh(skinnedMeshRenderer.sharedMesh);
					break;
				}
			}
			if (!flag)
			{
				customize.SetMesh(null);
			}
		}
	}

	private void SetCurHairs()
	{
		if (mTarget == null || mCurHairs == null)
		{
			return;
		}
		SkinnedMeshRenderer skinnedMeshRenderer = mCurHairs[mCurHairNum];
		Customize[] componentsInChildren = mTarget.GetComponentsInChildren<Customize>();
		Customize[] array = componentsInChildren;
		foreach (Customize customize in array)
		{
			if (customize.name == "Hair")
			{
				customize.SetMesh(skinnedMeshRenderer.sharedMesh);
				break;
			}
		}
	}
}
