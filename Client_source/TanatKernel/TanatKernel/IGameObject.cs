namespace TanatKernel
{
	public interface IGameObject : IStorable
	{
		InstanceData Data
		{
			get;
		}

		BattlePrototype Proto
		{
			get;
		}
	}
}
