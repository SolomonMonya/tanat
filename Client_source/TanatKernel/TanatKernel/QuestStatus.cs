namespace TanatKernel
{
	public enum QuestStatus
	{
		NONE = -1,
		WAIT_COOLDOWN = 0,
		IN_PROGRESS = 1,
		DONE = 2,
		CLOSED = 3,
		EXIST = 0x400,
		FAKE = 0x800
	}
}
