using UnityEngine;

public class Blink : MonoBehaviour
{
	public string mColorToChange = "_Emission";

	public int mId;

	private Renderer[] mRaceRenderers;

	private Color mMinColor;

	private Color mMaxColor;

	private Color mCurColor;

	private Color mStartColor;

	private bool mSelected;

	private float mSelectedTime;

	public float mSpeed = 1f;

	public bool mOff = true;

	private void Start()
	{
		mMinColor = new Color(10f / 51f, 10f / 51f, 10f / 51f, 0f);
		mMaxColor = new Color(28f / 51f, 28f / 51f, 28f / 51f, 0f);
		mRaceRenderers = GetComponentsInChildren<Renderer>();
	}

	private void Update()
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
			else
			{
				mSelectedTime = Time.time;
				mSelected = !mSelected;
				mStartColor = mCurColor;
			}
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
