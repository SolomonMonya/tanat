using System;
using System.Collections.Generic;
using AMF;
using Log4Tanat;

namespace TanatKernel
{
	public class HeroSender
	{
		private CtrlEntryPoint mEntryPoint;

		public HeroSender(CtrlEntryPoint _entryPoint)
		{
			if (_entryPoint == null)
			{
				throw new ArgumentNullException("_entryPoint");
			}
			mEntryPoint = _entryPoint;
		}

		public void Create(int _race, bool _gender, int _face, int _hair, int _distMark, int _skinColor, int _hairColor)
		{
			mEntryPoint.Send(CtrlCmdId.hero.create, new NamedVar("race", _race), new NamedVar("gender", _gender ? 1 : 0), new NamedVar("face", _face), new NamedVar("hair", _hair), new NamedVar("dist_mark", _distMark), new NamedVar("skin_color", _skinColor), new NamedVar("hair_color", _hairColor));
		}

		public void GameHeroesRequest(List<int> _heroIds)
		{
			MixedArray mixedArray = new MixedArray();
			foreach (int _heroId in _heroIds)
			{
				mixedArray.Add(_heroId);
			}
			mEntryPoint.Send(CtrlCmdId.hero.get_data_list, new NamedVar("id", mixedArray));
		}

		public void GameInfoRequest(int _heroId)
		{
			mEntryPoint.Send(CtrlCmdId.user.game_info, new NamedVar("user_id", _heroId));
		}

		public void DataRequest(int _heroId)
		{
			mEntryPoint.Send(CtrlCmdId.hero.get_data, new NamedVar("id", _heroId));
		}

		public void DressedItemsRequest(int _heroId)
		{
		}

		public void PersonalDetailsRequest(int _heroId)
		{
			mEntryPoint.Send(CtrlCmdId.user.personal_details, new NamedVar("user_id", _heroId));
		}

		public void DoAction(Thing _thing)
		{
			DoAction(_thing, _check: false);
		}

		public void DoAction(Thing _thing, bool _check)
		{
			if (_thing == null)
			{
				throw new ArgumentNullException("_thing");
			}
			CtrlPrototype.Action mAction = _thing.CtrlProto.Article.mAction;
			if (mAction == null)
			{
				Log.Warning("thing " + _thing.Id + " doesn't contain action");
			}
			else if (!_check)
			{
				mEntryPoint.Send(CtrlCmdId.common.action, new NamedVar("action_id", mAction.Id), new NamedVar("artifact_id", _thing.Id));
			}
			else
			{
				mEntryPoint.Send(CtrlCmdId.common.action, new NamedVar("action_id", mAction.Id), new NamedVar("artifact_id", _thing.Id), new NamedVar("check", _check));
			}
		}

		public void GetBag()
		{
			mEntryPoint.Send(CtrlCmdId.user.bag);
		}

		public void Dress(int _itemId, int _slot)
		{
			mEntryPoint.Send(CtrlCmdId.user.dress, new NamedVar("item", _itemId), new NamedVar("slot", _slot));
		}

		public void Undress(int _slot)
		{
			mEntryPoint.Send(CtrlCmdId.user.undress, new NamedVar("slot", _slot));
		}

		public void DropItems(IDictionary<int, int> _items)
		{
			if (_items == null)
			{
				throw new ArgumentNullException("_items");
			}
			MixedArray mixedArray = new MixedArray();
			foreach (KeyValuePair<int, int> _item in _items)
			{
				mixedArray[_item.Key.ToString()] = _item.Value;
			}
			mEntryPoint.Send(CtrlCmdId.user.drop_artifact, new NamedVar("artifacts", mixedArray));
		}

		public void DropItem(int _itemId, int _count)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			dictionary.Add(_itemId, _count);
			DropItems(dictionary);
		}
	}
}
