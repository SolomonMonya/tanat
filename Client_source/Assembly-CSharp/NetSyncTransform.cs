using TanatKernel;
using UnityEngine;

public class NetSyncTransform : MonoBehaviour, InstanceData.IPosSyncListener
{
	public float mHeight;

	public float mRotateSpeedDegMin = 150f;

	public float mRotateSpeedDeg = 360f;

	public float mRotationAccel = 400f;

	public float mFastRotateSpeedDeg = 1000f;

	private float mYOffset = 0.3f;

	private float mCurRotateSpeed;

	private float mPrevAngle;

	private Vector3 mTargetDir = Vector3.zero;

	private float mSpeed;

	private bool mWaitFirstSync;

	private bool mNextSyncReset;

	private bool mFastRotate;

	private bool mVisible;

	private BasePosPredictor<Vector2> mPredictor = new PosPrediction.SmoothErrorCorrector();

	private GameData mGameData;

	private HeightMap mHeightMap;

	public void SetHeightMap(HeightMap _hm)
	{
		mHeightMap = _hm;
	}

	public void OnEnable()
	{
		base.transform.Rotate(new Vector3(0f, Random.Range(0f, 360f), 0f));
	}

	public void ResetSyncData(bool _visible)
	{
		mVisible = _visible;
		mWaitFirstSync = true;
	}

	public void InstantResetSyncData()
	{
		if (!(null == mGameData) && mGameData.Data != null && !mWaitFirstSync)
		{
			Reset(mGameData.Data.Time, _forceMoveCam: true);
		}
	}

	public void NextSyncReset()
	{
		mNextSyncReset = true;
	}

	public void ProcessNetInput(InstanceData _data)
	{
		SyncedParams @params = _data.Params;
		float mPosSnapshotTime = @params.mPosSnapshotTime;
		Vector2 pos = new Vector2(@params.mX, @params.mY);
		Vector2 vel = new Vector2(@params.mVelX, @params.mVelY);
		if (mNextSyncReset && BattleServerConnection.mIsReady)
		{
			ResetSyncData(_visible: false);
		}
		if (mGameData == null)
		{
			mGameData = base.gameObject.GetComponent<GameData>();
		}
		float time = mGameData.Data.Time;
		mPredictor.Adjust(time, mPosSnapshotTime, pos, vel);
		if (mWaitFirstSync)
		{
			Reset(time, !mVisible);
			mWaitFirstSync = false;
			mNextSyncReset = false;
			mVisible = false;
		}
	}

	private void Reset(float clientTime, bool _forceMoveCam)
	{
		mPredictor.Reset();
		Vector2 position = mPredictor.GetPosition(clientTime);
		float y = HeightMap.GetY(mHeightMap, position.x, position.y);
		y += mHeight;
		base.gameObject.transform.position = new Vector3(position.x, y, position.y);
		TrySetTargetDir(clientTime);
		if (mTargetDir != Vector3.zero)
		{
			base.gameObject.transform.forward = mTargetDir;
		}
		if (mNextSyncReset || !(Camera.main != null) || !mGameData.Data.IsPlayerBinded)
		{
			return;
		}
		Player player = mGameData.Data.Player;
		if (player == null || !player.IsSelf)
		{
			return;
		}
		GameCamera component = Camera.main.GetComponent<GameCamera>();
		if (component != null)
		{
			component.SetPlayer(base.gameObject, player.Id);
			if (_forceMoveCam)
			{
				component.ForceMove(base.gameObject.transform.position);
			}
		}
	}

