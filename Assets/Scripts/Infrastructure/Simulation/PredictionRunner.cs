using Core.Data;
using Core.Prediction;
using Infrastructure.Providers;
using Presentation.Visualization;
using UnityEngine;

namespace Infrastructure.Simulation
{
    public class PredictionRunner: MonoBehaviour
    {
        [Header("Providers")]
        public UnityDroneStateProvider droneStateProvider;
        public UnityGroundHeightProvider groundHeightProvider;
        public UnityWindProvider windProvider;
        
        [Header("Visualization")]
        [SerializeField] private ImpactDiskVisualizer impactDiskVisualizer;
        
        public DroneState LatestDroneState { get; private set; }
        public DroneStateAssembler Assembler { get; private set; }

        private FallPredictor predictor;

        public FallPredictionResult LatestPrediction { get; private set; }
        public bool HasDroneState { get; private set; }
        public Vector3 LatestPredictedImpactPointSnapped { get; private set; }

        void Awake()
        {
            Assembler = new DroneStateAssembler(
                droneStateProvider,
                groundHeightProvider
            );

            predictor = new FallPredictor();
        }
        
        public DroneState GetLiveDroneState()
        {
            return Assembler.BuildState();
        }
        
        public FallPredictionResult ComputePredictionNow()
        {
            // DroneState state = Assembler.BuildState();
            // LatestDroneState = state;
            // HasDroneState = true;
            //
            // LatestPrediction = predictor.Predict(state, windProvider.GetWind());
            //
            // if (impactDiskVisualizer != null)
            //     impactDiskVisualizer.RefreshNow();
            //
            // return LatestPrediction;
            
            // 1. Build authoritative drone state (AGL-correct, wind-independent)
            DroneState state = Assembler.BuildState();
            LatestDroneState = state;
            HasDroneState = true;

            // 2. Run physics prediction
            LatestPrediction = predictor.Predict(state, windProvider.GetWind());

            // 3. Compute AUTHORITATIVE predicted impact point (GROUND-SNAPPED)
            //    This MUST match what the visualizer uses
            Vector3 unsnappedImpact = LatestPrediction.impactPointWorld;

            if (groundHeightProvider != null &&
                groundHeightProvider.TryGetGroundHeight(unsnappedImpact, out float groundY))
            {
                LatestPredictedImpactPointSnapped =
                    new Vector3(unsnappedImpact.x, groundY, unsnappedImpact.z);
            }
            else
            {
                // Fallback (should rarely happen)
                LatestPredictedImpactPointSnapped = unsnappedImpact;
            }

            // 4. Update visualization (disk uses snapped point)
            if (impactDiskVisualizer != null)
                impactDiskVisualizer.RefreshNow();

            return LatestPrediction;
        }
        
        public void HideImpactVisualization()
        {
            if (impactDiskVisualizer != null)
                impactDiskVisualizer.Hide();
        }
    }
}