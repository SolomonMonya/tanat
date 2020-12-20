using UnityEngine;

[AddComponentMenu("Death Behaviour/GunDeath")]
public class GunDeath : DeathBehaviour
{
	public string mDeathEffectId = string.Empty;

	private GameCamera mCamera;

	private Explosion mExplosion;

	private bool mDieStart;

	protected override void StartDie()
	{
		if (mDieStart)
		{
			return;
		}
		mDieStart = true;
		mCamera = Camera.main.GetComponent(typeof(GameCamera)) as GameCamera;
		if ((bool)mCamera)
		{
			mCamera.MakeShakeEffect(base.gameObject.transform.position);
		}
		if (mDeathEffectId != string.Empty && (bool)VisualEffectsMgr.Instance)
		{
			VisualEffectsMgr.Instance.PlayEffect(mDeathEffectId, base.gameObject.transform.position);
		}
		mExplosion = base.gameObject.GetComponent(typeof(Explosion)) as Explosion;
		if ((bool)mExplosion)
		{
			mExplosion.Explode();
		}
		Component[] componentsInChildren = base.gameObject.GetComponentsInChildren(typeof(ParticleEmitter));
		Component[] array = componentsInChildren;
		foreach (Component component in array)
		{
			ParticleEmitter particleEmitter = component as ParticleEmitter;
			if (particleEmitter != null)
			{
				particleEmitter.emit = false;
			}
		}
		Component[] componentsInChildren2 = base.gameObject.GetComponentsInChildren(typeof(Renderer));
		Component[] array2 = componentsInChildren2;
		foreach (Component component2 in array2)
		{
			Renderer renderer = component2 as Renderer;
			if (renderer != null)
			{
				renderer.enabled = false;
			}
		}
		Done();
	}

	public override void Reborn()
	{
		if (mDieStart)
		{
			base.Reborn();
			mDieStart = false;
			ParticleEmitter[] array = base.gameObject.GetComponentsInChildren(typeof(ParticleEmitter)) as ParticleEmitter[];
			ParticleEmitter[] array2 = array;
			foreach (ParticleEmitter particleEmitter in array2)
			{
				particleEmitter.emit = true;
			}
			Renderer[] array3 = base.gameObject.GetComponentsInChildren(typeof(Renderer)) as Renderer[];
			Renderer[] array4 = array3;
			foreach (Renderer renderer in array4)
			{
				renderer.enabled = true;
			}
		}
	}
}
