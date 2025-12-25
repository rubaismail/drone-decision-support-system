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
        
        public Transform DroneTransform => droneTransform;

        public DroneState GetCurrentState()
        {
            return new DroneState
            {
                position = droneTransform.position,
                velocity = droneRigidbody.linearVelocity,
                forward   = droneTransform.forward,
                mass = massKg,
                timestamp = Time.time
            };
        }
    }
}