	protected void UpdatePosition(float _time)
	{
		Vector3 position;
		if (BattleServerConnection.mIsReady)
		{
			position = base.gameObject.transform.position;
			Vector2 _pos = new Vector2(position.x, position.z);
			float num = mPredictor.CorrectPosition(_time, ref _pos);
			mSpeed = num / Time.smoothDeltaTime;
			if (mGameData != null)
			{
				mGameData.Data.mSpeed = mSpeed;
			}
			position.x = _pos.x;
			position.z = _pos.y;
			position.y = HeightMap.GetY(mHeightMap, _pos.x, _pos.y) + mHeight + mYOffset;
		}
		else
		{
			if (!(_time > 0f))
			{
				NextSyncReset();
				return;
			}
			Vector2 position2 = mPredictor.GetPosition(_time);
			float y = HeightMap.GetY(mHeightMap, position2.x, position2.y) + mHeight;
			position = new Vector3(position2.x, y, position2.y);
		}
		base.gameObject.transform.position = position;
	}

	protected void UpdateRotation(float _time)
	{
		TargetedAction targetedAction = null;
		if (mGameData.Data.DoingAction)
		{
			targetedAction = mGameData.Data.GetActionWithTarget();
		}
		if (targetedAction != null)
		{
			IGameObject gameObject = mGameData.Data.GameObjProv.TryGet(targetedAction.TargetId);
			if (gameObject != null)
			{
				Vector3 position = (gameObject as GameData).gameObject.transform.position;
				RotateTo(position.x, position.z, mFastRotate);
			}
		}
		else if (targetedAction == null && !mFastRotate)
		{
			TrySetTargetDir(_time);
		}
		float gapAngle = GetGapAngle();
		if (gapAngle > 1f)
		{
			if (gapAngle > mPrevAngle)
			{
				mCurRotateSpeed = mRotateSpeedDegMin;
			}
			float num = ((!mFastRotate) ? mCurRotateSpeed : mFastRotateSpeedDeg);
			float num2 = num * Time.smoothDeltaTime;
			base.gameObject.transform.forward = Vector3.Slerp(base.gameObject.transform.forward, mTargetDir, num2 / gapAngle);
			float num3 = mRotationAccel * Time.deltaTime;
			mCurRotateSpeed += num3;
			if (mCurRotateSpeed > mRotateSpeedDeg)
			{
				mCurRotateSpeed = mRotateSpeedDeg;
			}
			mPrevAngle = gapAngle;
		}
		else
		{
			mFastRotate = false;
		}
	}

	private void TrySetTargetDir(float _time)
	{
		Vector2 velocity = mPredictor.GetVelocity(_time);
		if (Mathf.Abs(velocity.sqrMagnitude) > 0.001f)
		{
			mTargetDir.x = velocity.x;
			mTargetDir.z = velocity.y;
			mTargetDir.Normalize();
		}
	}

	public void Update()
	{
		if (!(mGameData == null) && mGameData.Data.Relevant)
		{
			float time = mGameData.Data.Time;
			UpdatePosition(time);
			UpdateRotation(time);
		}
	}

	public void RotateTo(float _x, float _z, bool _fast)
	{
		Vector3 vector = new Vector3(_x, 0f, _z);
		Vector3 vector2 = vector - base.gameObject.transform.position;
		vector2.y = 0f;
		if (Mathf.Abs(vector2.sqrMagnitude) > 0.001f)
		{
			mTargetDir.x = vector2.x;
			mTargetDir.z = vector2.z;
			mTargetDir.Normalize();
		}
		mFastRotate = _fast;
	}

	public bool IsMoving()
	{
		return mSpeed > 0.1f;
	}

	public float GetSpeed()
	{
		return mSpeed;
	}

	public float GetGapAngle()
	{
		return (!(mTargetDir != Vector3.zero)) ? 0f : Vector3.Angle(base.gameObject.transform.forward, mTargetDir);
	}

	public void ForceStop()
	{
		if (!(mGameData == null) && mGameData.Data != null)
		{
			float time = mGameData.Data.Time;
			Vector3 position = base.gameObject.transform.position;
			mPredictor.Adjust(time, time, new Vector2(position.x, position.z), Vector2.zero);
			mPredictor.Reset();
		}
	}
}
