using System;
using System.Collections.Generic;

namespace TanatKernel
{
	public class TransferOperationGroup : BaseTransferOperation<TransferOperationGroup>
	{
		public delegate void ProgressChengedCallback(TransferOperationGroup _group, float _progress);

		public ProgressChengedCallback mProgressChengedCallback;

		private List<TransferOperation> mOperations = new List<TransferOperation>();

		private int mCompleteCnt;

		private int mFailCnt;

		public void Add(TransferOperation _op)
		{
			if (_op == null || mOperations.Contains(_op))
			{
				throw new ArgumentException();
			}
			_op.mNotifiers.Add(new Notifier<TransferOperation, object>(OnComplete, null));
			mOperations.Add(_op);
		}

		private void OnComplete(bool _success, TransferOperation _op, object _data)
		{
			if (_success)
			{
				mLastChunkSize = _op.TransferedSize;
				mTransferedSize += mLastChunkSize;
				if (mProgressChengedCallback != null)
				{
					float progress = (float)(mCompleteCnt + mFailCnt) / (float)mOperations.Count;
					mProgressChengedCallback(this, progress);
				}
				Check(ref mCompleteCnt);
			}
			else
			{
				Check(ref mFailCnt);
			}
		}

		private void Check(ref int _cntToInc)
		{
			bool flag = false;
			lock (this)
			{
				_cntToInc++;
				flag = IsDone();
			}
			if (flag)
			{
				if (mFailCnt > 0)
				{
					mNotifiers.Call(_success: false, this);
				}
				else
				{
					mNotifiers.Call(_success: true, this);
				}
			}
			else
			{
				BeginNextPart(1);
			}
		}

		public override void Begin()
		{
			BeginNextPart(1);
		}

		public void BeginNextPart(int _cnt)
		{
			int num = mCompleteCnt + mFailCnt;
			while (num < mOperations.Count && _cnt > 0)
			{
				mOperations[num].Begin();
				num++;
				_cnt--;
			}
		}

		public override void End()
		{
			foreach (TransferOperation mOperation in mOperations)
			{
				mOperation.End();
			}
			mOperations.Clear();
		}

		public bool IsDone()
		{
			return mCompleteCnt + mFailCnt == mOperations.Count;
		}
	}
}
