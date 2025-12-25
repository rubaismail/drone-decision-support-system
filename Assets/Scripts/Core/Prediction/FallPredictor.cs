using Core.Data;
using UnityEngine;

namespace Core.Prediction
{
    public class FallPredictor
    {
        private const float Gravity = 9.81f;

        public FallPredictionResult Predict(
            DroneState state,
            WindData wind
        )
        {
            FallPredictionResult result = new FallPredictionResult
            {
                isValid = false
            };

            // --- Vertical motion ---
            float y0 = state.altitudeAboveGround;
            float v0y = state.velocity.y;

            float a = -0.5f * Gravity;
            float b = v0y;
            float c = y0;

            float discriminant = b * b - 4f * a * c;
            if (discriminant < 0f)
                return result;

            float tImpact =
                (-b - Mathf.Sqrt(discriminant)) / (2f * a);

            if (tImpact <= 0f)
                return result;

            // --- Wind → world-space velocity ---
            float radians = wind.direction * Mathf.Deg2Rad;
            Vector3 windVelocity = new Vector3(
                Mathf.Sin(radians),
                0f,
                Mathf.Cos(radians)
            ) * wind.speed;

            // --- Horizontal drift ---
            Vector3 horizontalVelocity =
                new Vector3(state.velocity.x, 0f, state.velocity.z) +
                windVelocity;

            Vector3 impactPoint =
                state.position +
                horizontalVelocity * tImpact +
                Vector3.down * y0;

            float driftRadius = horizontalVelocity.magnitude * tImpact;

            // --- Impact energy proxy ---
            float impactEnergy =
                0.5f * state.mass *
                (state.velocity.sqrMagnitude + windVelocity.sqrMagnitude);

            // --- Risk classification (simple, defensible MVP) ---
            RiskLevel risk =
                impactEnergy < 50f ? RiskLevel.Low :
                impactEnergy < 200f ? RiskLevel.Medium :
                RiskLevel.High;

            // --- Populate result ---
            result.impactPointWorld = impactPoint;
            result.timeToImpact = tImpact;
            result.horizontalDriftRadius = driftRadius;
            result.impactEnergy = impactEnergy;
            result.riskLevel = risk;
            result.isValid = true;

            return result;
        }
    }
}