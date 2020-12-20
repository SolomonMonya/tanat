using System;
using TanatKernel;

public class TestGameObj : IGameObject, IStorable
{
	private BattlePrototype mProto;

	private InstanceData mData;

	public int Id => mData.Id;

	public BattlePrototype Proto => mProto;

	public InstanceData Data => mData;

	public TestGameObj(BattlePrototype _proto, InstanceData _data)
	{
		if (_proto == null)
		{
			throw new ArgumentNullException("_proto");
		}
		if (_data == null)
		{
			throw new ArgumentNullException("_data");
		}
		mProto = _proto;
		mData = _data;
	}
}
