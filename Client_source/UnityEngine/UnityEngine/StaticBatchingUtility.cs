namespace UnityEngine
{
	public sealed class StaticBatchingUtility
	{
		public static void Combine(GameObject staticBatchRoot)
		{
			InternalStaticBatchingUtility.Combine(staticBatchRoot, generateTriangleStrips: true);
		}

		public static void Combine(GameObject[] gos, GameObject staticBatchRoot)
		{
			InternalStaticBatchingUtility.Combine(gos, staticBatchRoot, generateTriangleStrips: true);
		}
	}
}
