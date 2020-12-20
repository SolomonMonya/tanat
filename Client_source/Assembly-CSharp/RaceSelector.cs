using System;
using UnityEngine;

[AddComponentMenu("MainMenu/RaceSelector")]
public class RaceSelector : MonoBehaviour
{
	public GameObject mRaceObject;

	public float mCrossFade = 0.3f;

	public HeroRace mRace;

	private Animation mRaceAnimation;

	private SkinnedMeshRenderer[] mRaceRenderers;

	private Color mMinColor;

	private Color mMaxColor;

	private Color mCurColor;

	private Color mStartColor;

	private bool mSelected;

	private float mSelectedTime;

	private float mSpeed = 3f;

	public Action<HeroRace> mSelectedCallback;

	public void Start()
	{
		if (!(mRaceObject == null))
		{
			mMinColor = new Color(0.3529412f, 0.3529412f, 0.3529412f, 0f);
			mMaxColor = new Color(28f / 51f, 28f / 51f, 28f / 51f, 0f);
			mRaceAnimation = mRaceObject.GetComponentInChildren<Animation>();
			mRaceRenderers = mRaceObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			ResetAnimations();
		}
	}

	public void OnEnable()
	{
		if (!(mRaceAnimation == null))
		{
			mRaceAnimation.Play("Idle01");
		}
	}

	public void Update()
	{
		if (mSelected && mCurColor != mMaxColor)
		{
			Color curColor = Color.Lerp(mStartColor, mMaxColor, (Time.time - mSelectedTime) * mSpeed);
			SetCurColor(curColor);
		}
		else if (!mSelected && mCurColor != mMinColor)
		{
			Color curColor2 = Color.Lerp(mStartColor, mMinColor, (Time.time - mSelectedTime) * mSpeed);
			SetCurColor(curColor2);
		}
	}

	public void OnMouseEnter()
	{
		mSelected = true;
		mSelectedTime = Time.time;
		mStartColor = mCurColor;
		mRaceAnimation.CrossFade("Idle02", mCrossFade, PlayMode.StopSameLayer);
	}

	public void OnMouseExit()
	{
		mSelected = false;
		mSelectedTime = Time.time;
		mStartColor = mCurColor;
		mRaceAnimation.CrossFade("Idle01", mCrossFade, PlayMode.StopSameLayer);
	}

	public void OnMouseUp()
	{
		if (mSelectedCallback != null)
		{
			mSelectedCallback(mRace);
		}
	}

	private void ResetAnimations()
	{
		if (mRaceAnimation == null)
		{
			return;
		}
		foreach (AnimationState item in mRaceAnimation)
		{
			item.wrapMode = WrapMode.Loop;
			item.time = 0f;
			item.layer = 0;
		}
	}

	private void SetCurColor(Color _color)
	{
		mCurColor = _color;
		SkinnedMeshRenderer[] array = mRaceRenderers;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			Material[] materials = skinnedMeshRenderer.materials;
			foreach (Material material in materials)
			{
				material.SetColor("_Emission", _color);
			}
		}
	}
}
