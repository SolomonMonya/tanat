using TanatKernel;

public interface IItemUsageMgr
{
	void UseItem(int _itemId);

	double GetCooldownProgress(int _itemId);

	double GetCooldownProgress(CtrlPrototype _item);
}
