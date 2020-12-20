using UnityEngine;

[AddComponentMenu("Death Behaviour/PreventDisable")]
public class PreventDisable : DeathBehaviour
{
	public bool mEnabled;

	private Renderer[] mRenderers;

	protected override void StartDie()
	{
		if (!mEnabled || !IsDisableMode())
		{
			Done();
			return;
		}
		mRenderers = base.gameObject.GetComponentsInChildren<Renderer>();
		int num = LayerMask.NameToLayer("Fog");
		Renderer[] array = mRenderers;
		foreach (Renderer renderer in array)
		{
			if (renderer.gameObject.layer != num)
			{
				renderer.enabled = false;
			}
		}
		mEnabled = false;
	}

	public override void Reborn()
	{
		base.Reborn();
		if (mRenderers == null)
		{
			return;
		}
		int num = LayerMask.NameToLayer("Fog");
		Renderer[] array = mRenderers;
		foreach (Renderer renderer in array)
		{
			if (!(null == renderer) && !(null == renderer.gameObject) && renderer.gameObject.layer != num)
			{
				renderer.enabled = true;
			}
		}
		mRenderers = null;
		NetSyncTransform component = base.gameObject.GetComponent<NetSyncTransform>();
		if (null != component)
		{
			component.ResetSyncData(_visible: false);
		}
	}

	public override bool IsForceStart()
	{
		return mEnabled;
	}
}
