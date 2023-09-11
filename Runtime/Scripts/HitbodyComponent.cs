using UnityEngine;

namespace LagCompensation
{
	public class HitbodyComponent : MonoBehaviour
	{
		#region Editor Fields

		/// <summary>
		/// Proximity sphere radius.
		/// </summary>
		[SerializeField]
		private float _radius;

		/// <summary>
		/// Position offset for Proximity Sphere.
		/// </summary>
		[SerializeField]
		private Vector3 _offset;

		/// <summary>
		/// Array of child components.
		/// </summary>
		[SerializeField]
		private HitboxComponent[] _hitboxComponents = { };

		/// <summary>
		/// Array of child components.
		/// </summary>
		[SerializeField]
		private HitsphereComponent[] _hitsphereComponents = { };

		#endregion

		#region Properties

		/// <summary>
		/// Amount of child spheres.
		/// </summary>
		public int ChildSphereCount => _hitsphereComponents.Length;

		/// <summary>
		/// Amount of child boxes.
		/// </summary>
		public int ChildBoxCount => _hitboxComponents.Length;

		#endregion

		#region Methods

		[ContextMenu("Find hitboxes in children")]
		public void FindHitBoxesInChildren() 
		{
			_hitboxComponents = GetComponentsInChildren<HitboxComponent>();
			_hitsphereComponents = GetComponentsInChildren<HitsphereComponent>();
		}

		/// <summary>
		/// On Enable.
		/// </summary>
		public void OnEnable()
			=> HitboxWorld.AddBody(this);

		/// <summary>
		/// On Disable
		/// </summary>
		public void OnDisable()
			=> HitboxWorld.RemoveBody(this);

		/// <summary>
		/// Copy hit boxes to HitsphereArrays struct.
		/// </summary>
		/// <param name="hitboxArrays"></param>
		/// <param name="start"></param>
		public void CopySpheresToArray(ref HitsphereArrays hitsphereArrays)
		{
			Markers.Begin(Markers.CopySpheres);
			int count = _hitsphereComponents.Length;
			int start = hitsphereArrays.Count;

			for (int i = 0; i < count; i++)
			{
				hitsphereArrays.Radius[start + i]     = _hitsphereComponents[i].Radius; 
				hitsphereArrays.Center[start + i]     = _hitsphereComponents[i].Transform.position + _offset;
				hitsphereArrays.HitObjects[start + i] = _hitsphereComponents[i].GameObject;
			}

			hitsphereArrays.Count += count;
			Markers.End(Markers.CopySpheres);
		}

		/// <summary>
		/// Copy hit boxes to Hitboxarrays struct.
		/// </summary>
		/// <param name="hitboxArrays"></param>
		/// <param name="start"></param>
		public void CopyBoxesToArray(ref HitboxArrays hitboxArrays)
		{
			Markers.Begin(Markers.CopyBoxes);
			
			int count = _hitboxComponents.Length;
			int start = hitboxArrays.Count;

			for (int i = 0; i < count; i++)
			{
				hitboxArrays.Matrices[start + i]   = _hitboxComponents[i].Transfrom.worldToLocalMatrix;
				hitboxArrays.Bounds[start + i]     = _hitboxComponents[i].Bounds;
				hitboxArrays.HitObjects[start + i] = _hitboxComponents[i].GameObject;
			}

			hitboxArrays.Count += count;

			Markers.End(Markers.CopyBoxes);
		}

		/// <summary>
		/// Returns the proximity sphere.
		/// </summary>
		/// <returns></returns>
		public void CopyProximitySphereToArray(ref ProximityArrays proximityArrays)
		{
			Markers.Begin(Markers.CopyProximity);
			int index = proximityArrays.Count;

			proximityArrays.Center[index] = transform.position;
			proximityArrays.Radius[index] = _radius;
			proximityArrays.HitObjects[index] = gameObject;

			int currentBoxStart = proximityArrays.BoxStart[index];
			int currentSphereStart = proximityArrays.SphereStart[index];

			//Setup start of the next children.
			proximityArrays.BoxStart[index + 1] = currentBoxStart + ChildBoxCount;
			proximityArrays.SphereStart[index + 1] = currentSphereStart + ChildSphereCount;

			proximityArrays.Count++;
			Markers.End(Markers.CopyProximity);
		}

		#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			HitBoxGizmoHelper.DrawHitSphere(transform.position + _offset, _radius, new Color(1, 1, 0, 1));
		}

		#endif

		#endregion

	}
}