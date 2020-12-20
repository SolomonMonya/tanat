using Log4Tanat;
using UnityEngine;

public class Moveable : MonoBehaviour
{
	public delegate void OnStart();

	public delegate void OnDone();

	public OnStart mOnStart;

	public OnDone mOnDone;

	private Transform mCur;

	private Transform mStart;

	private Transform mEnd;

	private bool mDone;

	public void Start()
	{
		CheckTransforms();
		if (mOnStart != null)
		{
			mOnStart();
		}
	}

	public void Update()
	{
		if (!mDone && CheckTransforms())
		{
			SetDirection();
		}
	}

	public void SetTransforms(Transform _cur, Transform _start, Transform _end)
	{
		mCur = _cur;
		mStart = _start;
		mEnd = _end;
		CheckTransforms();
	}

	private bool CheckTransforms()
	{
		if (null == mCur || null == mStart || null == mEnd)
		{
			Log.Error("Some Transforms are null in Moveable. Moveable Stopped");
			Done();
		}
		return !mDone;
	}

	private void Done()
	{
		mDone = true;
		if (mOnDone != null)
		{
			mOnDone();
		}
	}

	private void SetDirection()
	{
	}
}
