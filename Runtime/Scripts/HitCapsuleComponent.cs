using LagCompensation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LagCompensation
{
	public class HitCapsuleComponent : MonoBehaviour
	{
		#region Editor Field

		public enum CapsuleDirection
		{
			x, y, z
		}

		public Vector3 Center;
		public float Radius = 0.5f;
		public float Height = 1;
		public CapsuleDirection Direction;

		/// <summary>
		/// Cached transform.
		/// </summary>
		[NonSerialized]
		public Transform Transform;

		/// <summary>
		/// Cached transform.
		/// </summary>
		[NonSerialized]
		public GameObject GameObject;

		#endregion


		#region Initialization

		private void Awake()
		{
			GameObject = gameObject;
			Transform = transform;
		}

		#endregion

#if UNITY_EDITOR

		/// <summary>
		/// Draw gizmos
		/// </summary>
		private void OnDrawGizmos()
		{
			Transform = transform;
			Gizmos.matrix = Transform.localToWorldMatrix;
			Vector3 dir = Direction switch
			{
				CapsuleDirection.x => new Vector3(1, 0, 0),
				CapsuleDirection.y => new Vector3(0, 1, 0),
				CapsuleDirection.z => new Vector3(0, 0, 1),
				_ => Vector3.zero
			};

			HitBoxGizmoHelper.DrawHitCapsule(Center, dir, Height, Radius, Color.green);
			Gizmos.matrix = Matrix4x4.identity;
		}

#endif
	}
}