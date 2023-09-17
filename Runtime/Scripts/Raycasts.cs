using System;
using UnityEngine;

namespace LagCompensation
{
	public static class Raycasts
	{
		/// <summary>
		/// Intersects a ray with a sphere.
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <returns>Returns the distance the ray and the box intersect, 
		/// will return positive infinity when no intersection happend.</returns>
		public static float RaySphereIntersect(SimpleRay ray, Vector3 center, float radius)
		{
			// https://gamedev.stackexchange.com/questions/96459/fast-ray-sphere-collision-code
			Vector3 rayToCenter = ray.Origin;
			rayToCenter.x -= center.x;
			rayToCenter.y -= center.y;
			rayToCenter.z -= center.z;

			float sqrRadius = radius * radius;

			// inline dot products/
			float b = rayToCenter.x * ray.Direction.x + rayToCenter.y * ray.Direction.y + rayToCenter.z * ray.Direction.z;
			float c = (rayToCenter.x * rayToCenter.x + rayToCenter.y * rayToCenter.y + rayToCenter.z * rayToCenter.z) - sqrRadius;

			// Exit if r’s origin outside s (c > 0) and r pointing away from s (b > 0) 
			if (c > 0 && b > 0)
				return float.PositiveInfinity;

			float discr = b * b - c;

			// A negative discriminant corresponds to ray missing sphere 
			if (discr < 0)
				return float.PositiveInfinity;

			// Ray now found to intersect sphere, compute smallest t value of intersection
			float t = -b - Mathf.Sqrt(discr);

			// If t is negative, ray started inside sphere so clamp t to zero 
			t = Mathf.Max(t, 0);
			return t;
		}

		/// <summary>
		/// Intersects a ray with a box that is definined by bounds and a matrix.
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="worldToLocal"></param>
		/// <param name="bounds"></param>
		/// <returns>Returns the distance the ray and the box intersect, 
		/// will return positive infinity when no intersection happend.</returns>
		public static float RayBoxIntersect(SimpleRay ray, in Matrix4x4 worldToLocal, SimpleBounds bounds)
		{
			Vector3 rayOrigin = worldToLocal.MultiplyPoint(ray.Origin);
			Vector3 rayDir = worldToLocal.MultiplyVector(ray.Direction);

			Vector3 boundsMin = bounds.Min;
			Vector3 boundsMax = bounds.Max;

			float t1 = (boundsMin.x - rayOrigin.x) / rayDir.x;
			float t2 = (boundsMax.x - rayOrigin.x) / rayDir.x;
			float t3 = (boundsMin.y - rayOrigin.y) / rayDir.y;
			float t4 = (boundsMax.y - rayOrigin.y) / rayDir.y;
			float t5 = (boundsMin.z - rayOrigin.z) / rayDir.z;
			float t6 = (boundsMax.z - rayOrigin.z) / rayDir.z;

			float aMin = t1 < t2 ? t1 : t2;
			float bMin = t3 < t4 ? t3 : t4;
			float cMin = t5 < t6 ? t5 : t6;

			float aMax = t1 > t2 ? t1 : t2;
			float bMax = t3 > t4 ? t3 : t4;
			float cMax = t5 > t6 ? t5 : t6;

			float fMax = aMin > bMin ? aMin : bMin;
			float fMin = aMax < bMax ? aMax : bMax;

			float t7 = fMax > cMin ? fMax : cMin;
			float t8 = fMin < cMax ? fMin : cMax;

			float t9 = (t8 < 0 || t7 > t8) ? float.PositiveInfinity : t7;

			return t9;
		}

		private static readonly Vector3[] directions = 
		{
			new Vector3(1, 0, 0),
			new Vector3(0, 1, 0),
			new Vector3(0, 0, 1),
		};
		
