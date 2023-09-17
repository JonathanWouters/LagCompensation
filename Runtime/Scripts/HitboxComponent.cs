using System;
using UnityEngine;

namespace LagCompensation
{
	public class HitboxComponent : MonoBehaviour
	{
		#region Editor Fields

		/// <summary>
		/// Center.
		/// </summary>
		[SerializeField]
		private Vector3 _center;

		/// <summary>
		/// Size.
		/// </summary>
		[SerializeField]
		private Vector3 _size = Vector3.one;

		#endregion

		#region Fields

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

		/// <summary>
		/// bounds.
		/// </summary>
		[NonSerialized]
		public SimpleBounds Bounds;

		#endregion

		#region Initialization

		private void Awake()
		{
			GameObject = gameObject;
			Transform = transform;
			Bounds = new SimpleBounds(_center, _size * 0.5f);
		}

		#endregion

		#region Methods



#if UNITY_EDITOR

		/// <summary>
		/// Draw gizmos
		/// </summary>
		private void OnDrawGizmos()
		{
			Transform = transform;
			Bounds = new SimpleBounds(_center, _size * 0.5f);
			HitBoxGizmoHelper.DrawHitbox(Transform.localToWorldMatrix, Bounds, Color.green);
		}

#endif

		#endregion
	}
}