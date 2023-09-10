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
		public Transform Transfrom;

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
			Transfrom = transform;
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
			Transfrom = transform;
			Bounds = new SimpleBounds(_center, _size * 0.5f);
			HitBoxGizmoHelper.DrawHitbox(Transfrom.localToWorldMatrix, Bounds, Color.green);
		}

#endif

		#endregion
	}
}