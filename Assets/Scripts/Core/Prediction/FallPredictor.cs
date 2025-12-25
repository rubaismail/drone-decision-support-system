using System.Collections.Generic;
using Core.Data;
using UnityEngine;

namespace Core.Prediction
{
    public class FallPredictor
    {
        private const float Gravity = 9.81f;

        public Vector3 PredictImpactPoint(
            DroneState state,
            WindData wind
        )
        {
            float y0 = state.altitudeAboveGround;
            float v0y = state.velocity.y;

            // Solve: y0 + v0y * t - 0.5 * g * t^2 = 0
            float a = -0.5f * Gravity;
            float b = v0y;
            float c = y0;

            float discriminant = b * b - 4f * a * c;
            if (discriminant < 0f)
                return state.position;

            float tImpact =
                (-b - Mathf.Sqrt(discriminant)) / (2f * a);

            if (tImpact <= 0f)
                return state.position;

            // Convert wind to world-space velocity
            float radians = wind.direction * Mathf.Deg2Rad;
            Vector3 windVelocity = new Vector3(
                Mathf.Sin(radians),
                0f,
                Mathf.Cos(radians)
            ) * wind.speed;

            Vector3 horizontalVelocity =
                new Vector3(state.velocity.x, 0f, state.velocity.z) +
                windVelocity;

            Vector3 impactPoint =
                state.position +
                horizontalVelocity * tImpact +
                Vector3.down * y0;

            return impactPoint;
        }
    }
}