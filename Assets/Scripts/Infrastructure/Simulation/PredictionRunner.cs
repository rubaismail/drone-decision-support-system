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

        private DroneStateAssembler assembler;
        private FallPredictor predictor;

        public FallPredictionResult LatestPrediction { get; private set; }

        void Awake()
        {
            assembler = new DroneStateAssembler(
                droneStateProvider,
                groundHeightProvider
            );

            predictor = new FallPredictor();
        }
        
        public FallPredictionResult ComputePredictionNow()
        {
            DroneState state = assembler.BuildState();

            LatestPrediction = predictor.Predict(state, windProvider.GetWind());
            
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