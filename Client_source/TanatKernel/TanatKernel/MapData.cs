namespace TanatKernel
{
	public class MapData
	{
		public int mId;

		public string mName;

		public string mScene;

		public int mType;

		public bool mAvailable;

		public bool mUsed;

		public int mMinLevel;

		public int mMaxLevel;

		public string mDesc;

		public string mWinDesc;

		public int mMinPlayers;

		public int mMaxPlayers;

		public static implicit operator MapDataDesc(MapData _data)
		{
			MapDataDesc mapDataDesc = new MapDataDesc();
			mapDataDesc.mMapId = _data.mId;
			mapDataDesc.mMaxLevel = _data.mMaxLevel;
			mapDataDesc.mMinLevel = _data.mMinLevel;
			mapDataDesc.mName = _data.mName;
			mapDataDesc.mType = (MapType)_data.mType;
			mapDataDesc.mAvailable = _data.mAvailable;
			mapDataDesc.mDesc = _data.mDesc;
			mapDataDesc.mWinDesc = _data.mWinDesc;
			mapDataDesc.mMaxPlayers = _data.mMaxPlayers;
			mapDataDesc.mMinPlayers = _data.mMinPlayers;
			mapDataDesc.mScene = _data.mScene;
			return mapDataDesc;
		}
	}
}
