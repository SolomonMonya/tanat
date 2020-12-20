using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class QuestMark : MonoBehaviour
{
	public GameObject mExclamationMark;

	public GameObject mQuestionMark;

	public int mNpcId;

	public void SetData(Dictionary<int, QuestStatus> _statuses)
	{
		mQuestionMark.SetActiveRecursively(state: false);
		mExclamationMark.SetActiveRecursively(state: false);
		if (_statuses.ContainsKey(mNpcId))
		{
			if (_statuses[mNpcId] == QuestStatus.EXIST || _statuses[mNpcId] == QuestStatus.WAIT_COOLDOWN)
			{
				mExclamationMark.SetActiveRecursively(state: true);
			}
			else if (_statuses[mNpcId] == QuestStatus.DONE)
			{
				mQuestionMark.SetActiveRecursively(state: true);
			}
		}
	}
}
