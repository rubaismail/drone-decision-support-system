using UnityEngine;

namespace Core.Data
{
    [System.Serializable]
    public struct DroneState
    {
        // Position & motion
        public Vector3 position;          // World position (meters)
        public Vector3 velocity;          // World velocity (m/s)

        // Orientation
        public Vector3 forward;           // Direction drone is facing
        
        public float mass;                // kg
        
        public float altitudeAboveGround; // meters (computed externally)
        
        public float timestamp;           // Time.time when sampled
        
        public float bottomOffsetMeters;

        public DroneState(
            Vector3 position,
            Vector3 velocity,
            Vector3 forward,
            float mass,
            float altitudeAboveGround,
            float timestamp,
            float bottomOffsetMeters
        )
        {
            this.position = position;
            this.velocity = velocity;
            this.forward = forward;
            this.mass = mass;
            this.altitudeAboveGround = altitudeAboveGround;
            this.timestamp = timestamp;
            this.bottomOffsetMeters = bottomOffsetMeters;
        }
    }
}