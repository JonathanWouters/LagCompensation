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
		[SerializeField]
		public float Radius = 1;

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
			// // :NOTE: using 1 max Mathf.Max(localScale.x, localScale.y, localScale.z) generates garbage!
			// Vector3 localScale = Transform.localScale;
			// float scale = Mathf.Max(Mathf.Max(localScale.x, localScale.y), localScale.z);
			HitBoxGizmoHelper.DrawHitSphere(Transform.position, Radius /** scale*/, Color.green);
		}
		#endif

		#endregion
	}

}
