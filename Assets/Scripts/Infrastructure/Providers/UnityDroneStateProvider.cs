using CesiumForUnity;
using UnityEngine;
using Core.Data;
using Core.Interfaces;

namespace Infrastructure.Providers
{
    public class UnityDroneStateProvider : MonoBehaviour, IDroneStateProvider
    {
        [Header("Drone References")] 
        [SerializeField] private Transform droneTransform;
        [SerializeField] private Rigidbody droneRigidbody;
        [SerializeField] private CesiumGeoreference georeference;

        [Header("Physical Properties")] [SerializeField]
        private float massKg = 2.0f;
        
        [Header("Geometry")]
        [SerializeField] private float bottomOffsetMeters = 0.0f;
        
        private Vector3 _lastPosition;
        private float _lastSampleTime;
        private bool _hasPreviousSample;
        
        public Transform DroneTransform => droneTransform;
        
        public void SetMassKg(float newMassKg)
        {
            massKg = Mathf.Max(0.05f, newMassKg); // safety floor
            if (droneRigidbody != null)
            {
                droneRigidbody.mass = massKg;
            }
        }

        public DroneState GetCurrentState()
        {
            Vector3 currentPosition = droneTransform.position;
            float currentTime = Time.time;

            Vector3 computedVelocity = Vector3.zero;

            if (_hasPreviousSample)
            {
                float dt = currentTime - _lastSampleTime;
                if (dt > Mathf.Epsilon)
                {
                    computedVelocity = (currentPosition - _lastPosition) / dt;
                }
            }

            _lastPosition = currentPosition;
            _lastSampleTime = currentTime;
            _hasPreviousSample = true;
            
            return new DroneState
            {
                position = droneTransform.position,
                velocity = computedVelocity,
                forward   = droneTransform.forward,
                mass = massKg,
                timestamp = Time.time,
                bottomOffsetMeters = bottomOffsetMeters
            };
        }
    }
}