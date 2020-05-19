using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class represents a camera view, and consists mainly of a transform that the camera follows
    /// for position and rotation.
    /// </summary>
    public class CameraViewTarget : MonoBehaviour
    {
	
		[SerializeField] 
		private VehicleCameraView cameraView;
		public VehicleCameraView CameraView { get { return cameraView; } }

        [SerializeField]
        private bool lockPosition;
        public bool LockPosition { get { return lockPosition; } }

        [SerializeField]
        private bool lockRotation;
        public bool LockRotation { get { return lockRotation; } }

        [SerializeField]
        private float positionFollowStrength = 0.4f;
        public float PositionFollowStrength { get { return positionFollowStrength; } }

        [SerializeField]
        private float rotationFollowStrength = 0.08f;
        public float RotationFollowStrength { get { return rotationFollowStrength; } }

        [SerializeField]
        private float spinOffsetCoefficient = 1f;
        public float SpinOffsetCorefficient { get { return spinOffsetCoefficient; } }

        private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }


		void Awake()
		{
			cachedTransform = transform;
		}
	}
}
