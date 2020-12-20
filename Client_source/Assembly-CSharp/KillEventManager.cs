using System;
using System.Collections.Generic;
using TanatKernel;

public class KillEventManager
{
	private bool mWaitFirstBlood;

	private string mFirstBloodEvent = "First blood";

	private bool mWaitFirstCreep = true;

	private string mFirstCreepEvent = "music_creep_moving_ start";

	private Dictionary<int, string> mKillsNumEvents = new Dictionary<int, string>();

	private static int mKillTextsCount = 3;

	private List<string> mKillTexts = new List<string>();

	private Random mRand = new Random();

	private SelfPlayer mSelfPlayer;

	private int mFriendTeamKills;

	private int mEnemyTeamKills;

	private Chat mChat;

	public KillEventManager(Chat _chat, SelfPlayer _selfPlayer)
	{
		mChat = _chat;
		mSelfPlayer = _selfPlayer;
		mKillsNumEvents[3] = "Killing Spree";
		mKillsNumEvents[4] = "Dominating";
		mKillsNumEvents[5] = "Mega kill";
		mKillsNumEvents[6] = "Unstoppable";
		mKillsNumEvents[7] = "Wicked sick";
		mKillsNumEvents[8] = "Monster kill";
		mKillsNumEvents[9] = "GODLIKE";
		mKillsNumEvents[10] = "Beyond GODLIKE";
		for (int i = 0; i < mKillTextsCount; i++)
		{
			string localeText = GuiSystem.GetLocaleText("ON_KILL_TEXT_" + i);
			mKillTexts.Add(localeText);
		}
	}

	public void SetFirstBlood(bool _flag)
	{
		mWaitFirstBlood = _flag;
	}

	public void RegNewKill(Player _killer, Player _victim, float _time)
	{
		if (_victim == null)
		{
			return;
		}
		_victim.mKillsWithoutDeath = 0;
		_victim.mKillTime.Clear();
		if (_killer == null)
		{
			return;
		}
		_killer.mKillsWithoutDeath++;
		_killer.mKillTime.Enqueue(_time);
		if (mWaitFirstBlood)
		{
			mWaitFirstBlood = false;
			SoundSystem.Instance.PlaySoundEvent(mFirstBloodEvent);
		}
		if (mKillsNumEvents.TryGetValue(_killer.mKillsWithoutDeath, out var value))
		{
			SoundSystem.Instance.PlaySoundEvent(value);
		}
		string txt = GenerateRandomKillText(_killer.Name, _victim.Name);
		mChat.AddSystemMessage(txt);
		Player player = mSelfPlayer.Player;
		if (player != null)
		{
			if (_killer.Team == player.Team)
			{
				mFriendTeamKills++;
			}
			else if (_victim.Team == player.Team)
			{
				mEnemyTeamKills++;
			}
		}
	}

	private string GenerateRandomKillText(string _killerName, string _victimName)
	{
		if (mKillTexts.Count == 0)
		{
			return string.Empty;
		}
		int index = mRand.Next(mKillTexts.Count);
		string text = mKillTexts[index];
		text = text.Replace("{KILLER}", _killerName);
		return text.Replace("{VICTIM}", _victimName);
	}

	public void CheckFirstCreep(GameData _gd)
	{
		if (mWaitFirstCreep && _gd.Proto.Destructible != null && _gd.Proto.Avatar == null && _gd.Proto.Building == null && _gd.Proto.Shop == null)
		{
			SoundSystem.Instance.PlaySoundEvent(mFirstCreepEvent);
			mWaitFirstCreep = false;
		}
	}

	public int GetFriendTeamKills()
	{
		return mFriendTeamKills;
	}

	public int GetEnemyTeamKills()
	{
		return mEnemyTeamKills;
	}
}
