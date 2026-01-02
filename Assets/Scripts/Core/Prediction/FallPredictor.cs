using Core.Data;
using Core.Recommendation;
using UnityEngine;

namespace Core.Prediction
{
    public class FallPredictor
    {
        private const float Gravity = 9.81f;
        
        // TODO: These should be tuned later with data (or replaced by ML)
        private const float EnergyLowJ = 100f;     // below this ≈ Low
        private const float EnergyHighJ = 800f;   // above this ≈ High
        
            private struct MethodProfile
        {
            public float horizontalVelocityScale;   // scales lateral motion after neutralization
            public float timeScale;                 // scales descent time (e.g., partial thrust loss)
            public float energyScale;               // scales impact energy (e.g., tether capture)
            public float extraUncertaintyMeters;    // adds drift radius uncertainty (e.g., explosive disable)
        }

        private static MethodProfile GetProfile(NeutralizationMethod method)
        {
            // TODO: MVP placeholders — replace later
            switch (method)
            {
                case NeutralizationMethod.MotorCutoff:
                    return new MethodProfile
                    {
                        horizontalVelocityScale = 1f,
                        timeScale = 1f,
                        energyScale = 1f,
                        extraUncertaintyMeters = 0f
                    };

                case NeutralizationMethod.PartialThrustLoss:
                    // “Falls slower” / “some control remains briefly”
                    return new MethodProfile
                    {
                        horizontalVelocityScale = 0.8f,
                        timeScale = 1.25f,
                        energyScale = 1f,
                        extraUncertaintyMeters = 5f
                    };

                case NeutralizationMethod.ExplosiveDisable:
                    // Adds uncertainty due to breakup / impulse
                    return new MethodProfile
                    {
                        horizontalVelocityScale = 1.2f,
                        timeScale = 1f,
                        energyScale = 1f,
                        extraUncertaintyMeters = 25f
                    };

                case NeutralizationMethod.TetheredCapture:
                    // Captured → much lower energy, near-vertical
                    return new MethodProfile
                    {
                        horizontalVelocityScale = 0.1f,
                        timeScale = 1f,
                        energyScale = 0.2f,
                        extraUncertaintyMeters = 2f
                    };

                default:
                    return new MethodProfile
                    {
                        horizontalVelocityScale = 1f,
                        timeScale = 1f,
                        energyScale = 1f,
                        extraUncertaintyMeters = 0f
                    };
            }
        }

        public FallPredictionResult Predict(DroneState state, WindData wind)
        {
            FallPredictionResult result = new FallPredictionResult { isValid = false };

            // --- Vertical motion ---
            float y0 = state.altitudeAboveGround;
            float v0y = state.velocity.y;

            float a = -0.5f * Gravity;
            float b = v0y;
            float c = y0;

            float discriminant = b * b - 4f * a * c;
            if (discriminant < 0f)
                return result;

            float tImpact = (-b - Mathf.Sqrt(discriminant)) / (2f * a);

            if (tImpact <= 0f)
                return result;

            // --- Wind → world-space velocity ---
            float radians = wind.direction * Mathf.Deg2Rad;
            Vector3 windVelocity = new Vector3(
                Mathf.Sin(radians),        // +X = East
                0f,
                Mathf.Cos(radians)         // +Z = North
            ) * wind.speed;

            // --- Horizontal drift ---
            Vector3 horizontalVelocity =
                new Vector3(state.velocity.x, 0f, state.velocity.z) 
                + windVelocity;
            
            // Horizontal-only impact prediction (Y resolved later by ground snap)
            Vector3 impactOffsetXZ = new Vector3(
                horizontalVelocity.x * tImpact,
                0f,
                horizontalVelocity.z * tImpact
            );

            float driftRadius = horizontalVelocity.magnitude * tImpact;
            
            // --- Impact energy (physically grounded) ---
            // Vertical impact speed from kinematics: v^2 = v0^2 + 2gh
            float vImpactY = Mathf.Sqrt(
                state.velocity.y * state.velocity.y +
                2f * Gravity * state.altitudeAboveGround
            );

            // Horizontal velocity at impact (approx constant)
            Vector3 vImpact = new Vector3(
                state.velocity.x + windVelocity.x,
                -vImpactY,
                state.velocity.z + windVelocity.z
            );

            // Kinetic energy at impact
            float impactEnergy = 0.5f * state.mass * vImpact.sqrMagnitude;
            
            // --- Continuous risk (0..1) derived from energy ---
            // Map EnergyLowJ -> 0, EnergyHighJ -> 1
            float risk01 = Mathf.InverseLerp(EnergyLowJ, EnergyHighJ, impactEnergy);
            risk01 = Mathf.Clamp01(risk01);
            
            // --- Qualitative risk derived from risk01 (keeps consistent) ---
            RiskLevel riskLevel =
                risk01 < 0.33f ? RiskLevel.Low :
                risk01 < 0.66f ? RiskLevel.Medium :
                RiskLevel.High;
            
            // --- Recommendation (delegated) ---
            ImpactRecommendationEngine.Evaluate(
                state,
                wind,
                tImpact,
                impactEnergy,
                EnergyLowJ,
                EnergyHighJ,
                out float bestDelay,
                out float riskReduction01
            );
            
            // --- Populate result ---
            
            //a best-guess world point for UI/debug only
            result.impactPointWorld = state.position + impactOffsetXZ;
            
            result.impactOffsetXZ = impactOffsetXZ;
            result.timeToImpact = tImpact;
            result.horizontalDriftRadius = driftRadius;
            result.impactEnergy = impactEnergy;
            result.risk01 = risk01;
            result.riskLevel = riskLevel;
            result.recommendedDelaySeconds = bestDelay;
            result.riskReductionPercent = riskReduction01 * 100f;
            result.isValid = true;
            
            Debug.Log(
                $"[PREDICT] DronePos={state.position} | " +
                $"HorizVel={horizontalVelocity} | tImpact={tImpact:F2}s | " +
                $"Offset={impactOffsetXZ} | ImpactGuess={result.impactPointWorld}"
            );

            return result;
        }
    }
}