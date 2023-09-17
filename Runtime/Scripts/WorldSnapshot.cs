using System;
using System.Collections.Generic;
using UnityEngine;

namespace LagCompensation
{
	public class WorldSnapshot
	{
		#region Fields

		public int Frame;

		private int _bodiesCount = 0;
		private HitbodyComponent[] _bodies = new HitbodyComponent[WorldConsts.MaxBodies];
		private (int start, int count)[] _hitBoxSpan = new (int, int)[WorldConsts.MaxBodies];
		private (int start, int count)[] _hitSphereSpan = new (int, int)[WorldConsts.MaxBodies];

		public ProximityArrays _proximityArrays = new ProximityArrays(WorldConsts.MaxProximityCount);

		public HitboxArrays _hitboxArrays = new HitboxArrays(WorldConsts.MaxBoxCount);

		public HitsphereArrays _hitsphereArrays = new HitsphereArrays(WorldConsts.MaxSphereCount);
		
		public HitCapsuleArrays _hitCapsuleArrays = new HitCapsuleArrays(WorldConsts.MaxCapsuleCount);

		//Cached arrays.
		private readonly static HitboxArrays _cachedHitBoxes = new HitboxArrays(WorldConsts.MaxBoxCount);
		private readonly static HitsphereArrays _cachedHitSpheres = new HitsphereArrays(WorldConsts.MaxSphereCount);
		private readonly static HitCapsuleArrays _cachedHitCapsules = new HitCapsuleArrays(WorldConsts.MaxCapsuleCount);

		private readonly static bool[] _cachedBools = new bool[WorldConsts.MaxProximityCount];
		private readonly static float[] _cachedFloats = new float[WorldConsts.MaxObjects];

		#endregion

		#region Properties

		/// <summary>
		/// Reference to all hitboxes in the snapshot.
		/// </summary>
		public HitboxArrays HitboxArrays => _hitboxArrays;

		/// <summary>
		/// Reference to all spheres in the snapshot.
		/// </summary>
		public HitsphereArrays SphereArrays => _hitsphereArrays;

		/// <summary>
		/// Reference to all Capsules in the snapshot.
		/// </summary>
		public HitCapsuleArrays CapsuleArrays => _hitCapsuleArrays;

		/// <summary>
		/// Reference to all proximities in the snapshot.
		/// </summary>
		public ProximityArrays ProximityArrays => _proximityArrays;

		#endregion

		#region Methods
		public void SaveFrame(int frame, List<HitbodyComponent> hitBodies)
		{
			Frame = frame;

			_hitsphereArrays.Count  = 0;
			_hitboxArrays.Count     = 0;
			_proximityArrays.Count  = 0;
			_hitCapsuleArrays.Count = 0;

			// Add all box components to snapshot.
			int componentCount = hitBodies.Count;
			_bodiesCount= componentCount;

			for (int i = 0; i < componentCount; i++)
			{
				_hitSphereSpan[i].start = _hitsphereArrays.Count;
				_hitBoxSpan[i].start = _hitboxArrays.Count;

				hitBodies[i].CopyProximitySphereToArray(ref _proximityArrays);
				hitBodies[i].CopySpheresToArray(ref _hitsphereArrays);
				hitBodies[i].CopyBoxesToArray(ref _hitboxArrays);
				hitBodies[i].CopyCapsulesToArray(ref _hitCapsuleArrays);

				_bodies[i] = hitBodies[i];
				_hitSphereSpan[i].count = _hitsphereArrays.Count - _hitSphereSpan[i].start;
				_hitBoxSpan[i].count = _hitboxArrays.Count - _hitBoxSpan[i].start;
			}
		}

