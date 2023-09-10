using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

namespace LagCompensation
{
	public static class HitBoxGizmoHelper
	{

		/// <summary>
		/// Draw hit box gizmo.
		/// </summary>
		/// <param name="localToWorld"></param>
		/// <param name="bounds"></param>
		public static void DrawHitbox(Matrix4x4 localToWorld, SimpleBounds bounds, Color color)
		{
			Vector3 size = bounds.Max - bounds.Min;
			Vector3 center = (bounds.Min + bounds.Max) / 2;

			Gizmos.matrix = localToWorld;
			color.a = 0.1f;
			Gizmos.color = color;
			Gizmos.DrawCube(center, size * 1.001f);
			color.a = 0.65f;
			Gizmos.color = color;
			Gizmos.DrawWireCube(center, size * 1.001f);

			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.white;
		}

		/// <summary>
		/// Draw hit sphere.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="radius"></param>
		public static void DrawHitSphere(Vector3 position, float radius, Color color)
		{
			color.a = 0.65f;
			Gizmos.color = color;
			Gizmos.DrawWireSphere(position, radius);

			color.a = 0.1f;
			Gizmos.color = color;
			Gizmos.DrawSphere(position, radius);

			Gizmos.color = Color.white;
		}

	}
}

#endif

