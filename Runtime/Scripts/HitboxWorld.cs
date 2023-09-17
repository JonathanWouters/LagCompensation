using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LagCompensation
{
	public struct Hitsphere
	{
		public float Radius;
		public Vector3 Center;
		public GameObject HitObject;
	}

	public struct Hitbox
	{
		public Matrix4x4 LocalToWorld;
		public Bounds bounds;
		public GameObject HitObject;
	}

	public struct HitCapsuleArrays
	{
		public readonly Matrix4x4[] Matrices;
		public readonly Vector3[] Center;
		public readonly int[] direction;
		public readonly float[] Height;
		public readonly float[] Radius;
		public readonly GameObject[] HitObjects;

		public int Count;
		public readonly int MaxSize;

		public HitCapsuleArrays(int size)
		{
			Center = new Vector3[size];
			direction = new int[size];
			Radius = new float[size];
			Height = new float[size];
			Matrices = new Matrix4x4[size];
			HitObjects = new GameObject[size];
			MaxSize = size;
			Count = 0;
		}
	}

	public struct HitboxArrays
	{
		public readonly Matrix4x4[] Matrices;
		public readonly SimpleBounds[] Bounds;
		public readonly GameObject[] HitObjects;

		public int Count;
		public readonly int MaxSize;

		public HitboxArrays(int size)
		{
			Matrices = new Matrix4x4[size];
			Bounds = new SimpleBounds[size];
			HitObjects = new GameObject[size];
			MaxSize = size;
			Count = 0;
		}
	}

	public struct HitsphereArrays
	{
		// TODO: might be better to merge radius and center. They are usually used together.
		public readonly float[] Radius;
		public readonly Vector3[] Center;
		
		public readonly GameObject[] HitObjects;

		public int Count;
		public readonly int MaxSize;

		public HitsphereArrays(int size)
		{
			Radius = new float[size];
			Center = new Vector3[size];
			HitObjects = new GameObject[size];

			MaxSize = size;
			Count = 0;
		}
	}

	public struct ProximityArrays
	{
		public readonly float[] Radius;
		public readonly Vector3[] Center;
		public readonly GameObject[] HitObjects;
		public readonly int[] BoxStart;
		public readonly int[] SphereStart;
		public readonly int[] CapsuleStart;

		public int Count;
		public readonly int MaxSize;

		public ProximityArrays(int size)
		{
			Radius = new float[size];
			Center = new Vector3[size];
			HitObjects = new GameObject[size];
			BoxStart = new int[size + 1];
			SphereStart = new int[size + 1];
			CapsuleStart = new int[size + 1];

			MaxSize = size;
			Count = 0;
		}
	}

	public struct SimpleBounds
	{
		public Vector3 Min;
		public Vector3 Max;

		public SimpleBounds(Bounds bounds)
		{
			Min = bounds.min;
			Max = bounds.max;
		}

		public SimpleBounds(Vector3 center, Vector3 extends)
		{
			Min = center - extends;
			Max = center + extends;
		}
	}

	public struct SimpleRay
	{
		public Vector3 Origin;
		public Vector3 Direction;

		public SimpleRay(Ray ray)
		{
			Origin = ray.origin;
			Direction = ray.direction;
		}

		public SimpleRay(Vector3 origin, Vector3 direction)
		{
			Origin = origin;
			Direction = direction;
		}
	}

	public struct HitInfo
	{
		public float Distance;
		public Vector3 Point;
		public GameObject HitObject;
	}
	
	public class HitboxWorld : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// Current frame.
		/// </summary>
		static int _frame = 0;

		/// <summary>
		/// List of all snapshots.
		/// </summary>
		private static WorldSnapshot[] _worldSnapShots = 
			new WorldSnapshot[WorldConsts.MaxRewindFrames];
	
		/// <summary>
		/// List of all hit box components in the scene.
		/// </summary>
		private static List<HitbodyComponent> _hitBoxComponents = 
			new List<HitbodyComponent>(WorldConsts.MaxProximityCount);

		#endregion

		#region Properties

		public static int LastFrame => _frame;

		#endregion

		#region Initialization

		/// <summary>
		/// Awake.
		/// </summary>
		private void Awake()
		{
			for (int i = 0; i < _worldSnapShots.Length; i++)
				_worldSnapShots[i] = new WorldSnapshot();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Add a body to the world.
		/// </summary>
		/// <param name="component"></param>
		public static void AddBody(HitbodyComponent component)
		{
			#if DEBUG
			Debug.Assert(_hitBoxComponents.Contains(component) == false,
				"List can't contain same HitbodyComponent twice !");
			#endif
			_hitBoxComponents.Add(component);
		}
	
		/// <summary>
		/// Remove a body from the world.
		/// </summary>
		/// <param name="component"></param>
		public static void RemoveBody(HitbodyComponent component)
		{
			#if DEBUG
			Debug.Assert(_hitBoxComponents.Contains(component),
				"List doesn't contain HitbodyComponent!");
			#endif
			
			_hitBoxComponents.Remove(component);
		}

		/// <summary>
		/// Raycast against a given frame.
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="ray"></param>
		public static bool Raycast(int frame, Ray ray, out HitInfo info)
		{
			int delta = _frame - frame;
			
			#if DEBUG
				Debug.Assert(delta < WorldConsts.MaxRewindFrames, "Frame to far in the past");
				Debug.Assert(delta >= 0, "Frame is in the future");
			#endif

			delta = Mathf.Clamp(delta, 0, WorldConsts.MaxRewindFrames - 1);

			return _worldSnapShots[delta].Raycast(ray,out info);
		}

		public static void SaveFrame(int frame) 
		{
			_frame = frame;
			Markers.Begin(Markers.CopySnapshots);

			WorldSnapshot currentSnapshot = _worldSnapShots[WorldConsts.MaxRewindFrames -1];
			// Shift snapshot array
			Array.Copy(_worldSnapShots, 0, _worldSnapShots, 1, WorldConsts.MaxRewindFrames - 1);
			_worldSnapShots[0] = currentSnapshot;

			Markers.End(Markers.CopySnapshots);
	
			// Initialize the new frame.
			currentSnapshot.SaveFrame(frame, _hitBoxComponents);
		}

		public static void GetSnapShot(int frame, out WorldSnapshot snapshot)
		{
			int delta = _frame - frame;

#if DEBUG
			Debug.Assert(delta < WorldConsts.MaxRewindFrames, "Frame to far in the past");
			Debug.Assert(delta >= 0, "Frame is in the future");
#endif

			delta = Mathf.Clamp(delta, 0, WorldConsts.MaxRewindFrames - 1);
			snapshot = _worldSnapShots[delta];
		}

		/// <summary>
		/// Fixed Update
		/// </summary>
		// private void FixedUpdate()
		// {
		// 	_frame++;
		// 	SaveFrame(_frame);
		// }

#if UNITY_EDITOR
		// TODO: Move to editor script.
		[Range(0, WorldConsts.MaxRewindFrames - 1)]
		public int FramesBack = 10; 
		private void OnDrawGizmos()
		{
			if (Application.isPlaying == false)
				return;
			
			//WorldSnapshot snapshot = _worldSnapShots[FramesBack];
			//
			////Draw boxes.
			//var allBounds = snapshot.HitboxArrays.Bounds;
			//var allMatrices = snapshot.HitboxArrays.Matrices;
			//
			//for (int i = 0; i < allMatrices.Length; i++)
			//	HitBoxGizmoHelper.DrawHitbox(allMatrices[i].inverse, allBounds[i], Color.red);
			//
			////Draw Spheres.
			//var allPositions = snapshot.SphereArrays.Center;
			//var allRadia = snapshot.SphereArrays.Radius;
			//
			//for (int i = 0; i < allPositions.Length; i++)
			//	HitBoxGizmoHelper.DrawHitSphere(allPositions[i], allRadia[i], Color.red);
		}
#endif


		#endregion
	}
}
