using System;
using AMF;

namespace TanatKernel
{
	public class MapSender
	{
		private CtrlEntryPoint mEntryPoint;

		public MapSender(CtrlEntryPoint _entryPoint)
		{
			if (_entryPoint == null)
			{
				throw new ArgumentNullException("_entryPoint");
			}
			mEntryPoint = _entryPoint;
		}

		public void MapList(int _mapType)
		{
			mEntryPoint.Send(CtrlCmdId.arena.get_maps, new NamedVar("type", _mapType));
		}

		public void MapInfo(int _mapType, int _mapId)
		{
			mEntryPoint.Send(CtrlCmdId.arena.get_maps, new NamedVar("type", _mapType), new NamedVar("map_id", _mapId));
		}

		public void MapsInfo()
		{
			mEntryPoint.Send(CtrlCmdId.arena.get_maps_info);
		}

		public void JoinQueue(int _mapType, int _mapId)
		{
			mEntryPoint.Send(CtrlCmdId.arena.join_queue, new NamedVar("type", _mapType), new NamedVar("map_id", _mapId));
		}

		public void Join(int _mapType, int _mapId)
		{
			mEntryPoint.Send(CtrlCmdId.arena.join_request, new NamedVar("type", _mapType), new NamedVar("map_id", _mapId));
		}

		public void RandomJoin()
		{
			mEntryPoint.Send(CtrlCmdId.arena.join_random);
		}

		public void AcceptJoinQueue(int _mapType, int _mapId, int _requestId)
		{
			mEntryPoint.Send(CtrlCmdId.arena.join_request, new NamedVar("type", _mapType), new NamedVar("map_id", _mapId), new NamedVar("request_id", _requestId));
		}

		public void JoinWithGroup(int _mapType, int _mapId, int _requestId, int _team, int _group, bool _needMap)
		{
			mEntryPoint.Send(CtrlCmdId.arena.join_queue, new NamedVar("type", _mapType), new NamedVar("map_id", _mapId), new NamedVar("request_id", _requestId), new NamedVar("team", _team), new NamedVar("group", _group), new NamedVar("get_map", _needMap));
		}

		public void SelectAvatar(int _avatarId)
		{
			mEntryPoint.Send(CtrlCmdId.arena.select_avatar, new NamedVar("avatar", _avatarId));
		}

		public void Ready()
		{
			mEntryPoint.Send(CtrlCmdId.arena.ready);
		}

		public void Ready(int _mapId, int _avatarId)
		{
			mEntryPoint.Send(CtrlCmdId.arena.ready, new NamedVar("map_id", _mapId), new NamedVar("avatar", _avatarId));
		}

		public void Desert(int _requestId, int _mapType)
		{
			mEntryPoint.Send(CtrlCmdId.arena.desert, new NamedVar("request_id", _requestId), new NamedVar("type", _mapType));
		}

		public void Desert(int _mapType)
		{
			mEntryPoint.Send(CtrlCmdId.arena.desert, new NamedVar("type", _mapType));
		}

		public void GetMapTypeDescs()
		{
			mEntryPoint.Send(CtrlCmdId.arena.get_map_type_descs);
		}
	}
}
