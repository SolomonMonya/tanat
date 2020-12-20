using System.Collections.Generic;

namespace TanatKernel
{
	public class MapAvatarData
	{
		public int mId;

		public bool mAvailable;

		public bool mDenyForMap;

		public List<AvatarRestriction> mRestrictions = new List<AvatarRestriction>();
	}
}
