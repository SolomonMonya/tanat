namespace TanatKernel
{
	public interface ICustomer
	{
		void Buy(int _shopId, int _sellerId, int _itemId, int _cnt);
	}
}
