using System;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
	public delegate void VoidCallback();

	public GameObject mRaceObject;

	public float mCrossFade = 1f;

	public string mColorToChange = "_Emission";

	private Color mMinColor;

	private Color mMaxColor;

	private Color mCurColor;

	private Color mStartColor;

	private bool mSelected;

	private float mSelectedTime;

	private float mSpeed = 3f;

	private Renderer[] mRaceRenderers;

	public bool mIsClickable = true;

	public Action<int> mSelectedCallback;

	public List<int> mAnimationsPriority = new List<int>();

	private Animation mRaceAnimation;

	private AnimationState mCurrentState;

	private List<AnimationState> mAnimations;

	private System.Random mRnd;

	private int mPriority;

	public VoidCallback mOnMouseEnter;

	public VoidCallback mOnMouseLeave;

	public bool mOff;

	public void Start()
	{
		Init();
		if (mRaceObject == null)
		{
			return;
		}
		mAnimations = new List<AnimationState>();
		mRnd = new System.Random();
		mMinColor = new Color(10f / 51f, 10f / 51f, 10f / 51f, 0f);
		mMaxColor = new Color(28f / 51f, 28f / 51f, 28f / 51f, 0f);
		mRaceRenderers = mRaceObject.GetComponentsInChildren<Renderer>();
		mRaceAnimation = mRaceObject.GetComponentInChildren<Animation>();
		foreach (int item in mAnimationsPriority)
		{
			mPriority += item;
		}
		ResetAnimations();
	}

	public void Update()
	{
		if (!mOff)
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
		if (mRaceAnimation == null)
		{
			return;
		}
		if (mCurrentState == null)
		{
			mCurrentState = GetAnimation();
			if (mCurrentState != null)
			{
				mRaceAnimation.CrossFade(mCurrentState.name, mCrossFade, PlayMode.StopSameLayer);
			}
		}
		else if (!mRaceAnimation.IsPlaying(mCurrentState.name))
		{
			mCurrentState = GetAnimation();
			if (mCurrentState != null)
			{
				mRaceAnimation.CrossFade(mCurrentState.name, mCrossFade, PlayMode.StopSameLayer);
			}
		}
	}

	public void OnMouseUp()
	{
		if (mSelectedCallback != null && mIsClickable)
		{
			mSelectedCallback(CurrentValue());
		}
	}

	public virtual int CurrentValue()
	{
		return 0;
	}

	public virtual void Init()
	{
	}

	public void OnMouseEnter()
	{
		mSelected = true;
		mSelectedTime = Time.time;
		mStartColor = mCurColor;
		if (mOnMouseEnter != null && mIsClickable)
		{
			mOnMouseEnter();
		}
	}

	public void OnMouseExit()
	{
		mSelected = false;
		mSelectedTime = Time.time;
		mStartColor = mCurColor;
		if (mOnMouseLeave != null && mIsClickable)
		{
			mOnMouseLeave();
		}
	}

	private AnimationState GetAnimation()
	{
		if (mAnimationsPriority.Count <= 1)
		{
			return mAnimations[0];
		}
		int num = mRnd.Next(mPriority);
		for (int i = 0; i < mAnimationsPriority.Count; i++)
		{
			if (num <= mAnimationsPriority[i])
			{
				if (i < mAnimations.Count)
				{
					return mAnimations[i];
				}
				return null;
			}
			num -= mAnimationsPriority[i];
		}
		return mAnimations[0];
	}

	private void ResetAnimations()
	{
		if (mRaceAnimation == null)
		{
			return;
		}
		foreach (AnimationState item in mRaceAnimation)
		{
			item.wrapMode = WrapMode.Once;
			item.time = 0f;
			item.layer = 0;
			mAnimations.Add(item);
		}
		mCurrentState = GetAnimation();
		if (mCurrentState != null)
		{
			mRaceAnimation.CrossFade(mCurrentState.name, mCrossFade, PlayMode.StopSameLayer);
		}
	}

	private void SetCurColor(Color _color)
	{
		mCurColor = _color;
		Renderer[] array = mRaceRenderers;
		foreach (Renderer renderer in array)
		{
			Material[] materials = renderer.materials;
			foreach (Material material in materials)
			{
				material.SetColor(mColorToChange, _color);
			}
		}
	}
}
