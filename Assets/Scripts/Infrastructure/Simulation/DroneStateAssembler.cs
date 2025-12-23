using Core.Data;
using Core.Interfaces;
using UnityEngine;

namespace Infrastructure.Simulation
{
    public class DroneStateAssembler
    {
        private readonly IDroneStateProvider _droneProvider;
        private readonly IGroundHeightProvider _groundProvider;

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

            float groundHeight;
            float altitudeAGL = 0f;

            if (_groundProvider.TryGetGroundHeight(baseState.position, out groundHeight))
            {
                altitudeAGL = baseState.position.y - groundHeight;
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