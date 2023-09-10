
namespace LagCompensation
{
	public static class WorldConsts
	{
		public const int MaxRewindFrames = 60;

		public const int MaxProximityCount = 64;
		public const int MaxBoxesPerBody = 16;
		public const int MaxSpheresPerBody = 8;
		public const int MaxChilderenPerProximity = MaxBoxesPerBody + MaxSpheresPerBody;
		public const int MaxObjects = MaxProximityCount * MaxChilderenPerProximity;

		public const int MaxBoxCount = MaxProximityCount * MaxBoxesPerBody;
		public const int MaxSphereCount = MaxProximityCount * MaxSpheresPerBody;
	}
}
