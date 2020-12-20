using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using AMF;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class CtrlEntryPoint : PacketManager<CtrlPacket, Enum>, WebTransferOperation.CookieReceiver, ICtrlResponseHolder
	{
		public delegate void ConnectionErrorCallback();

		private ConnectionErrorCallback mConnectionErrorCallback = delegate
		{
		};

		private UserNetData mUserData;

		private string mHost = "localhost";

		private int mPort = 80;

		private static uint mCounter;

		private Session mSession = new Session(new string[2]
		{
			"sess_key",
			"sess_uid"
		});

		private CtrlParser mParser = new CtrlParser();

		private Queue<CtrlPacket> mResponses = new Queue<CtrlPacket>();

		private Queue<WebTransferOperation> mRequests = new Queue<WebTransferOperation>();

		private WebTransferOperation mLastRequest;

		private DateTime mLastSendTime;

		private float mPingTime = 60f;

		private float mDisconnectTime = 15f;

		private bool mPingEnabled;

		private Formatter mInFormatter = new Formatter();

		private Formatter mOutFormatter = new Formatter();

		[CompilerGenerated]
		private static ConnectionErrorCallback _003C_003E9__CachedAnonymousMethodDelegate1;

		[CompilerGenerated]
		private static ConnectionErrorCallback _003C_003E9__CachedAnonymousMethodDelegate3;

		public UserNetData UserData => mUserData;

		public string Address => "http://" + Host + ":" + Port + "/";

		public string FullAddress => Address + "entry_point.php";

		public string Host => mHost;

		public string Port => mPort.ToString();

		public Session Session => mSession;

		public float PingTime
		{
			set
			{
				if (value > 0f)
				{
					mPingTime = value;
				}
				else
				{
					Log.Warning("invalid ping time: " + value);
				}
			}
		}

		public float DisconnectTime
		{
			set
			{
				if (value > 0f)
				{
					mDisconnectTime = value;
				}
				else
				{
					Log.Warning("invalid disconnect time: " + value);
				}
			}
		}

		public CtrlEntryPoint(UserNetData _userData)
		{
			if (_userData == null)
			{
				throw new ArgumentNullException("_userData");
			}
			mUserData = _userData;
			CtrlPacketValidator validator = new CtrlPacketValidator();
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.login, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.conf, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.all_info, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.game_info, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.get_global_buffs, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.update_buffs_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.update_global_buffs_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.personal_details, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.bag, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.dress, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.dress_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.undress, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.undress_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.dressed_items, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.money, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.money_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.common.area_conf, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.common.hero_conf, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.common.dummy, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.common.init, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.common.can_reconnect, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.common.reconnect, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.common.action, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.hero.get_data, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.hero.get_data_list, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.hero.get_data_list_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.hero.create, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.get_maps, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.join_request, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.join_request_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.select_avatar, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.select_avatar_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.ready, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.ready_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.desert, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.desert_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.launch_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.join_queue_invite_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.join_invite_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.get_map_type_descs, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.get_maps_info, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.log, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.store.list, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.store.buy, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.store.sell, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.chat.conf, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.chat.add, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.chat.message_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.chat.set_gag, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.chat.set_gag_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.trade.start, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.trade.start_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.trade.start_answer, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.trade.start_answer_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.trade.ready, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.trade.ready_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.trade.not_ready, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.trade.not_ready_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.trade.cancel, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.trade.cancel_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.trade.confirm, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.trade.confirm_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.log, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.bank.change, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.group_list, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.joined_to_group_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.removed_from_group_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.join_from_group_request, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.join_from_group_request_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.join_from_group_answer, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.join_from_group_answer_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.join_to_group_request, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.join_to_group_request_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.join_to_group_answer, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.join_to_group_answer_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.leave_group, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.change_leader, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.remove_from_group, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.remove_from_group_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.group_deleted_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.group_leader_changed_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.online_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.offline_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.join_queue, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.join_queue_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.join_queue_ready_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.arena.join_random, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.drop_artifact, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.update_info_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.npc.list, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.quest.update, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.quest.update_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.quest.accept, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.quest.cancel, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.quest.done, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.clan.change_role, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.clan.create, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.clan.info, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.clan.info_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.clan.invite, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.clan.invite_answer, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.clan.invite_answer_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.clan.invite_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.clan.remove, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.clan.remove_user, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.clan.remove_user_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.system.ping_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.battle_info, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.desert, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.fighters, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.history, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.info, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.list, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.set_fighters, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.common.message_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.start_battle_info_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.start_request_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.select_avatar_timer_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.select_avatar, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.select_avatar_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.ready, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.ready_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.desert_battle, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.desert_battle_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.launch_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.castle.early_won_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.common.reconnected, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.full_hero_info, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.get_bw_list, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.add_to_list, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.remove_from_list, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.add_to_list_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.find, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.friend_request_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.friend_answer, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.get_buffs, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.remove_from_list_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.get_observer_info, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.join, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.desert, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.invite_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.answer, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.start_select_avatar_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.select_avatar, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.select_avatar_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.desert_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.launch_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.in_request, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.in_request_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.ready, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.fight.ready_mpd, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.hunt.join, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.hunt.ready, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.user.leave_info, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.common.user_log, validator);
			base.HandlerMgr.RegisterValidator(CtrlCmdId.common.day_reward_mpd, validator);
			base.HandlerMgr.RegisterParser(new LoginArgParser());
			base.HandlerMgr.RegisterParser(new HeroStatsArgParser());
			base.HandlerMgr.RegisterParser(new HeroBuffsUpdateArgMpdParser());
			base.HandlerMgr.RegisterParser(new GlobalBuffsUpdateMpdArgParser());
			base.HandlerMgr.RegisterParser(new HeroDataArgParser());
			base.HandlerMgr.RegisterParser(new HeroDataListArgParser());
			base.HandlerMgr.RegisterParser(new HeroDataListMpdArgParser());
			base.HandlerMgr.RegisterParser(new ServerDataArgParser());
			base.HandlerMgr.RegisterParser(new MapListArgParser());
			base.HandlerMgr.RegisterParser(new MapInfoArgParser());
			base.HandlerMgr.RegisterParser(new JoinArgParser());
			base.HandlerMgr.RegisterParser(new HeroGameInfoArgParser());
			base.HandlerMgr.RegisterParser(new UserPersonalInfoArgParser());
			base.HandlerMgr.RegisterParser(new ShopContentArgParser());
			base.HandlerMgr.RegisterParser(new UserBagArgParser());
			base.HandlerMgr.RegisterParser(new UpdateBagArgMpdParser());
			base.HandlerMgr.RegisterParser(new DressedItemsArgParser());
			base.HandlerMgr.RegisterParser(new HeroMoneyArgParser());
			base.HandlerMgr.RegisterParser(new HeroMoneyMpdArgParser());
			base.HandlerMgr.RegisterParser(new ChatConfArgParser());
			base.HandlerMgr.RegisterParser(new TradeStartMpdArgParser());
			base.HandlerMgr.RegisterParser(new TradeStartAnsMpdArgParser());
			base.HandlerMgr.RegisterParser(new TradeReadyArgParser());
			base.HandlerMgr.RegisterParser(new ChatMsgArgParser());
			base.HandlerMgr.RegisterParser(new ChatGagArgParser());
			base.HandlerMgr.RegisterParser(new DressItemArgParser());
			base.HandlerMgr.RegisterParser(new NewDressedItemArgMpdParser());
			base.HandlerMgr.RegisterParser(new UndressedItemArgMpdParser());
			base.HandlerMgr.RegisterParser(new SelectAvatarMpdArgParser());
			base.HandlerMgr.RegisterParser(new PlayerJoinedMpdArgParser());
			base.HandlerMgr.RegisterParser(new PlayerDesertedMpdArgParser());
			base.HandlerMgr.RegisterParser(new PlayerReadyMpdArgParser());
			base.HandlerMgr.RegisterParser(new LaunchMpdArgParser());
			base.HandlerMgr.RegisterParser(new FightLogArgParser());
			base.HandlerMgr.RegisterParser(new CanReconnectArgParser());
			base.HandlerMgr.RegisterParser(new ReconnectArgParser());
			base.HandlerMgr.RegisterParser(new JoinInviteArgMpdParser());
			base.HandlerMgr.RegisterParser(new GroupListArgParser());
			base.HandlerMgr.RegisterParser(new JoinedToGroupArgMpdParser());
			base.HandlerMgr.RegisterParser(new RemovedFromGroupArgMpdParser());
			base.HandlerMgr.RegisterParser(new JoinFromGroupRequestArgMpdParser());
			base.HandlerMgr.RegisterParser(new JoinFromGroupAnswerArgMpdParser());
			base.HandlerMgr.RegisterParser(new JoinToGroupRequestArgMpdParser());
			base.HandlerMgr.RegisterParser(new JoinToGroupAnswerArgMpdParser());
			base.HandlerMgr.RegisterParser(new RemoveFromGroupArgMpdParser());
			base.HandlerMgr.RegisterParser(new LeaderChangedArgMpdParser());
			base.HandlerMgr.RegisterParser(new JoinToGroupArgParser());
			base.HandlerMgr.RegisterParser(new JoinFromGroupArgParser());
			base.HandlerMgr.RegisterParser(new CtrlOnlineArgMpdParser());
			base.HandlerMgr.RegisterParser(new CtrlOfflineArgMpdParser());
			base.HandlerMgr.RegisterParser(new DesertArgParser());
			base.HandlerMgr.RegisterParser(new MapTypeDescsArgParser());
			base.HandlerMgr.RegisterParser(new JoinQueueArgParser());
			base.HandlerMgr.RegisterParser(new JoinQueueStateArgMpdParser());
			base.HandlerMgr.RegisterParser(new JoinQueueReadyArgMpdParser());
			base.HandlerMgr.RegisterParser(new HeroesInfoUpdateArgMpdParser());
			base.HandlerMgr.RegisterParser(new NpcListArgParser());
			base.HandlerMgr.RegisterParser(new QuestListArgParser());
			base.HandlerMgr.RegisterParser(new QuestUpdateArgMpdParser());
			base.HandlerMgr.RegisterParser(new InfoMpdArgParser());
			base.HandlerMgr.RegisterParser(new InfoArgParser());
			base.HandlerMgr.RegisterParser(new RemoveUserMpdArgParser());
			base.HandlerMgr.RegisterParser(new InviteMpdArgParser());
			base.HandlerMgr.RegisterParser(new InviteAnswerMpdArgParser());
			base.HandlerMgr.RegisterParser(new RemoveUserArgParser());
			base.HandlerMgr.RegisterParser(new CreateArgParser());
			base.HandlerMgr.RegisterParser(new ActionUseArgParser());
			base.HandlerMgr.RegisterParser(new CastleListArgParser());
			base.HandlerMgr.RegisterParser(new CastleMembersArgParser());
			base.HandlerMgr.RegisterParser(new CastleHistoryArgParser());
			base.HandlerMgr.RegisterParser(new CastleBattleInfoArgParser());
			base.HandlerMgr.RegisterParser(new CastleFighterListArgParser());
			base.HandlerMgr.RegisterParser(new DeferredMessageArgParser());
			base.HandlerMgr.RegisterParser(new CastleStartBattleInfoArgParser());
			base.HandlerMgr.RegisterParser(new CastleStartRequestArgParser());
			base.HandlerMgr.RegisterParser(new CastleSelectAvatarTimerArgParser());
			base.HandlerMgr.RegisterParser(new CastleSelectAvatarArgParser());
			base.HandlerMgr.RegisterParser(new CastleReadyArgParser());
			base.HandlerMgr.RegisterParser(new CastleDesertArgParser());
			base.HandlerMgr.RegisterParser(new CastleLaunchArgParser());
			base.HandlerMgr.RegisterParser(new FullHeroInfoArgParser());
			base.HandlerMgr.RegisterParser(new BwListArgParser());
			base.HandlerMgr.RegisterParser(new RemoveFromListArgParser());
			base.HandlerMgr.RegisterParser(new AddToListMpdArgParser());
			base.HandlerMgr.RegisterParser(new FindResultArgParser());
			base.HandlerMgr.RegisterParser(new FriendRequestMpdArgParser());
			base.HandlerMgr.RegisterParser(new RemoveFromListMpdArgParser());
			base.HandlerMgr.RegisterParser(new ObserverArgParser());
			base.HandlerMgr.RegisterParser(new FightInviteMpdArgParser());
			base.HandlerMgr.RegisterParser(new FightStartSelectAvatarMpdArgParser());
			base.HandlerMgr.RegisterParser(new FightSelectAvatarMpdArgParser());
			base.HandlerMgr.RegisterParser(new FightDesertMpdArgParser());
			base.HandlerMgr.RegisterParser(new FightLaunchArgParser());
			base.HandlerMgr.RegisterParser(new FightInRequestMpdParser());
			base.HandlerMgr.RegisterParser(new FightReadyMpdArgParser());
			base.HandlerMgr.RegisterParser(new FightJoinArgParser());
			base.HandlerMgr.RegisterParser(new FightDesertArgParser());
			base.HandlerMgr.RegisterParser(new FightAnswerArgParser());
			base.HandlerMgr.RegisterParser(new HuntJoinArgParser());
			base.HandlerMgr.RegisterParser(new HuntReadyArgParser());
			base.HandlerMgr.RegisterParser(new UserLeaveInfoArgParser());
			base.HandlerMgr.RegisterParser(new DayRewardArgMpdParser());
			base.HandlerMgr.Register<LoginArg>(CtrlCmdId.user.login);
			base.HandlerMgr.Register<HeroStatsArg>(CtrlCmdId.user.conf);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.user.get_global_buffs);
			base.HandlerMgr.Register<HeroBuffsUpdateMpdArg>(CtrlCmdId.user.update_buffs_mpd);
			base.HandlerMgr.Register<GlobalBuffsUpdateMpdArg>(CtrlCmdId.user.update_global_buffs_mpd);
			base.HandlerMgr.Register<HeroGameInfoArg>(CtrlCmdId.user.game_info);
			base.HandlerMgr.Register<UserPersonalInfoArg>(CtrlCmdId.user.personal_details);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.user.all_info);
			base.HandlerMgr.Register<DressItemArg>(CtrlCmdId.user.dress);
			base.HandlerMgr.Register<NewDressedItemArg>(CtrlCmdId.user.dress_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.user.undress);
			base.HandlerMgr.Register<UndressedItemArg>(CtrlCmdId.user.undress_mpd);
			base.HandlerMgr.Register<DressedItemsArg>(CtrlCmdId.user.dressed_items);
			base.HandlerMgr.Register<HeroMoneyArg>(CtrlCmdId.user.money);
			base.HandlerMgr.Register<HeroMoneyMpdArg>(CtrlCmdId.user.money_mpd);
			base.HandlerMgr.Register<ServerDataArg>(CtrlCmdId.common.area_conf);
			base.HandlerMgr.Register<HeroDataArg>(CtrlCmdId.common.hero_conf);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.common.dummy);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.common.init);
			base.HandlerMgr.Register<CanReconnectArg>(CtrlCmdId.common.can_reconnect);
			base.HandlerMgr.Register<ReconnectArg>(CtrlCmdId.common.reconnect);
			base.HandlerMgr.Register<ActionUseArg>(CtrlCmdId.common.action);
			base.HandlerMgr.Register<HeroDataArg>(CtrlCmdId.hero.get_data);
			base.HandlerMgr.Register<HeroDataListArg>(CtrlCmdId.hero.get_data_list);
			base.HandlerMgr.Register<HeroDataListMpdArg>(CtrlCmdId.hero.get_data_list_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.hero.create);
			base.HandlerMgr.Register<MapListArg>(CtrlCmdId.arena.get_maps);
			base.HandlerMgr.Register<JoinArg>(CtrlCmdId.arena.join_request);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.arena.select_avatar);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.arena.ready);
			base.HandlerMgr.Register<DesertArg>(CtrlCmdId.arena.desert);
			base.HandlerMgr.Register<ShopContentArg>(CtrlCmdId.store.list);
			base.HandlerMgr.Register<UserBagArg>(CtrlCmdId.user.bag);
			base.HandlerMgr.Register<UpdateBagArg>(CtrlCmdId.user.update_bag_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.store.buy);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.store.sell);
			base.HandlerMgr.Register<ChatConfArg>(CtrlCmdId.chat.conf);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.chat.add);
			base.HandlerMgr.Register<ChatMsgArg>(CtrlCmdId.chat.message_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.chat.set_gag);
			base.HandlerMgr.Register<ChatGagArg>(CtrlCmdId.chat.set_gag_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.trade.start);
			base.HandlerMgr.Register<TradeStartArg>(CtrlCmdId.trade.start_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.trade.start_answer);
			base.HandlerMgr.Register<TradeStartAnsArg>(CtrlCmdId.trade.start_answer_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.trade.ready);
			base.HandlerMgr.Register<TradeReadyArg>(CtrlCmdId.trade.ready_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.trade.not_ready);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.trade.not_ready_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.trade.cancel);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.trade.cancel_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.trade.confirm);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.trade.confirm_mpd);
			base.HandlerMgr.Register<SelectAvatarArg>(CtrlCmdId.arena.select_avatar_mpd);
			base.HandlerMgr.Register<PlayerJoinedArg>(CtrlCmdId.arena.join_request_mpd);
			base.HandlerMgr.Register<PlayerDesertedArg>(CtrlCmdId.arena.desert_mpd);
			base.HandlerMgr.Register<PlayerReadyArg>(CtrlCmdId.arena.ready_mpd);
			base.HandlerMgr.Register<LaunchArg>(CtrlCmdId.arena.launch_mpd);
			base.HandlerMgr.Register<FightLogArg>(CtrlCmdId.fight.log);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.bank.change);
			base.HandlerMgr.Register<JoinInviteArg>(CtrlCmdId.arena.join_queue_invite_mpd);
			base.HandlerMgr.Register<JoinInviteArg>(CtrlCmdId.arena.join_invite_mpd);
			base.HandlerMgr.Register<GroupListArg>(CtrlCmdId.user.group_list);
			base.HandlerMgr.Register<JoinedToGroupArg>(CtrlCmdId.user.joined_to_group_mpd);
			base.HandlerMgr.Register<RemovedFromGroupArg>(CtrlCmdId.user.removed_from_group_mpd);
			base.HandlerMgr.Register<JoinFromGroupArg>(CtrlCmdId.user.join_from_group_request);
			base.HandlerMgr.Register<JoinFromGroupRequestArg>(CtrlCmdId.user.join_from_group_request_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.user.join_from_group_answer);
			base.HandlerMgr.Register<JoinFromGroupAnswerArg>(CtrlCmdId.user.join_from_group_answer_mpd);
			base.HandlerMgr.Register<JoinToGroupArg>(CtrlCmdId.user.join_to_group_request);
			base.HandlerMgr.Register<JoinToGroupRequestArg>(CtrlCmdId.user.join_to_group_request_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.user.join_to_group_answer);
			base.HandlerMgr.Register<JoinToGroupAnswerArg>(CtrlCmdId.user.join_to_group_answer_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.user.leave_group);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.user.change_leader);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.user.remove_from_group);
			base.HandlerMgr.Register<RemoveFromGroupArg>(CtrlCmdId.user.remove_from_group_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.user.group_deleted_mpd);
			base.HandlerMgr.Register<LeaderChangedArg>(CtrlCmdId.user.group_leader_changed_mpd);
			base.HandlerMgr.Register<CtrlOnlineArg>(CtrlCmdId.user.online_mpd);
			base.HandlerMgr.Register<CtrlOfflineArg>(CtrlCmdId.user.offline_mpd);
			base.HandlerMgr.Register<MapTypeDescsArg>(CtrlCmdId.arena.get_map_type_descs);
			base.HandlerMgr.Register<JoinQueueArg>(CtrlCmdId.arena.join_queue);
			base.HandlerMgr.Register<JoinQueueStateArg>(CtrlCmdId.arena.join_queue_mpd);
			base.HandlerMgr.Register<JoinQueueReadyArg>(CtrlCmdId.arena.join_queue_ready_mpd);
			base.HandlerMgr.Register<MapInfoArg>(CtrlCmdId.arena.get_maps_info);
			base.HandlerMgr.Register<JoinQueueArg>(CtrlCmdId.arena.join_random);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.user.drop_artifact);
			base.HandlerMgr.Register<HeroesInfoUpdateArg>(CtrlCmdId.user.update_info_mpd);
			base.HandlerMgr.Register<NpcListArg>(CtrlCmdId.npc.list);
			base.HandlerMgr.Register<QuestListArg>(CtrlCmdId.quest.update);
			base.HandlerMgr.Register<QuestUpdateArg>(CtrlCmdId.quest.update_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.quest.accept);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.quest.cancel);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.quest.done);
			base.HandlerMgr.Register<CreateArg>(CtrlCmdId.clan.create);
			base.HandlerMgr.Register<InfoMpdArg>(CtrlCmdId.clan.info_mpd);
			base.HandlerMgr.Register<InfoArg>(CtrlCmdId.clan.info);
			base.HandlerMgr.Register<RemoveUserArg>(CtrlCmdId.clan.remove_user);
			base.HandlerMgr.Register<RemoveUserMpdArg>(CtrlCmdId.clan.remove_user_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.clan.remove);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.clan.change_role);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.clan.invite);
			base.HandlerMgr.Register<InviteMpdArg>(CtrlCmdId.clan.invite_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.clan.invite_answer);
			base.HandlerMgr.Register<InviteAnswerMpdArg>(CtrlCmdId.clan.invite_answer_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.system.ping_mpd);
			base.HandlerMgr.Register<CastleListArg>(CtrlCmdId.castle.list);
			base.HandlerMgr.Register<CastleMembersArg>(CtrlCmdId.castle.info);
			base.HandlerMgr.Register<CastleHistoryArg>(CtrlCmdId.castle.history);
			base.HandlerMgr.Register<CastleBattleInfoArg>(CtrlCmdId.castle.battle_info);
			base.HandlerMgr.Register<CastleFighterListArg>(CtrlCmdId.castle.fighters);
			base.HandlerMgr.Register<DeferredMessageArg>(CtrlCmdId.common.message_mpd);
			base.HandlerMgr.Register<CastleStartBattleInfoArg>(CtrlCmdId.castle.start_battle_info_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.castle.set_fighters);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.castle.desert);
			base.HandlerMgr.Register<CastleStartRequestArg>(CtrlCmdId.castle.start_request_mpd);
			base.HandlerMgr.Register<CastleSelectAvatarTimerArg>(CtrlCmdId.castle.select_avatar_timer_mpd);
			base.HandlerMgr.Register<CastleSelectAvatarArg>(CtrlCmdId.castle.select_avatar_mpd);
			base.HandlerMgr.Register<CastleReadyArg>(CtrlCmdId.castle.ready_mpd);
			base.HandlerMgr.Register<CastleDesertArg>(CtrlCmdId.castle.desert_battle_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.castle.ready);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.castle.select_avatar);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.castle.desert_battle);
			base.HandlerMgr.Register<CastleLaunchArg>(CtrlCmdId.castle.launch_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.castle.early_won_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.common.reconnected);
			base.HandlerMgr.Register<FullHeroInfoArg>(CtrlCmdId.user.full_hero_info);
			base.HandlerMgr.Register<BwListArg>(CtrlCmdId.user.get_bw_list);
			base.HandlerMgr.Register<RemoveFromListArg>(CtrlCmdId.user.remove_from_list);
			base.HandlerMgr.Register<AddToListMpdArg>(CtrlCmdId.user.add_to_list_mpd);
			base.HandlerMgr.Register<FindResultArg>(CtrlCmdId.user.find);
			base.HandlerMgr.Register<FriendRequestMpdArg>(CtrlCmdId.user.friend_request_mpd);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.user.add_to_list);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.user.friend_answer);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.user.get_buffs);
			base.HandlerMgr.Register<RemoveFromListMpdArg>(CtrlCmdId.user.remove_from_list_mpd);
			base.HandlerMgr.Register<ObserverArg>(CtrlCmdId.user.get_observer_info);
			base.HandlerMgr.Register<FightInviteMpdArg>(CtrlCmdId.fight.invite_mpd);
			base.HandlerMgr.Register<FightStartSelectAvatarMpdArg>(CtrlCmdId.fight.start_select_avatar_mpd);
			base.HandlerMgr.Register<FightSelectAvatarMpdArg>(CtrlCmdId.fight.select_avatar_mpd);
			base.HandlerMgr.Register<FightDesertMpdArg>(CtrlCmdId.fight.desert_mpd);
			base.HandlerMgr.Register<FightLaunchArg>(CtrlCmdId.fight.launch_mpd);
			base.HandlerMgr.Register<FightInRequestMpd>(CtrlCmdId.fight.in_request_mpd);
			base.HandlerMgr.Register<FightReadyMpdArg>(CtrlCmdId.fight.ready_mpd);
			base.HandlerMgr.Register<FightDesertArg>(CtrlCmdId.fight.desert);
			base.HandlerMgr.Register<FightAnswerArg>(CtrlCmdId.fight.answer);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.fight.select_avatar);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.fight.in_request);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.fight.ready);
			base.HandlerMgr.Register<HuntJoinArg>(CtrlCmdId.hunt.join);
			base.HandlerMgr.Register<HuntReadyArg>(CtrlCmdId.hunt.ready);
			base.HandlerMgr.Register<FightJoinArg>(CtrlCmdId.fight.join);
			base.HandlerMgr.Register<UserLeaveInfoArg>(CtrlCmdId.user.leave_info);
			base.HandlerMgr.RegisterValidation(CtrlCmdId.common.user_log);
			base.HandlerMgr.Register<DayRewardMpdArg>(CtrlCmdId.common.day_reward_mpd);
		}

		public void Clean()
		{
			lock (mInFormatter)
			{
				mInFormatter.ClearRefTables();
			}
			lock (mOutFormatter)
			{
				mOutFormatter.ClearRefTables();
			}
			lock (mRequests)
			{
				mRequests.Clear();
				mLastRequest = null;
			}
			lock (mResponses)
			{
				mResponses.Clear();
			}
			lock (mSession)
			{
				mSession.Clean();
			}
			mConnectionErrorCallback = delegate
			{
			};
		}

		public void SetHost(string _host)
		{
			mHost = _host;
		}

		public void SetPort(int _port)
		{
			mPort = _port;
		}

		public void SetCookies(CookieCollection _cookies)
		{
		}

		public bool TryReadSession(string _fn)
		{
			Log.Debug("TryReadSession : " + _fn);
			if (string.IsNullOrEmpty(_fn))
			{
				throw new ArgumentNullException("_fn");
			}
			if (mSession == null)
			{
				return false;
			}
			if (_fn.Contains("{EMAIL}"))
			{
				_fn = _fn.Replace("{EMAIL}", mUserData.EMail);
			}
			if (_fn.Contains("{APP_DATA}"))
			{
				string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				folderPath += Path.DirectorySeparatorChar;
				_fn = _fn.Replace("{APP_DATA}", folderPath);
			}
			mSession.SetSavePath(_fn);
			mSession.TryRead();
			Log.Debug("mSession.IsValid : " + mSession.IsValid);
			return mSession.IsValid;
		}

		public void TryCleanSavedSession()
		{
			Log.Debug("TryCleanSavedSession");
			if (mSession != null)
			{
				mSession.CleanSaved();
			}
		}

		public void SetSessionKey(string _key)
		{
			Log.Debug("SetSessionKey : " + _key);
			lock (mSession)
			{
				if (mUserData.Inited)
				{
					mSession.Update("sess_uid", mUserData.UserId.ToString());
				}
				mSession.Update("sess_key", _key);
			}
		}

		public void EnablePing()
		{
			mPingEnabled = true;
		}

		public void DisablePing()
		{
			mPingEnabled = false;
		}

		public void SendOutcomings()
		{
			TrySendNextRequest();
			if (mPingEnabled)
			{
				TimeSpan timeSpan = DateTime.Now - mLastSendTime;
				if (timeSpan.TotalSeconds > (double)mPingTime)
				{
					SendPing();
				}
				if (timeSpan.TotalSeconds > (double)(mPingTime + mDisconnectTime))
				{
					mConnectionErrorCallback();
				}
			}
		}

		private void TrySendNextRequest()
		{
			if (mLastRequest != null)
			{
				return;
			}
			lock (mRequests)
			{
				if (mRequests.Count > 0)
				{
					mLastRequest = mRequests.Dequeue();
				}
			}
			if (mLastRequest != null)
			{
				if (mLastRequest.mData == null)
				{
					mLastRequest.PostContent = Pack(null);
				}
				else if (mLastRequest.mData is CtrlPacket)
				{
					CtrlPacket packet = (CtrlPacket)mLastRequest.mData;
					mLastRequest.PostContent = Pack(packet);
				}
				mLastSendTime = DateTime.Now;
				mLastRequest.Begin();
			}
		}

		private void EnqueueRequest(object _post)
		{
			WebTransferOperation webTransferOperation = new WebTransferOperation(FullAddress);
			webTransferOperation.mProxyCredential = mUserData.ProxyCredential;
			webTransferOperation.mCookieReceiver = this;
			webTransferOperation.mData = _post;
			Notifier<TransferOperation, object> notifier = new Notifier<TransferOperation, object>();
			notifier.mCallback = (Notifier<TransferOperation, object>.Callback)Delegate.Combine(notifier.mCallback, new Notifier<TransferOperation, object>.Callback(OnResponse));
			webTransferOperation.mNotifiers.Add(notifier);
			lock (mRequests)
			{
				mRequests.Enqueue(webTransferOperation);
			}
		}

		public void Send(Enum _cmd, params NamedVar[] _args)
		{
			MixedArray mixedArray = new MixedArray();
			mixedArray.Set(_args);
			CtrlPacket post = new CtrlPacket(_cmd, mixedArray);
			EnqueueRequest(post);
		}

		private void SendPing()
		{
			Log.Debug("Ping");
			EnqueueRequest(null);
		}

		private byte[] Pack(CtrlPacket _packet)
		{
			string text = mSession.GetValue("sess_uid");
			if (string.IsNullOrEmpty(text))
			{
				text = mUserData.UserId.ToString();
			}
			string value = mSession.GetValue("sess_key");
			mCounter++;
			Variable postContent;
			if (_packet != null)
			{
				postContent = _packet.SerializeWithSession(text, value, mCounter);
				Log.Info(() => string.Concat(_packet.Id, "\n", postContent.ToString()));
			}
			else
			{
				MixedArray mixedArray = new MixedArray();
				mixedArray["sess_uid"] = text;
				mixedArray["sess_key"] = value;
				mixedArray["counter"] = mCounter;
				postContent = mixedArray;
			}
			MemoryStream memoryStream = new MemoryStream();
			lock (mOutFormatter)
			{
				mOutFormatter.ClearRefTables();
				mOutFormatter.Serialize(postContent, memoryStream);
				mOutFormatter.ClearRefTables();
			}
			return memoryStream.ToArray();
		}

		protected override bool GetNextIncomingPacket(out CtrlPacket _packet, out Enum _id)
		{
			_packet = null;
			_id = null;
			lock (mResponses)
			{
				if (mResponses.Count == 0)
				{
					return false;
				}
				_packet = mResponses.Dequeue();
			}
			_id = _packet.Id;
			return true;
		}

		private void OnResponse(bool _success, TransferOperation _op, object _data)
		{
			Stream receiver = _op.Receiver;
			if (_success)
			{
				if (mParser.Begin(receiver))
				{
					lock (mInFormatter)
					{
						mParser.Parse(receiver, this, mInFormatter);
					}
				}
				else
				{
					_success = false;
					Log.Warning("failed to start parsing");
				}
			}
			else
			{
				Log.Warning(_op.URI + " failed");
			}
			mParser.End(_success, receiver);
			if (_op == mLastRequest)
			{
				mLastRequest = null;
			}
		}

		public void AddResponse(CtrlPacket _packet)
		{
			if (_packet == null)
			{
				throw new ArgumentNullException("_packet");
			}
			lock (mResponses)
			{
				mResponses.Enqueue(_packet);
			}
		}

		public void SubscribeConnectionError(ConnectionErrorCallback _callback)
		{
			mConnectionErrorCallback = (ConnectionErrorCallback)Delegate.Combine(mConnectionErrorCallback, _callback);
		}

		public void UnsubscribeConnectionError(ConnectionErrorCallback _callback)
		{
			mConnectionErrorCallback = (ConnectionErrorCallback)Delegate.Remove(mConnectionErrorCallback, _callback);
		}
	}
}
