using System.Collections.Generic;
using Log4Tanat;
using UnityEngine;

[AddComponentMenu("FXs/Lightning")]
public class Lightning : MultiTargetsEffect
{
	private class KeyPos
	{
		public Vector3 mCurPos;

		public Vector3 mChangePos;

		public KeyPos()
		{
			mCurPos = Vector3.zero;
			mChangePos = Vector3.zero;
		}
	}

	private LineRenderer mLineRenderer;

	private Transform mStartTransform;

	private Transform mEndTransform;

	private Dictionary<int, KeyPos> mPos = new Dictionary<int, KeyPos>();

	private int mLastFrame;

	private float mCurTime;

	private float mCountMin = 3f;

	private int mCount;

	private Vector3 mDirection = Vector3.zero;

	private float mMagnitude;

	public float mOffsetYSpeed = 0.25f;

	public float mOffsetYPos = 1.5f;

	public float mXRange = 0.5f;

	public float mYRange = 0.5f;

	public float mZRange = 0.5f;

	public int mChangeSpeed = 5;

	public float mMaxTime = 0.3f;

	public float mMagnitudeK = 1f;

	public float mChangeAlpha = 0.1f;

	private void Update()
	{
		if (mDone)
		{
			return;
		}
		if (mCurTime < mMaxTime)
		{
			mCurTime += Time.deltaTime;
			if ((mChangeSpeed > 0 && Time.frameCount % mChangeSpeed == 0) || mChangeSpeed == 0)
			{
				InitVertexPos(ref mPos);
				GenerateVertexPos(ref mPos);
				mLastFrame = Time.frameCount;
			}
			else
			{
				for (int i = 1; i < mCount - 1; i++)
				{
					float num = (float)(Time.frameCount - mLastFrame) / (float)mChangeSpeed;
					Vector3 position = mPos[i].mCurPos + mPos[i].mChangePos * num;
					mLineRenderer.SetPosition(i, position);
				}
			}
			if (mLineRenderer.material.HasProperty("_TintColor"))
			{
				Color color = mLineRenderer.material.GetColor("_TintColor");
				color.a -= mChangeAlpha;
				if (color.a < 0f)
				{
					color.a = 0f;
				}
				mLineRenderer.material.SetColor("_TintColor", color);
			}
		}
		else
		{
			mDone = true;
		}
	}

	public override bool SetTargets(List<GameObject> _targets)
	{
		if (_targets.Count != 2 || !base.SetTargets(_targets))
		{
			Log.Error("Bad targets in lightning");
			return false;
		}
		SetStartTransform(_targets[0].transform);
		SetEndTransform(_targets[1].transform);
		return true;
	}

	public override void Init()
	{
		mLineRenderer = base.gameObject.GetComponent(typeof(LineRenderer)) as LineRenderer;
		InitVertexPos(ref mPos);
		GenerateVertexPos(ref mPos);
		mLastFrame = Time.frameCount;
	}

	public void SetStartTransform(Transform _transform)
	{
		mStartTransform = _transform;
	}

	public void SetEndTransform(Transform _transform)
	{
		mEndTransform = _transform;
	}

	private void GenerateVertexPos(ref Dictionary<int, KeyPos> _vertexPos)
	{
		for (int i = 1; i < mCount - 1; i++)
		{
			Vector3 zero = Vector3.zero;
			zero.x = Random.Range(0f - mXRange, mXRange);
			zero.y = mOffsetYPos + Random.Range(0f - mYRange, mYRange);
			zero.z = Random.Range(0f - mZRange, mZRange);
			_vertexPos[i].mChangePos = zero;
		}
	}

	private void InitVertexPos(ref Dictionary<int, KeyPos> _vertexPos)
	{
		if (!(null == mEndTransform) && !(null == mStartTransform))
		{
			_vertexPos.Clear();
			mDirection = mEndTransform.position - mStartTransform.position;
			mMagnitude = mDirection.magnitude;
			mDirection.Normalize();
			mCount = (int)Random.Range(mCountMin, mCountMin + mMagnitude * mMagnitudeK);
			mLineRenderer.SetVertexCount(mCount);
			for (int i = 0; i < mCount; i++)
			{
				Vector3 vector = mStartTransform.transform.position + mDirection * mMagnitude * ((float)i / (float)(mCount - 1));
				vector.y += mOffsetYPos;
				mLineRenderer.SetPosition(i, vector);
				KeyPos keyPos = new KeyPos();
				keyPos.mCurPos = vector;
				_vertexPos.Add(i, keyPos);
			}
		}
	}
}