		public static float RayCapsuleIntersect(SimpleRay ray, in Matrix4x4 worldToLocal, Vector3 center, float radius, int directionIndex, float height)
		{
			Vector3 rayOrigin = worldToLocal.MultiplyPoint(ray.Origin);
			Vector3 rayDirection = worldToLocal.MultiplyVector(ray.Direction);

			// Calculate the vector from the ray's origin to the capsule's center
			Vector2 center2D;
			Vector2 rayOrigin2D;
			Vector2 rayDir2D;

			switch (directionIndex)
			{
				case 0:
					center2D = new Vector2(center.y, center.z);
					rayOrigin2D = new Vector2(rayOrigin.y, rayOrigin.z);
					rayDir2D = new Vector2(rayDirection.y, rayDirection.z);
					break;
				case 1:
					center2D = new Vector2(center.x, center.z);
					rayOrigin2D = new Vector2(rayOrigin.x, rayOrigin.z);
					rayDir2D = new Vector2(rayDirection.x, rayDirection.z);
					break;
				default:
					center2D = new Vector2(center.x, center.y);
					rayOrigin2D = new Vector2(rayOrigin.x, rayOrigin.y);
					rayDir2D = new Vector2(rayDirection.x, rayDirection.y);
					break;
			}
			
			Vector2 m = rayOrigin2D - center2D;

			// Calculate coefficients for the quadratic equation (cylinder part)
			float a = Vector2.Dot(rayDir2D, rayDir2D);
			float b = 2.0f * Vector2.Dot(rayDir2D, m);
			float c = Vector2.Dot(m, m) - (radius * radius);

			// Calculate the discriminant of the quadratic equation
			float discriminant = b * b - 4 * a * c;

			// Check for intersection with the cylindrical part
			float distanceToCylinder = float.PositiveInfinity;
			if (discriminant >= 0)
			{
				float sqrtDiscriminant = (float)Math.Sqrt(discriminant);
				float a2 = 2 * a;
				float t1 = (-b + sqrtDiscriminant) / a2;
				float t2 = (-b - sqrtDiscriminant) / a2;

				// Find the smallest positive t value
				if (t1 < 0 && t2 < 0)
				{
					return float.PositiveInfinity;
				}
				else
				{
					distanceToCylinder = Math.Min(t2, t1);
					if (distanceToCylinder <= 0)
						distanceToCylinder = 0;
				}
			}

			float heightBound = height * 0.5f - radius ;
			Vector3 pointOnCylinder = (rayOrigin + rayDirection * distanceToCylinder);

			switch (directionIndex)
			{
				case 0:
					if (pointOnCylinder.x > heightBound + center.x)
					{
						Vector3 pointA = center + new Vector3(heightBound, 0, 0);
						return RaySphereIntersect(new SimpleRay(rayOrigin, rayDirection), pointA, radius);
					}
					else if (pointOnCylinder.x < -heightBound + center.x)
					{
						Vector3 pointB = center - new Vector3(heightBound, 0, 0);
						return RaySphereIntersect(new SimpleRay(rayOrigin, rayDirection), pointB, radius);
					}
					break;
				case 1:
					if (pointOnCylinder.y > heightBound + center.y)
					{
						Vector3 pointA = center + new Vector3(0, heightBound, 0);
						return RaySphereIntersect(new SimpleRay(rayOrigin, rayDirection), pointA, radius);
					}
					else if (pointOnCylinder.y < -heightBound + center.y)
					{
						Vector3 pointB = center - new Vector3(0, heightBound, 0);
						return RaySphereIntersect(new SimpleRay(rayOrigin, rayDirection), pointB, radius);
					}
					break;
				default:
					if (pointOnCylinder.z > heightBound + center.z)
					{
						Vector3 pointA = center + new Vector3(0, 0, heightBound);
						return RaySphereIntersect(new SimpleRay(rayOrigin, rayDirection), pointA, radius);
					}
					else if (pointOnCylinder.z < -heightBound + center.z)
					{
						Vector3 pointB = center - new Vector3(0, 0, heightBound);
						return RaySphereIntersect(new SimpleRay(rayOrigin, rayDirection), pointB, radius);
					}
					break;
			}



			return distanceToCylinder;
		}
	}
}
