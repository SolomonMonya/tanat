using System;
using System.Collections.Generic;
using System.IO;
using AMF;
using Log4Tanat;
using Network;

namespace TanatKernel
{
	public class BattlePacketManager : PacketManager<BattlePacket, BattleCmdId>
	{
		public delegate void SyncCallback(SyncData _data, float _time);

		public delegate void SetObjStateCallback(int _objId);

		public SyncCallback mSyncCallback;

		public SetObjStateCallback mVisibleCallback;

		public SetObjStateCallback mInvisibleCallback;

		public SetObjStateCallback mRelevantCallback;

		public SetObjStateCallback mUnrelevantCallback;

		private object mInSync = new object();

		private object mOutSync = new object();

		private Queue<BattlePacket> mIncomings = new Queue<BattlePacket>();

		private Queue<BattlePacket> mOutcomings = new Queue<BattlePacket>();

		private Formatter mInFormatter = new Formatter();

		private Formatter mOutFormatter = new Formatter();

		private MemoryStream mInputBuffer = new MemoryStream();

		private int mOffset;

		private int mNextPartSize;

		private PacketNumManager mNumMgr = new PacketNumManager();

		private TrackingIdManager mTrackingIdMgr = new TrackingIdManager();

		private int mIncomingTraffic;

		private int mOutcomingTraffic;

		private Queue<float> mPingData = new Queue<float>();

		private float mPing;

		private StatisticsCollector<BattleCmdId> mStatCollector = new StatisticsCollector<BattleCmdId>();

		public StatisticsCollector<BattleCmdId> StatisticsCollector => mStatCollector;

		public float Ping => mPing;

