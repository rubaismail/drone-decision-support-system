using UnityEngine;
using Core.Data;
using Core.Interfaces;

namespace Infrastructure.Providers
{
    public class UnityDroneStateProvider : MonoBehaviour, IDroneStateProvider
    {
        [Header("Drone References")]
        [SerializeField] private Transform _droneTransform;
        [SerializeField] private Rigidbody _droneRigidbody;

        [Header("Physical Properties")]
        [SerializeField] private float _massKg = 2.0f;

        public DroneState GetCurrentState()
        {
            return new DroneState
            {
                position = _droneTransform.position,
                velocity = _droneRigidbody.linearVelocity,
                mass = _massKg
            };
        }
    }
}