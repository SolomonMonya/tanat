using System;
using System.Collections.Generic;
using AMF;

namespace TanatKernel
{
	public class UserLog
	{
		private class SimpleAction
		{
			public DateTime mTime;

			public UserActionType mType;

			public int mThingId;

			public string mComment;

			public SimpleAction(UserActionType _type)
			{
				mTime = DateTime.Now;
				mType = _type;
			}

			public override string ToString()
			{
				return mTime.ToString() + " " + mType.ToString() + " " + mThingId + " " + mComment;
			}
		}

		private static volatile UserLog mInstance = new UserLog();

		private List<SimpleAction> mActions = new List<SimpleAction>();

		private CtrlServerConnection mSender;

		private int mNeedSend;

		private UserLog()
		{
		}

		public static void SetData(CtrlServerConnection _sender, int _needSend)
		{
			mInstance.mSender = _sender;
			mInstance.mNeedSend = _needSend;
		}

		public static void AddAction(UserActionType _type)
		{
			mInstance.Save(new SimpleAction(_type));
		}

		public static void AddAction(UserActionType _type, int _thingId)
		{
			SimpleAction simpleAction = new SimpleAction(_type);
			simpleAction.mThingId = _thingId;
			mInstance.Save(simpleAction);
		}

		public static void AddAction(UserActionType _type, int _thingId, string _comment)
		{
			SimpleAction simpleAction = new SimpleAction(_type);
			simpleAction.mThingId = _thingId;
			simpleAction.mComment = _comment;
			mInstance.Save(simpleAction);
		}

		public static void AddAction(UserActionType _type, string _comment)
		{
			SimpleAction simpleAction = new SimpleAction(_type);
			simpleAction.mComment = _comment;
			mInstance.Save(simpleAction);
		}

		private void Save(SimpleAction _sa)
		{
			if (mNeedSend > 0)
			{
				mActions.Add(_sa);
				if (mActions.Count >= mNeedSend)
				{
					mInstance.SendLogToServer();
				}
			}
		}

		public static void SaveLog()
		{
			mInstance.SendLogToServer();
		}

		private void SendLogToServer()
		{
			if (mSender == null)
			{
				return;
			}
			MixedArray mixedArray = new MixedArray();
			foreach (SimpleAction mAction in mActions)
			{
				MixedArray mixedArray2 = new MixedArray();
				mixedArray2.Associative.Add("type", (int)mAction.mType);
				mixedArray2.Associative.Add("param", mAction.mThingId);
				mixedArray2.Associative.Add("comment", mAction.mComment);
				mixedArray2.Associative.Add("date", mAction.mTime.ToString("yyyy-MM-dd HH:mm:ss"));
				mixedArray.Dense.Add(mixedArray2);
			}
			mSender.SendLog(mixedArray);
			mActions.Clear();
		}
	}
}