		public bool Raycast(Ray ray, out HitInfo hitInfo)
		{
			hitInfo = new HitInfo
			{
				Distance = float.PositiveInfinity
			};

			SimpleRay simpleRay = new SimpleRay(ray);

			bool[] hitProximity = _cachedBools;
			int proximityCount = _proximityArrays.Count;

			// Raycast against proximity hitspheres
			for (int i = 0; i < proximityCount; i++)
			{
				hitProximity[i] = float.IsPositiveInfinity(Raycasts.RaySphereIntersect(simpleRay, _proximityArrays.Center[i], _proximityArrays.Radius[i])) == false;
			}

			// Group all the child Hitspheres and Hitboxes in arrays.
			HitboxArrays childHitBoxes = _cachedHitBoxes;
			HitsphereArrays childHitSpheres = _cachedHitSpheres;
			HitCapsuleArrays childHitCapsules = _cachedHitCapsules;
			childHitBoxes.Count = 0;
			childHitSpheres.Count = 0;
			childHitCapsules.Count = 0;

			for (int i = 0; i < proximityCount; i++)
			{
				if (hitProximity[i] == false) 
					continue;

				int index = _proximityArrays.BoxStart[i];
				int lenght = _proximityArrays.BoxStart[i + 1] - index;
				int destIndex = childHitBoxes.Count;

				Array.Copy(_hitboxArrays.Bounds, index, childHitBoxes.Bounds, destIndex, lenght);
				Array.Copy(_hitboxArrays.Matrices, index, childHitBoxes.Matrices, destIndex, lenght);
				Array.Copy(_hitboxArrays.HitObjects, index, childHitBoxes.HitObjects, destIndex, lenght);

				childHitBoxes.Count += lenght;

				index = _proximityArrays.SphereStart[i];
				lenght = _proximityArrays.SphereStart[i + 1] - index;
				destIndex = childHitSpheres.Count;

				Array.Copy(_hitsphereArrays.Center, index, childHitSpheres.Center, destIndex, lenght);
				Array.Copy(_hitsphereArrays.Radius, index, childHitSpheres.Radius, destIndex, lenght);
				Array.Copy(_hitsphereArrays.HitObjects, index, childHitSpheres.HitObjects, destIndex, lenght);

				childHitSpheres.Count += lenght;

				index = _proximityArrays.CapsuleStart[i];
				lenght = _proximityArrays.CapsuleStart[i + 1] - index;
				destIndex = childHitCapsules.Count;

				Array.Copy(_hitCapsuleArrays.Center, index, _hitCapsuleArrays.Center, destIndex, lenght);
				Array.Copy(_hitCapsuleArrays.Height, index, _hitCapsuleArrays.Height, destIndex, lenght);
				Array.Copy(_hitCapsuleArrays.Radius, index, _hitCapsuleArrays.Radius, destIndex, lenght);
				Array.Copy(_hitCapsuleArrays.Matrices, index, _hitCapsuleArrays.Matrices, destIndex, lenght);
				Array.Copy(_hitCapsuleArrays.direction, index, _hitCapsuleArrays.direction, destIndex, lenght);
				Array.Copy(_hitCapsuleArrays.HitObjects, index, _hitCapsuleArrays.HitObjects, destIndex, lenght);
				
				childHitCapsules.Count += lenght;
			}

			int boxCount = childHitBoxes.Count;
			int sphereCount = childHitSpheres.Count;
			int capsuleCount = childHitSpheres.Count;
			int totalChildCount = boxCount + sphereCount + capsuleCount;
			if (totalChildCount == 0)
				return false;

			//Raycast against all potential boxes and spheres.
			float[] distances = _cachedFloats;


			for (int i = 0; i < boxCount; i++)
				distances[i] = Raycasts.RayBoxIntersect(simpleRay, in childHitBoxes.Matrices[i], childHitBoxes.Bounds[i]);

			for (int i = 0; i < sphereCount; i++)
				distances[boxCount + i] = Raycasts.RaySphereIntersect(simpleRay, childHitSpheres.Center[i], childHitSpheres.Radius[i]);

			//for (int i = 0; i < capsuleCount; i++)
			//	distances[capsuleCount + i] = Raycasts.RayCapsuleIntersect(simpleRay, childHitCapsules.Matrices[i], childHitCapsules.Center[i], childHitCapsules.Radius[i], childHitCapsules.direction[i]);

			// Find closest.
			int closestIndex = -1;
			float closestDistance = float.PositiveInfinity;
			for (int i = 0; i < totalChildCount; i++)
			{
				if (closestDistance <= distances[i])
					continue;

				closestIndex = i;
				closestDistance = distances[i];
			}

			if (closestIndex == -1)
				return false;

			if (closestIndex < boxCount)
			{
				hitInfo.HitObject = childHitBoxes.HitObjects[closestIndex];
			}
			else
			{
				closestIndex = closestIndex - boxCount;
				hitInfo.HitObject = childHitSpheres.HitObjects[closestIndex];
			}

			hitInfo.Distance = closestDistance;
			hitInfo.Point = simpleRay.Origin + simpleRay.Direction * closestDistance;

			return true;
		}

		public bool TryGetHitboxesForBody(HitbodyComponent body, out (int ,int) boxes, out (int,int) spheres) 
		{
			for (int i = 0; i < _bodiesCount; i++)
			{
				if (_bodies[i] != body)
					continue;

				boxes   = _hitBoxSpan[i];
				spheres = _hitSphereSpan[i];
				return true;
			}

			boxes = default;
			spheres = default;
			return false;
		}
		
		#endregion

	}
}
