using System;
using UnityEngine;

namespace LagCompensation
{
	public class HitsphereComponent : MonoBehaviour
	{
		#region Editor Fields

		/// <summary>
		/// Radius.
		/// </summary>
		public float Radius = 1;

		/// <summary>
		/// Position Offset
		/// </summary>
		public Vector3 Center;

		#endregion

		#region Fields

		/// <summary>
		/// This transform.
		/// </summary>
		[NonSerialized]
		public Transform Transform;

		/// <summary>
		/// This transform.
		/// </summary>
		[NonSerialized]
		public GameObject GameObject;

		#endregion

		#region Initialization

		private void Awake()
		{
			Transform = transform;
			GameObject = gameObject;
		}

		#endregion

		#region Methods

		#if UNITY_EDITOR


		private void OnDrawGizmos()
		{
			Transform = transform;
			HitBoxGizmoHelper.DrawHitSphere(Transform.position + Center, Radius, Color.green);
		}
		#endif

		#endregion
	}

}
