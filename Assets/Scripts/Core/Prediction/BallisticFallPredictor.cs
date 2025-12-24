using System.Collections.Generic;
using Core.Data;
using UnityEngine;

namespace Core.Prediction
{
    public class BallisticFallPredictor
    {
        private const float Gravity = 9.81f;

        public float PredictTimeToImpact(DroneState state)
        {
            float y0 = state.altitudeAboveGround;
            float v0 = state.velocity.y;

            // Solve: y0 + v0 t - 1/2 g t^2 = 0
            float a = -0.5f * Gravity;
            float b = v0;
            float c = y0;

            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0f)
                return float.PositiveInfinity;

            float sqrt = Mathf.Sqrt(discriminant);

            float t1 = (-b + sqrt) / (2 * a);
            float t2 = (-b - sqrt) / (2 * a);

            float t = Mathf.Max(t1, t2);

            return t > 0 ? t : float.PositiveInfinity;
        }

        public List<Vector3> PredictFallPath(
            DroneState state,
            float timeStep = 0.1f
        )
        {
            var path = new List<Vector3>();

            float timeToImpact = PredictTimeToImpact(state);
            if (float.IsInfinity(timeToImpact))
                return path;

            for (float t = 0; t <= timeToImpact; t += timeStep)
            {
                Vector3 position =
                    state.position +
                    state.velocity * t +
                    Vector3.down * (0.5f * Gravity * t * t);

                path.Add(position);
            }

            return path;
        }
    }
}