		public BattlePacketManager()
		{
			BattlePacketValidator validator = new BattlePacketValidator();
			base.HandlerMgr.RegisterValidator(BattleCmdId.CONNECT, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.READY, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.GET_TIME, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.SET_BEACON, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.MOVE_PLAYER, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.STOP_PLAYER, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.DO_ACTION, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.UPGRADE_SKILL, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.BUY, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.EQUIP_ITEM, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.SET_STATE, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.FORCE_RESPAWN, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.USE_OBJECT, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.GET_DROP_INFO, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.PICK_UP, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.DROP_ITEM, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.ENTER, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.ADD_BUFF, validator);
			base.HandlerMgr.RegisterValidator(BattleCmdId.START_BATTLE, validator);
			base.HandlerMgr.RegisterParser(new ConnectArgParser());
			base.HandlerMgr.RegisterParser(new ServerTimeArgParser());
			base.HandlerMgr.RegisterParser(new UpdateSkillArgParser());
			base.HandlerMgr.RegisterParser(new EquipItemArgParser());
			base.HandlerMgr.RegisterParser(new SetStateArgParser());
			base.HandlerMgr.RegisterParser(new UseObjectArgParser());
			base.HandlerMgr.RegisterParser(new DropInfoArgParser());
			base.HandlerMgr.RegisterParser(new DoActionArgParser());
			base.HandlerMgr.RegisterParser(new BoughtItemArgParsser());
			base.HandlerMgr.RegisterParser(new PickedUpArgParser());
			base.HandlerMgr.RegisterParser(new DropItemArgParser());
			base.HandlerMgr.RegisterParser(new GameDataArgParser());
			base.HandlerMgr.RegisterParser(new RegPlayerArgParser());
			base.HandlerMgr.RegisterParser(new UnregPlayerArgParser());
			base.HandlerMgr.RegisterParser(new SetMoneyArgParser());
			base.HandlerMgr.RegisterParser(new CreateObjectArgParser());
			base.HandlerMgr.RegisterParser(new DeleteObjectArgParser());
			base.HandlerMgr.RegisterParser(new SetAvatarArgParser());
			base.HandlerMgr.RegisterParser(new SyncPacketArgParser(mTrackingIdMgr));
			base.HandlerMgr.RegisterParser(new ActionArgParser());
			base.HandlerMgr.RegisterParser(new ActionDoneArgParser());
			base.HandlerMgr.RegisterParser(new LevelUpArgParser());
			base.HandlerMgr.RegisterParser(new KillArgParser());
			base.HandlerMgr.RegisterParser(new AddToInventoryArgParser());
			base.HandlerMgr.RegisterParser(new RemoveFromInventoryArgParser());
			base.HandlerMgr.RegisterParser(new EffectStartArgParser());
			base.HandlerMgr.RegisterParser(new EffectEndArgParser());
			base.HandlerMgr.RegisterParser(new ReceiveHitArgParser());
			base.HandlerMgr.RegisterParser(new SetProjectileArgParser());
			base.HandlerMgr.RegisterParser(new RespawnArgParser());
			base.HandlerMgr.RegisterParser(new ItemEquipedArgParser());
			base.HandlerMgr.RegisterParser(new ProrotypeInfoArgParser());
			base.HandlerMgr.RegisterParser(new BattleEndArgParser());
			base.HandlerMgr.RegisterParser(new RefreshDropInfoArgParser());
			base.HandlerMgr.RegisterParser(new AddAffectorArgParser());
			base.HandlerMgr.RegisterParser(new RemoveAffectorArgParser());
			base.HandlerMgr.RegisterParser(new NotifyBeaconArgParser());
			base.HandlerMgr.RegisterParser(new PlayerStatsArgParser());
			base.HandlerMgr.RegisterParser(new OnlineArgParser());
			base.HandlerMgr.RegisterParser(new QuestTaskArgParser());
			base.HandlerMgr.RegisterParser(new BuffArgParser());
			base.HandlerMgr.RegisterParser(new OrderDoneArgParser());
			base.HandlerMgr.Register<ConnectArg>(BattleCmdId.CONNECT);
			base.HandlerMgr.RegisterValidation(BattleCmdId.READY);
			base.HandlerMgr.Register<ServerTimeArg>(BattleCmdId.GET_TIME);
			base.HandlerMgr.RegisterValidation(BattleCmdId.SET_BEACON);
			base.HandlerMgr.RegisterValidation(BattleCmdId.MOVE_PLAYER);
			base.HandlerMgr.RegisterValidation(BattleCmdId.STOP_PLAYER);
			base.HandlerMgr.Register<DoActionArg>(BattleCmdId.DO_ACTION);
			base.HandlerMgr.Register<UpdateSkillArg>(BattleCmdId.UPGRADE_SKILL);
			base.HandlerMgr.Register<BoughtItemArg>(BattleCmdId.BUY);
			base.HandlerMgr.Register<EquipItemArg>(BattleCmdId.EQUIP_ITEM);
			base.HandlerMgr.Register<SetStateArg>(BattleCmdId.SET_STATE);
			base.HandlerMgr.RegisterValidation(BattleCmdId.FORCE_RESPAWN);
			base.HandlerMgr.Register<UseObjectArg>(BattleCmdId.USE_OBJECT);
			base.HandlerMgr.RegisterValidation(BattleCmdId.CAMERA_ATTACH);
			base.HandlerMgr.RegisterValidation(BattleCmdId.CAMERA_MOVE);
			base.HandlerMgr.RegisterValidation(BattleCmdId.CAMERA_ZOOM);
			base.HandlerMgr.Register<DropInfoArg>(BattleCmdId.GET_DROP_INFO);
			base.HandlerMgr.Register<PickedUpArg>(BattleCmdId.PICK_UP);
			base.HandlerMgr.Register<DropItemArg>(BattleCmdId.DROP_ITEM);
			base.HandlerMgr.RegisterValidation(BattleCmdId.ENTER);
			base.HandlerMgr.Register<RegPlayerArg>(BattleCmdId.PLAYER_REG);
			base.HandlerMgr.Register<UnregPlayerArg>(BattleCmdId.PLAYER_UNREG);
			base.HandlerMgr.Register<GameDataArg>(BattleCmdId.GAME_DATA);
			base.HandlerMgr.Register<SetAvatarArg>(BattleCmdId.SET_AVATAR);
			base.HandlerMgr.Register<CreateObjectArg>(BattleCmdId.CREATE_OBJECT);
			base.HandlerMgr.Register<DeleteObjectArg>(BattleCmdId.DELETE_OBJECT);
			base.HandlerMgr.Register<SetMoneyArg>(BattleCmdId.SET_MONEY);
			base.HandlerMgr.Register<SyncPacket>(BattleCmdId.SYNC);
			base.HandlerMgr.Register<ActionArg>(BattleCmdId.ACTION);
			base.HandlerMgr.Register<ActionDoneArg>(BattleCmdId.ACTION_DONE);
			base.HandlerMgr.Register<LevelUpArg>(BattleCmdId.LEVEL_UP);
			base.HandlerMgr.Register<KillArg>(BattleCmdId.ON_KILL);
			base.HandlerMgr.Register<BattleEndArg>(BattleCmdId.BATTLE_END);
			base.HandlerMgr.Register<AddToInventoryArg>(BattleCmdId.ADD_TO_INVENTORY);
			base.HandlerMgr.Register<RemoveFromInventoryArg>(BattleCmdId.REM_FROM_INVENTORY);
			base.HandlerMgr.Register<EffectStartArg>(BattleCmdId.EFFECT_START);
			base.HandlerMgr.Register<EffectEndArg>(BattleCmdId.EFFECT_END);
			base.HandlerMgr.Register<ReceiveHitArg>(BattleCmdId.RECEIVE_HIT);
			base.HandlerMgr.Register<SetProjectileArg>(BattleCmdId.SET_PROJECTILE);
			base.HandlerMgr.Register<RespawnArg>(BattleCmdId.RESPAWN);
			base.HandlerMgr.Register<ItemEquipedArg>(BattleCmdId.ITEM_EQUIP);
			base.HandlerMgr.Register<ProrotypeInfoArg>(BattleCmdId.PROTOTYPE_INFO);
			base.HandlerMgr.Register<RefreshDropInfoArg>(BattleCmdId.REFRESH_DROP_CONTENT);
			base.HandlerMgr.Register<AddEffectorArg>(BattleCmdId.ADD_EFFECTOR);
			base.HandlerMgr.Register<RemoveAffectorArg>(BattleCmdId.REMOVE_EFFECTOR);
			base.HandlerMgr.Register<NotifyBeaconArg>(BattleCmdId.NOTIFY_BEACON);
			base.HandlerMgr.Register<PlayerStatsArg>(BattleCmdId.PLAYER_STATS);
			base.HandlerMgr.Register<OnlineArg>(BattleCmdId.PLAYER_ONLINE);
			base.HandlerMgr.Register<QuestTaskArg>(BattleCmdId.QUEST_TASK);
			base.HandlerMgr.Register<BuffArg>(BattleCmdId.ADD_BUFF);
			base.HandlerMgr.Register<OrderDoneArg>(BattleCmdId.ORDER_DONE);
			base.HandlerMgr.RegisterValidation(BattleCmdId.START_BATTLE);
			base.HandlerMgr.Subscribe<SyncPacket>(BattleCmdId.SYNC, null, null, DistributeSync);
		}

