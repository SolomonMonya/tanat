using UnityEngine;

[AddComponentMenu("FXs/EmitParticles")]
public class EmitParticles : MonoBehaviour
{
	public int mParticlesCount;

	private void Start()
	{
		ParticleEmitter[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleEmitter>();
		ParticleEmitter[] array = componentsInChildren;
		foreach (ParticleEmitter particleEmitter in array)
		{
			particleEmitter.Emit(mParticlesCount);
		}
	}
}
