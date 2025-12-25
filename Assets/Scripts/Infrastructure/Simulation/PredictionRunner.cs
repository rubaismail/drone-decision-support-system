using Core.Data;
using Core.Prediction;
using Infrastructure.Providers;
using UnityEngine;

namespace Infrastructure.Simulation
{
    public class PredictionRunner: MonoBehaviour
    {
        [Header("Providers")]
        public UnityDroneStateProvider droneStateProvider;
        public UnityGroundHeightProvider groundHeightProvider;
        public UnityWindProvider windProvider;

        [Header("Debug")]
        public GameObject predictedImpactMarker;

        private DroneStateAssembler assembler;
        private FallPredictor predictor;

        void Awake()
        {
            assembler = new DroneStateAssembler(
                droneStateProvider,
                groundHeightProvider
            );

            predictor = new FallPredictor();
        }

        void Update()
        {
            DroneState state = assembler.BuildState();
            Vector3 impact =
                predictor.PredictImpactPoint(
                    state,
                    windProvider.GetWind()
                );
            
            if (predictedImpactMarker != null)
                predictedImpactMarker.transform.position = impact;
        }
    }
}