		public void Clear()
		{
			lock (mInSync)
			{
				Log.Debug(mIncomings.Count + " incoming packets");
				mIncomings.Clear();
				mInFormatter.ClearRefTables();
				Log.Debug("input buffer length: " + mInputBuffer.Length);
				Log.Debug("input buffer capacity: " + mInputBuffer.Capacity);
				mNextPartSize = 0;
				mOffset = 0;
				mInputBuffer.SetLength(0L);
			}
			lock (mOutSync)
			{
				Log.Debug(mOutcomings.Count + " outcoming packets");
				mOutcomings.Clear();
				mOutFormatter.ClearRefTables();
				mStatCollector.Clear();
			}
			lock (mPingData)
			{
				mPingData.Clear();
				mPing = 0f;
			}
			mNumMgr.Clear();
			mTrackingIdMgr.Clear();
		}

		protected override bool GetNextIncomingPacket(out BattlePacket _packet, out BattleCmdId _id)
		{
			_packet = null;
			_id = BattleCmdId.ECHO;
			lock (mIncomings)
			{
				if (mIncomings.Count == 0)
				{
					return false;
				}
				_packet = mIncomings.Dequeue();
			}
			_id = _packet.Id;
			return true;
		}

		public void Parse(byte[] _buffer, int _offset, int _size)
		{
			lock (mInSync)
			{
				mInputBuffer.Position = mInputBuffer.Length;
				mInputBuffer.Write(_buffer, _offset, _size);
				mIncomingTraffic += _size;
				if (mInputBuffer.Length < 4)
				{
					return;
				}
				do
				{
					if (mNextPartSize <= 0)
					{
						mInputBuffer.Position = mOffset;
						if (mInputBuffer.Length - mInputBuffer.Position < 4)
						{
							break;
						}
						mNextPartSize = ReadSize();
						mOffset += 4;
						if (mNextPartSize <= 0)
						{
							Log.Warning("invalid chunk size: " + mNextPartSize);
							continue;
						}
					}
					if (mInputBuffer.Length - mOffset < mNextPartSize)
					{
						break;
					}
					mInputBuffer.Position = mOffset;
					mOffset += mNextPartSize;
					mNextPartSize = 0;
					while (mInputBuffer.Position != mOffset)
					{
						int num = ReadSize();
						if (num <= 0 || num > mOffset - mInputBuffer.Position)
						{
							Log.Warning("invalid packet size: " + num);
							break;
						}
						long position = mInputBuffer.Position;
						Variable content = mInFormatter.Deserialize(mInputBuffer);
						try
						{
							BattlePacket battlePacket = new BattlePacket(content, mNumMgr);
							if (battlePacket.HasRequest())
							{
								battlePacket.RegisterReceivingTime();
								float ping = battlePacket.GetPing();
								lock (mPingData)
								{
									mPingData.Enqueue(ping);
								}
							}
							lock (mIncomings)
							{
								mIncomings.Enqueue(battlePacket);
							}
						}
						catch (NetSystemException)
						{
						}
						if (mInputBuffer.Position > mOffset || mInputBuffer.Position <= position)
						{
							Log.Warning("wrong buffer position");
							break;
						}
					}
				}
				while (mInputBuffer.Length - mInputBuffer.Position > 0);
				if (mOffset == mInputBuffer.Length)
				{
					mInputBuffer.SetLength(0L);
					mOffset = 0;
				}
			}
		}

