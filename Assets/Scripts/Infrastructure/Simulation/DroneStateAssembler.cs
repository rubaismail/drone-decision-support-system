using Core.Data;
using Core.Interfaces;
using UnityEngine;

namespace Infrastructure.Simulation
{
    public class DroneStateAssembler
    {
        private readonly IDroneStateProvider _droneProvider;
        private readonly IGroundHeightProvider _groundProvider;
        
        private bool _hasCachedGround = false;
        private float _lastGroundHeight;

        public DroneStateAssembler(
            IDroneStateProvider droneProvider,
            IGroundHeightProvider groundProvider)
        {
            _droneProvider = droneProvider;
            _groundProvider = groundProvider;
        }

        public DroneState BuildState()
        {
            DroneState baseState = _droneProvider.GetCurrentState();

            float altitudeAGL;

            if (_groundProvider.TryGetGroundHeight(baseState.position, out float groundHeight))
            {
                _lastGroundHeight = groundHeight;
                _hasCachedGround = true;
            }

            if (_hasCachedGround)
            {
                altitudeAGL = baseState.position.y - _lastGroundHeight;
            }
            else
            {
                //fallback 
                altitudeAGL = Mathf.Max(0.1f, baseState.position.y);
            }

            return new DroneState(
                position: baseState.position,
                velocity: baseState.velocity,
                forward: baseState.forward,
                mass: baseState.mass,
                altitudeAboveGround: altitudeAGL,
                timestamp: Time.time
            );
        }
    }
}