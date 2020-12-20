using System.Collections.Generic;

namespace TanatKernel
{
	public interface IClanInfo
	{
		int Id
		{
			get;
		}

		string ClanName
		{
			get;
		}

		string Tag
		{
			get;
		}

		int Level
		{
			get;
		}

		int Rating
		{
			get;
		}

		IEnumerable<Clan.ClanMember> Members
		{
			get;
		}

		bool Contains(int _userId);
	}
}
