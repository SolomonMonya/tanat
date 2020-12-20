using System;
using System.Reflection;

namespace TanatKernel
{
	public class CtrlCmdId
	{
		public enum user
		{
			login,
			conf,
			all_info,
			game_info,
			personal_details,
			save_personal_details,
			money,
			money_mpd,
			bag,
			update_bag_mpd,
			drop_artifact,
			get_global_buffs,
			get_buffs,
			update_buffs_mpd,
			update_global_buffs_mpd,
			dress,
			dress_mpd,
			undress,
			undress_mpd,
			dressed_items,
			group_list,
			joined_to_group_mpd,
			removed_from_group_mpd,
			join_from_group_request,
			join_from_group_request_mpd,
			join_from_group_answer,
			join_from_group_answer_mpd,
			join_to_group_request,
			join_to_group_request_mpd,
			join_to_group_answer,
			join_to_group_answer_mpd,
			recommend_to_group_request,
			recommend_to_group_request_mpd,
			recommend_to_group_decline,
			recommend_to_group_decline_mpd,
			leave_group,
			change_leader,
			remove_from_group,
			remove_from_group_mpd,
			group_deleted_mpd,
			group_leader_changed_mpd,
			online_mpd,
			offline_mpd,
			update_info_mpd,
			get_bw_list,
			add_to_list,
			remove_from_list,
			remove_from_list_mpd,
			add_to_list_mpd,
			find,
			friend_request_mpd,
			friend_answer,
			full_hero_info,
			get_observer_info,
			leave_info
		}

		public enum common
		{
			area_conf,
			hero_conf,
			dummy,
			init,
			can_reconnect,
			reconnect,
			reconnected,
			action,
			message_mpd,
			user_log,
			day_reward_mpd
		}

		public enum hero
		{
			get_data,
			get_data_list,
			get_data_list_mpd,
			create
		}

		public enum arena
		{
			join_queue,
			join_queue_mpd,
			join_queue_ready_mpd,
			get_maps,
			join_request,
			join_request_mpd,
			select_avatar,
			select_avatar_mpd,
			ready,
			ready_mpd,
			desert,
			desert_mpd,
			launch_mpd,
			join_queue_invite_mpd,
			join_invite_mpd,
			get_map_type_descs,
			join_random,
			get_maps_info
		}

		public enum fight
		{
			log,
			join,
			desert,
			invite_mpd,
			answer,
			start_select_avatar_mpd,
			select_avatar,
			select_avatar_mpd,
			desert_mpd,
			launch_mpd,
			in_request,
			in_request_mpd,
			ready,
			ready_mpd
		}

		public enum hunt
		{
			join,
			ready
		}

		public enum store
		{
			list,
			buy,
			sell
		}

		public enum chat
		{
			conf,
			add,
			message_mpd,
			set_gag,
			set_gag_mpd
		}

		public enum trade
		{
			start,
			start_mpd,
			start_answer,
			start_answer_mpd,
			ready,
			ready_mpd,
			not_ready,
			not_ready_mpd,
			cancel,
			cancel_mpd,
			confirm,
			confirm_mpd
		}

		public enum bank
		{
			change
		}

		public enum npc
		{
			list
		}

		public enum quest
		{
			update,
			update_mpd,
			accept,
			cancel,
			done
		}

		public enum log
		{
			insert
		}

		public enum clan
		{
			create,
			info,
			info_mpd,
			remove_user,
			remove_user_mpd,
			remove,
			change_role,
			invite,
			invite_mpd,
			invite_answer,
			invite_answer_mpd
		}

		public enum system
		{
			ping_mpd
		}

		public enum castle
		{
			list,
			info,
			history,
			battle_info,
			fighters,
			set_fighters,
			desert,
			start_battle_info_mpd,
			launch_mpd,
			early_won_mpd,
			start_request_mpd,
			select_avatar_timer_mpd,
			select_avatar,
			ready,
			ready_mpd,
			select_avatar_mpd,
			desert_battle,
			desert_battle_mpd
		}

		public struct CallPath
		{
			public static readonly char mSeparator = '|';

			private string mObj;

			private string mAct;

			public string Obj => mObj;

			public string Act => mAct;

			public CallPath(string _obj, string _act)
			{
				if (string.IsNullOrEmpty(_obj) || string.IsNullOrEmpty(_act))
				{
					throw new ArgumentException();
				}
				mObj = _obj;
				mAct = _act;
			}

			public CallPath(string _proc)
			{
				if (string.IsNullOrEmpty(_proc))
				{
					throw new ArgumentException();
				}
				string[] array = _proc.Split(mSeparator);
				if (array.Length != 2)
				{
					throw new ArgumentException();
				}
				mObj = array[0];
				mAct = array[1];
			}

			public override string ToString()
			{
				return mObj + mSeparator + mAct;
			}
		}

		public static CallPath ToCallPath(Enum _proc)
		{
			return new CallPath(_proc.GetType().Name, _proc.ToString());
		}

		public static Enum Parse(string _proc)
		{
			try
			{
				CallPath callPath = new CallPath(_proc);
				Type declaringType = MethodBase.GetCurrentMethod().DeclaringType;
				Type nestedType = declaringType.GetNestedType(callPath.Obj);
				if (nestedType != null)
				{
					return Enum.Parse(nestedType, callPath.Act) as Enum;
				}
			}
			catch (ArgumentException)
			{
			}
			return null;
		}
	}
}
