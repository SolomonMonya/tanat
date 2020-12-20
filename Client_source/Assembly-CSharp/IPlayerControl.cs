using TanatKernel;
using UnityEngine;

public interface IPlayerControl
{
	SelfPlayer SelfPlayer
	{
		get;
	}

	int SelectedObjId
	{
		get;
	}

	int MarkedObjectId
	{
		get;
	}

	bool IsValid();

	void PlaySound(SoundEmiter.SoundType _type);

	bool SetSelection(int _id, bool _send);

	void Mark(GameObject _obj);

	void UnmarkCurrent();

	bool RemoveActiveAbility();
}
