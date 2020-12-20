namespace TanatKernel
{
	public class TargetedAction
	{
		private int mActionId;

		private int mTargetId;

		private float mStartTime;

		public int TargetId => mTargetId;

		public int ActionId => mActionId;

		public float StartTime => mStartTime;

		public bool HasTarget => mTargetId != -1;

		public TargetedAction(int _actionId, int _targetId, float _startTime)
		{
			mActionId = _actionId;
			mTargetId = _targetId;
			mStartTime = _startTime;
		}

		public override string ToString()
		{
			return "action { id: " + mActionId + ", target id: " + mTargetId + "}";
		}
	}
}
