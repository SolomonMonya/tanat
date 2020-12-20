using UnityEngine;

[AddComponentMenu("Death Behaviour/BulletDeath")]
public class BulletDeath : DeathBehaviour
{
	private Component[] mParticleEmitters = new Component[0];

	private Component[] mMeshRenderers = new Component[0];

	private Component[] mTrailRenderers = new Component[0];

	private bool mDieStarted;

	private void Update()
	{
		if (!mDieStarted)
		{
			return;
		}
		bool flag = true;
		Component[] array = mParticleEmitters;
		foreach (Component component in array)
		{
			if ((bool)(component as ParticleEmitter) && ((ParticleEmitter)component).particleCount != 0)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			Done();
		}
	}

	protected override void StartDie()
	{
		if (mDieStarted)
		{
			return;
		}
		mMeshRenderers = GetComponentsInChildren(typeof(MeshRenderer));
		mTrailRenderers = GetComponentsInChildren(typeof(TrailRenderer));
		mParticleEmitters = GetComponentsInChildren(typeof(ParticleEmitter));
		mDieStarted = true;
		Component[] array = mParticleEmitters;
		foreach (Component component in array)
		{
			if ((bool)(component as ParticleEmitter))
			{
				((ParticleEmitter)component).emit = false;
			}
		}
		Component[] array2 = mMeshRenderers;
		foreach (Component component2 in array2)
		{
			if ((bool)(component2 as MeshRenderer))
			{
				((MeshRenderer)component2).enabled = false;
			}
		}
		Component[] array3 = mTrailRenderers;
		foreach (Component component3 in array3)
		{
			if ((bool)(component3 as TrailRenderer))
			{
				((TrailRenderer)component3).enabled = false;
			}
		}
		Projectile projectile = base.gameObject.GetComponent(typeof(Projectile)) as Projectile;
		if (projectile != null)
		{
			projectile.enabled = false;
		}
	}

	public override void Reborn()
	{
		base.Reborn();
		mDieStarted = false;
		Component[] array = mParticleEmitters;
		foreach (Component component in array)
		{
			if ((bool)(component as ParticleEmitter))
			{
				((ParticleEmitter)component).emit = true;
			}
		}
		Component[] array2 = mMeshRenderers;
		foreach (Component component2 in array2)
		{
			if ((bool)(component2 as MeshRenderer))
			{
				((MeshRenderer)component2).enabled = true;
			}
		}
		Component[] array3 = mTrailRenderers;
		foreach (Component component3 in array3)
		{
			if ((bool)(component3 as TrailRenderer))
			{
				((TrailRenderer)component3).enabled = true;
			}
		}
	}

	public override bool IsForceStart()
	{
		return true;
	}
}
