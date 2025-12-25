using UnityEngine;

namespace Core.Data
{
    public enum RiskLevel
    {
        Low,
        Medium,
        High
    }

    [System.Serializable]
    public struct FallPredictionResult
    {
        // Core prediction
        public Vector3 impactPointWorld;
        public float timeToImpact;

        // Horizontal uncertainty / drift
        public float horizontalDriftRadius;

        // Energy proxy (Joules)
        public float impactEnergy;

        // Qualitative risk
        public RiskLevel riskLevel;

        // Validity flag (important for edge cases)
        public bool isValid;
    }
}