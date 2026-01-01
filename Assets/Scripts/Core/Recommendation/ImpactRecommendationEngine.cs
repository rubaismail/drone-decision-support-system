using UnityEngine;
using Core.Data;

namespace Core.Recommendation
{
    /// <summary>
    /// Evaluates neutralization timing options and estimates relative risk reduction.
    /// Uses the SAME impact-energy model as FallPredictor.
    /// </summary>
    public static class ImpactRecommendationEngine
    {
        private const float Gravity = 9.81f;

        // Safety cap: we never claim zero risk
        private const float MaxRiskReduction = 0.95f;

        public static void Evaluate(
            DroneState state,
            WindData wind,
            float timeToImpact,
            float baselineImpactEnergy,
            float energyLowJ,
            float energyHighJ,
            out float bestDelaySeconds,
            out float riskReduction01
        )
        {
            bestDelaySeconds = 0f;
            riskReduction01 = 0f;

            float baselineRisk01 = Mathf.Clamp01(
                Mathf.InverseLerp(energyLowJ, energyHighJ, baselineImpactEnergy)
            );

            if (baselineRisk01 <= 0f)
                return;

            // Wind velocity (same convention as predictor)
            float radians = wind.direction * Mathf.Deg2Rad;
            Vector3 windVelocity = new Vector3(
                Mathf.Sin(radians),
                0f,
                Mathf.Cos(radians)
            ) * wind.speed;

            float maxDelay = Mathf.Min(timeToImpact * 0.7f, 3f);

            for (float delay = 0.1f; delay <= maxDelay; delay += 0.1f)
            {
                float hRemaining =
                    state.altitudeAboveGround +
                    state.velocity.y * delay -
                    0.5f * Gravity * delay * delay;

                if (hRemaining <= 0f)
                    break;

                float vY_at_strike = state.velocity.y - Gravity * delay;

                float vImpactY_post = Mathf.Sqrt(
                    vY_at_strike * vY_at_strike +
                    2f * Gravity * hRemaining
                );

                Vector3 vImpact_post = new Vector3(
                    state.velocity.x + windVelocity.x,
                    -vImpactY_post,
                    state.velocity.z + windVelocity.z
                );

                float postEnergy =
                    0.5f * state.mass * vImpact_post.sqrMagnitude;

                float postRisk01 = Mathf.Clamp01(
                    Mathf.InverseLerp(energyLowJ, energyHighJ, postEnergy)
                );

                float reduction = (baselineRisk01 - postRisk01) / baselineRisk01;

                if (reduction > riskReduction01)
                {
                    riskReduction01 = reduction;
                    bestDelaySeconds = delay;
                }
            }

            riskReduction01 = Mathf.Min(riskReduction01, MaxRiskReduction);
        }
    }
}
