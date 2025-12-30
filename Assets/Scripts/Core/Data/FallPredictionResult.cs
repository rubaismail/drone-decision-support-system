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

        // Qualitative risk (for logic and UI)
        public RiskLevel riskLevel;
        
        // Continuous risk (for visuals, scaling, math)
        public float risk01; // 0 = safe, 1 = catastrophic

        // Validity flag (important for edge cases)
        public bool isValid;
    }
}