using Core.Data;
using UnityEngine;

namespace Infrastructure.Providers
{
    public class UnityWindProvider : MonoBehaviour
    {
        [Header("Operator Wind Input")]
        [Tooltip("Wind speed in meters per second")]
        [SerializeField] private float windSpeed = 0f;

        [Tooltip("Wind direction in degrees (0 = North, 90 = East)")]
        [SerializeField] private float windDirectionDegrees = 0f;

        /// <summary>
        /// Returns wind data in world-space terms.
        /// </summary>
        public WindData GetWind()
        {
            return new WindData
            {
                speed = windSpeed,
                direction = windDirectionDegrees
            };
        }

        /// <summary>
        /// Converts wind data to a world-space velocity vector.
        /// This is NOT part of the interface — utility for prediction and simulation.
        /// </summary>
        public Vector3 GetWindVelocityWorld()
        {
            float radians = windDirectionDegrees * Mathf.Deg2Rad;

            // Assumption:
            // +Z = North
            // +X = East
            Vector3 direction = new Vector3(
                Mathf.Sin(radians), // X (East)
                0f,
                Mathf.Cos(radians)  // Z (North)
            );

            return direction.normalized * windSpeed;
        }
        
        public void SetWindSpeed(float speed)
        {
            windSpeed = Mathf.Max(0f, speed);
        }

        public void SetWindDirection(float degrees)
        {
            windDirectionDegrees = Mathf.Repeat(degrees, 360f);
        }
    }
}
