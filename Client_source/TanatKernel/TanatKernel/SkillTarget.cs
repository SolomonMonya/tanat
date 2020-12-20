namespace TanatKernel
{
	public enum SkillTarget
	{
		NONE = 0,
		FRIEND = 1,
		NOT_FRIEND = 2,
		ENEMY = 4,
		NOT_ENEMY = 8,
		BUILDING = 0x10,
		NOT_BUILDING = 0x20,
		OBJECT = 0x40,
		NOT_OBJECT = 0x80,
		POINT = 0x100,
		SELF = 0x200,
		NOT_SELF = 0x400
	}
}
