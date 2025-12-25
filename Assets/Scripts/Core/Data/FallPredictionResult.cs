using System.Numerics;

namespace Core.Data
{
    public struct FallPredictionResult
    {
        public Vector3 PredictedImpactPointWorld;
        public float TimeToImpact;
        public float HorizontalDriftDistance;
        public float ImpactEnergy;
        //RiskLevel riskLevel;
    }
}