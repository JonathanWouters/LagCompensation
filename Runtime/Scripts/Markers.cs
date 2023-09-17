using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Profiling;

namespace LagCompensation
{
	public static class Markers
	{
		public const int CopySnapshots = 0;
		public const int CopySpheres   = 1;
		public const int CopyBoxes     = 2;
		public const int CopyProximity = 3;
		public const int CopyCapsules = 4;

		private static readonly ProfilerMarker[] markers =
		{
			new ProfilerMarker(nameof( CopySnapshots )),
			new ProfilerMarker(nameof( CopySpheres   )),
			new ProfilerMarker(nameof( CopyBoxes     )),
			new ProfilerMarker(nameof( CopyProximity )),
			new ProfilerMarker(nameof( CopyCapsules )),
		};
		
		[Conditional("HITBOXES_PROFILING")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Begin(int type) 
			=> markers[type].Begin();

		[Conditional("HITBOXES_PROFILING")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void End(int type) 
			=> markers[type].End(); 
	}
}
