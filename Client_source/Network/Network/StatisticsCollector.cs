using System;
using System.Collections.Generic;
using System.IO;

namespace Network
{
	public class StatisticsCollector<CmdIdT>
	{
		public class Statistics
		{
			public CmdIdT mCmdId;

			public int mCount;

			public long mTraffic;

			public DateTime mFirstTime;

			public DateTime mLastTime;

			public double mAvgFrequency;

			public double mMaxFrequency;
		}

		private class SendData
		{
			public CmdIdT mCmdId;

			public long mTraffic;

			public List<DateTime> mTimeStamps = new List<DateTime>();
		}

		private Dictionary<CmdIdT, SendData> mSendData = new Dictionary<CmdIdT, SendData>();

		public void Register(CmdIdT _packetId, long _packetSize)
		{
			SendData value;
			lock (mSendData)
			{
				if (!mSendData.TryGetValue(_packetId, out value))
				{
					value = new SendData();
					value.mCmdId = _packetId;
					mSendData.Add(_packetId, value);
				}
			}
			lock (value)
			{
				value.mTraffic += _packetSize;
				value.mTimeStamps.Add(DateTime.Now);
			}
		}

		public IEnumerable<Statistics> GetAllStatistics()
		{
			List<SendData> list;
			lock (mSendData)
			{
				list = new List<SendData>(mSendData.Values);
			}
			List<Statistics> list2 = new List<Statistics>();
			foreach (SendData item in list)
			{
				lock (item)
				{
					if (item.mTimeStamps.Count == 0)
					{
						continue;
					}
				}
				Statistics statistics = new Statistics();
				statistics.mCmdId = item.mCmdId;
				lock (item)
				{
					statistics.mCount = item.mTimeStamps.Count;
					statistics.mTraffic = item.mTraffic;
					statistics.mFirstTime = item.mTimeStamps[0];
					statistics.mLastTime = item.mTimeStamps[item.mTimeStamps.Count - 1];
					statistics.mMaxFrequency = 1.0;
					double num = 1.0;
					int num2 = 1;
					int num3 = 1;
					DateTime value = item.mTimeStamps[0];
					for (int i = 1; i < item.mTimeStamps.Count; i++)
					{
						num3++;
						DateTime dateTime = item.mTimeStamps[i];
						TimeSpan timeSpan = dateTime.Subtract(value);
						if (timeSpan.TotalSeconds > 1.0 || i == item.mTimeStamps.Count - 1)
						{
							double num4 = (double)num3 / timeSpan.TotalSeconds;
							value = dateTime;
							num3 = 1;
							num2++;
							if (num4 > statistics.mMaxFrequency)
							{
								statistics.mMaxFrequency = num4;
							}
							num += num4;
						}
					}
					statistics.mAvgFrequency = num / (double)num2;
				}
				list2.Add(statistics);
			}
			return list2;
		}

		public void Clear()
		{
			lock (mSendData)
			{
				mSendData.Clear();
			}
		}

		public static void Save(string _fn, IDictionary<string, IEnumerable<Statistics>> _allStats)
		{
			StreamWriter streamWriter = new StreamWriter(_fn, append: false);
			foreach (KeyValuePair<string, IEnumerable<Statistics>> _allStat in _allStats)
			{
				streamWriter.WriteLine("-------------------- " + _allStat.Key);
				streamWriter.WriteLine("cmd id | count | traffic | first | last | max frequency | avg frequency");
				foreach (Statistics item in _allStat.Value)
				{
					streamWriter.Write(item.mCmdId.ToString());
					streamWriter.Write(" | ");
					streamWriter.Write(item.mCount.ToString());
					streamWriter.Write(" | ");
					streamWriter.Write(item.mTraffic.ToString());
					streamWriter.Write(" | ");
					streamWriter.Write(item.mFirstTime.ToString("HH:mm:ss:fff"));
					streamWriter.Write(" | ");
					streamWriter.Write(item.mLastTime.ToString("HH:mm:ss:fff"));
					streamWriter.Write(" | ");
					streamWriter.Write(item.mMaxFrequency.ToString("F"));
					streamWriter.Write(" | ");
					streamWriter.Write(item.mAvgFrequency.ToString("F"));
					streamWriter.WriteLine();
				}
				streamWriter.WriteLine();
			}
			streamWriter.Flush();
			streamWriter.Close();
		}
	}
}
