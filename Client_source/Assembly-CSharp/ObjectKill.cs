using UnityEngine;

public class ObjectKill : MonoBehaviour
{
	public GameObject mKillTarget;

	public float mMaxDuration;

	private float mCurTime;

	private Explosion mExplosion;

	private void Start()
	{
		mExplosion = mKillTarget.GetComponent<Explosion>();
	}

	private void Update()
	{
		if (!(mExplosion == null))
		{
			if (mCurTime < mMaxDuration)
			{
				mCurTime += Time.deltaTime;
				return;
			}
			mExplosion.Explode();
			mCurTime = 0f;
		}
	}
}
