using TanatKernel;

public class PortalSelector : Selector
{
	public Location mLocation;

	public override int CurrentValue()
	{
		return (int)mLocation;
	}
}