		private int ReadSize()
		{
			byte[] array = new byte[4];
			mInputBuffer.Read(array, 0, array.Length);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(array);
			}
			return BitConverter.ToInt32(array, 0);
		}

		public void SendOutcomings(NetSystem _netSys, int _connectionId)
		{
			lock (mOutSync)
			{
				if (mOutcomings.Count == 0)
				{
					return;
				}
			}
			MemoryStream memoryStream = new MemoryStream();
			long num = 0L;
			byte[] array = new byte[4];
			memoryStream.Write(array, 0, array.Length);
			Queue<NetSystem.ITimeRegisterer> queue = new Queue<NetSystem.ITimeRegisterer>();
			lock (mOutSync)
			{
				foreach (BattlePacket mOutcoming in mOutcomings)
				{
					memoryStream.Write(array, 0, array.Length);
					long num2 = mOutFormatter.Serialize(mOutcoming.Serialize(), memoryStream);
					queue.Enqueue(mOutcoming);
					array = BitConverter.GetBytes((int)num2);
					if (BitConverter.IsLittleEndian)
					{
						Array.Reverse(array);
					}
					memoryStream.Position -= num2 + 4;
					memoryStream.Write(array, 0, array.Length);
					memoryStream.Position = memoryStream.Length;
					num += num2 + 4;
				}
				mOutcomings.Clear();
			}
			array = BitConverter.GetBytes((int)num);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(array);
			}
			memoryStream.Position = 0L;
			memoryStream.Write(array, 0, array.Length);
			byte[] array2 = memoryStream.ToArray();
			mOutcomingTraffic += array2.Length;
			_netSys.Send(_connectionId, array2, queue);
		}

		public void Send(BattleCmdId _cmd, params NamedVar[] _args)
		{
			MixedArray args = new MixedArray();
			args.Set(_args);
			BattlePacket item = new BattlePacket(_cmd, args, mNumMgr);
			lock (mOutSync)
			{
				mOutcomings.Enqueue(item);
			}
			Log.Info(() => string.Concat("enqueued ", _cmd, " with args:\n", args.ToString()));
		}

		public void UpdatePing()
		{
			lock (mPingData)
			{
				if (mPingData.Count == 0)
				{
					return;
				}
				float num = 0f;
				foreach (float mPingDatum in mPingData)
				{
					float num2 = mPingDatum;
					num += num2;
				}
				mPing = num / (float)mPingData.Count;
				mPingData.Clear();
			}
		}

		private void DistributeSync(SyncPacket _arg)
		{
			if (mInvisibleCallback != null)
			{
				foreach (int newInvisibleId in _arg.NewInvisibleIds)
				{
					mInvisibleCallback(newInvisibleId);
				}
			}
			if (mVisibleCallback != null)
			{
				foreach (int newVisibleId in _arg.NewVisibleIds)
				{
					mVisibleCallback(newVisibleId);
				}
			}
			if (mUnrelevantCallback != null)
			{
				foreach (int newUnrelevantId in _arg.NewUnrelevantIds)
				{
					mUnrelevantCallback(newUnrelevantId);
				}
			}
			if (mRelevantCallback != null)
			{
				foreach (int newRelevantId in _arg.NewRelevantIds)
				{
					mRelevantCallback(newRelevantId);
				}
			}
			if (mSyncCallback == null)
			{
				return;
			}
			foreach (SyncData datum in _arg.Data)
			{
				mSyncCallback(datum, _arg.Time);
			}
		}
	}
}
