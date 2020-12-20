using TanatKernel;
using UnityEngine;

public class ChildTurn : MonoBehaviour
{
	public string mChildName;

	public float mRotateSpeedDeg = 500f;

	private float mUpdateTargetTime = 0.05f;

	private float mInvisUpdateTargetTime = 2f;

	private Transform mChild;

	private float mStartRotation;

	private float mTargetAngle;

	private float mLastUpdateTargetTime;

	private void Start()
	{
		mChild = base.gameObject.transform.Find(mChildName);
		mStartRotation = mChild.eulerAngles.y;
	}

	private void Update()
	{
		GameData component = base.gameObject.GetComponent<GameData>();
		if (component == null)
		{
			return;
		}
		bool flag = VisibilityMgr.IsVisible(base.gameObject);
		TargetedAction actionWithTarget = component.Data.GetActionWithTarget();
		if (actionWithTarget != null)
		{
			float num = ((!flag) ? mInvisUpdateTargetTime : mUpdateTargetTime);
			if (Time.realtimeSinceStartup - mLastUpdateTargetTime > num)
			{
				IGameObject gameObject = component.Data.GameObjProv.TryGet(actionWithTarget.TargetId);
				if (gameObject != null)
				{
					GameObject gameObject2 = (gameObject as GameData).gameObject;
					Vector3 vector = gameObject2.transform.position - base.gameObject.transform.position;
					mTargetAngle = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
					if (mTargetAngle < 0f)
					{
						mTargetAngle += 360f;
					}
				}
				mLastUpdateTargetTime = Time.realtimeSinceStartup;
			}
		}
		else
		{
			mTargetAngle = mStartRotation;
		}
		float num2 = mTargetAngle - mChild.eulerAngles.y;
		if (Mathf.Abs(num2) > 0.1f)
		{
			if (num2 > 180f)
			{
				num2 -= 360f;
			}
			if (num2 < -180f)
			{
				num2 += 360f;
			}
			if (flag)
			{
				float num3 = mRotateSpeedDeg * Time.smoothDeltaTime;
				num2 = Mathf.Clamp(num2, 0f - num3, num3);
			}
			mChild.Rotate(Vector3.up, num2, Space.World);
		}
	}
}
