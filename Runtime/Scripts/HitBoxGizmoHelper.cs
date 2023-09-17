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
			color.a = 0.04f;
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

			color.a = 0.04f;
			Gizmos.color = color;
			Gizmos.DrawSphere(position, radius);

			Gizmos.color = Color.white;
		}

		public static void DrawCircle(Vector3 position, Quaternion rotation, float radius, int segments)
		{
			// If either radius or number of segments are less or equal to 0, skip drawing
			if (radius <= 0.0f || segments <= 0)
			{
				return;
			}

			// Single segment of the circle covers (360 / number of segments) degrees
			float angleStep = (360.0f / segments);

			// Result is multiplied by Mathf.Deg2Rad constant which transforms degrees to radians
			// which are required by Unity's Mathf class trigonometry methods

			angleStep *= Mathf.Deg2Rad;

			// lineStart and lineEnd variables are declared outside of the following for loop
			Vector3 lineStart = Vector3.zero;
			Vector3 lineEnd = Vector3.zero;

			for (int i = 0; i < segments; i++)
			{
				// Line start is defined as starting angle of the current segment (i)
				lineStart.x = Mathf.Cos(angleStep * i);
				lineStart.y = Mathf.Sin(angleStep * i);
				lineStart.z = 0.0f;

				// Line end is defined by the angle of the next segment (i+1)
				lineEnd.x = Mathf.Cos(angleStep * (i + 1));
				lineEnd.y = Mathf.Sin(angleStep * (i + 1));
				lineEnd.z = 0.0f;

				// Results are multiplied so they match the desired radius
				lineStart *= radius;
				lineEnd *= radius;

				// Results are multiplied by the rotation quaternion to rotate them 
				// since this operation is not commutative, result needs to be
				// reassigned, instead of using multiplication assignment operator (*=)
				lineStart = rotation * lineStart;
				lineEnd = rotation * lineEnd;

				// Results are offset by the desired position/origin 
				lineStart += position;
				lineEnd += position;

				// Points are connected using DrawLine method and using the passed color
				Gizmos.DrawLine(lineStart, lineEnd);
			}
		}

		public static void DrawArc(float startAngle, float endAngle,
			Vector3 position, Quaternion orientation, float radius, bool drawChord = false, bool drawSector = false,
			int arcSegments = 32)
		{
			float arcSpan = Mathf.DeltaAngle(startAngle, endAngle);

			// Since Mathf.DeltaAngle returns a signed angle of the shortest path between two angles, it 
			// is necessary to offset it by 360.0 degrees to get a positive value
			if (arcSpan <= 0)
			{
				arcSpan += 360.0f;
			}

			// angle step is calculated by dividing the arc span by number of approximation segments
			float angleStep = (arcSpan / arcSegments) * Mathf.Deg2Rad;
			float stepOffset = startAngle * Mathf.Deg2Rad;

			// stepStart, stepEnd, lineStart and lineEnd variables are declared outside of the following for loop
			float stepStart = 0.0f;
			float stepEnd = 0.0f;
			Vector3 lineStart = Vector3.zero;
			Vector3 lineEnd = Vector3.zero;

			// arcStart and arcEnd need to be stored to be able to draw segment chord
			Vector3 arcStart = Vector3.zero;
			Vector3 arcEnd = Vector3.zero;

			// arcOrigin represents an origin of a circle which defines the arc
			Vector3 arcOrigin = position;

			for (int i = 0; i < arcSegments; i++)
			{
				// Calculate approximation segment start and end, and offset them by start angle
				stepStart = angleStep * i + stepOffset;
				stepEnd = angleStep * (i + 1) + stepOffset;

				lineStart.x = Mathf.Cos(stepStart);
				lineStart.y = Mathf.Sin(stepStart);
				lineStart.z = 0.0f;

				lineEnd.x = Mathf.Cos(stepEnd);
				lineEnd.y = Mathf.Sin(stepEnd);
				lineEnd.z = 0.0f;

				// Results are multiplied so they match the desired radius
				lineStart *= radius;
				lineEnd *= radius;

				// Results are multiplied by the orientation quaternion to rotate them 
				// since this operation is not commutative, result needs to be
				// reassigned, instead of using multiplication assignment operator (*=)
				lineStart = orientation * lineStart;
				lineEnd = orientation * lineEnd;

				// Results are offset by the desired position/origin 
				lineStart += position;
				lineEnd += position;

				// If this is the first iteration, set the chordStart
				if (i == 0)
				{
					arcStart = lineStart;
				}

				// If this is the last iteration, set the chordEnd
				if (i == arcSegments - 1)
				{
					arcEnd = lineEnd;
				}

				Gizmos.DrawLine(lineStart, lineEnd);
			}

			if (drawChord)
			{
				Gizmos.DrawLine(arcStart, arcEnd);
			}
			if (drawSector)
			{
				Gizmos.DrawLine(arcStart, arcOrigin);
				Gizmos.DrawLine(arcEnd, arcOrigin);
			}
		}

		public static void DrawCylinder(Vector3 position, Quaternion orientation, float height, float radius, bool drawFromBase = true)
		{
			Vector3 localUp = orientation * Vector3.up;
			Vector3 localRight = orientation * Vector3.right;
			Vector3 localForward = orientation * Vector3.forward;

			Vector3 basePositionOffset = drawFromBase ? Vector3.zero : (localUp * height * 0.5f);
			Vector3 basePosition = position - basePositionOffset;
			Vector3 topPosition = basePosition + localUp * height;

			Quaternion circleOrientation = orientation * Quaternion.Euler(90, 0, 0);

			Vector3 pointA = basePosition + localRight * radius;
			Vector3 pointB = basePosition + localForward * radius;
			Vector3 pointC = basePosition - localRight * radius;
			Vector3 pointD = basePosition - localForward * radius;

			Gizmos.DrawRay(pointA, localUp * height);
			Gizmos.DrawRay(pointB, localUp * height);
			Gizmos.DrawRay(pointC, localUp * height);
			Gizmos.DrawRay(pointD, localUp * height);

			DrawCircle(basePosition, circleOrientation, radius, 32);
			DrawCircle(topPosition, circleOrientation, radius, 32);
		}

		public static void DrawCapsule(Vector3 position, Quaternion orientation, float height, float radius, bool drawFromBase = true)
		{
			// Clamp the radius to a half of the capsule's height
			radius = Mathf.Clamp(radius, 0, height * 0.5f);
			Vector3 localUp = orientation * Vector3.up;
			Quaternion arcOrientation = orientation * Quaternion.Euler(0, 90, 0);

			Vector3 baseArcPosition = drawFromBase ? 
				position + localUp * radius : position - 
				localUp * (height * 0.5f - radius);
			
			DrawArc(180, 360, baseArcPosition, orientation, radius);
			DrawArc(180, 360, baseArcPosition, arcOrientation, radius);

			float cylinderHeight = height - radius * 2.0f;
			DrawCylinder(baseArcPosition, orientation, cylinderHeight, radius, true);

			Vector3 topArcPosition = baseArcPosition + localUp * cylinderHeight;

			DrawArc(0, 180, topArcPosition, orientation, radius);
			DrawArc(0, 180, topArcPosition, arcOrientation, radius);
		}


		private static Mesh capsuleMesh;
		public static void DrawHitCapsule(Vector3 position, Vector3 direction, float height, float radius, Color color) 
		{
			// Create new mesh instantiate
			if (capsuleMesh == null)
				capsuleMesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");

			color.a = 0.04f;
			Gizmos.color = color;
			Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
			Gizmos.DrawMesh(capsuleMesh, position, rotation, new Vector3(radius, height * 0.25f,radius) );

			color.a = 0.65f;
			Gizmos.color = color;
			DrawCapsule(position - direction * height * 0.5f, rotation, height, radius);
			Gizmos.color = Color.white;
		}

	}
}

#endif

