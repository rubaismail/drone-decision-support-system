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
        public float timeToImpact;
            // relative offset from drone in Unity world space (XZ only)
        public Vector3 impactOffsetXZ;
        
        public Vector3 impactPointWorld;   // debug only
        
        // Horizontal uncertainty / drift
        public float horizontalDriftRadius;

        // Energy proxy (Joules)
        public float impactEnergy;

        // Qualitative risk (for logic and UI)
        public RiskLevel riskLevel;
        
        // Continuous risk (for visuals, scaling, math)
        public float risk01; // 0 = safe, 1 = catastrophic
        
        public float recommendedDelaySeconds;
        public float riskReductionPercent; // 0–100

        // Validity flag (important for edge cases)
        public bool isValid;
    }
}