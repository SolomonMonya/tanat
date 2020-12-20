namespace TanatKernel
{
	public class MapDataDesc
	{
		public MapType mType;

		public int mMapId;

		public int mMaxLevel;

		public int mMinLevel;

		public string mName;

		public string mScene;

		public bool mAvailable;

		public string mDesc;

		public string mWinDesc;

		public int mMinPlayers;

		public int mMaxPlayers;

		public static implicit operator MapData(MapDataDesc _desc)
		{
			MapData mapData = new MapData();
			mapData.mId = _desc.mMapId;
			mapData.mMaxLevel = _desc.mMaxLevel;
			mapData.mMinLevel = _desc.mMinLevel;
			mapData.mName = _desc.mName;
			mapData.mType = (int)_desc.mType;
			mapData.mAvailable = _desc.mAvailable;
			mapData.mDesc = _desc.mDesc;
			mapData.mWinDesc = _desc.mWinDesc;
			mapData.mMaxPlayers = _desc.mMaxPlayers;
			mapData.mMinPlayers = _desc.mMinPlayers;
			mapData.mScene = _desc.mScene;
			return mapData;
		}
	}
}
