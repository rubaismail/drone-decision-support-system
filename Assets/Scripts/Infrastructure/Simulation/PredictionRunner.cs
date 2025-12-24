using CesiumForUnity;
using Core.Data;
using Core.Prediction;
using Infrastructure.Providers;
using UnityEngine;

namespace Infrastructure.Simulation
{
    public class PredictionRunner: MonoBehaviour
    {
        [Header("Providers")]
        [SerializeField] private UnityDroneStateProvider droneStateProvider;
        [SerializeField] private UnityGroundHeightProvider groundHeightProvider;
        
        [Header("Cesium")]
        [SerializeField] private CesiumGeoreference georeference;
    
        [Header("Debug")]
        [SerializeField] private bool drawPath = true;
        [SerializeField] private float pathStep = 0.1f;
    
        private DroneStateAssembler _assembler;
        private BallisticFallPredictor _predictor;
    
        private DroneState _latestState;
        private Vector3[] _cachedPath;
    
        void Awake()
        {
            _assembler = new DroneStateAssembler(
                droneStateProvider,
                groundHeightProvider
            );
    
            _predictor = new BallisticFallPredictor();
        }
    
        void Update()
        {
            _latestState = _assembler.BuildState();
    
            var path = _predictor.PredictFallPath(_latestState, pathStep);
            _cachedPath = path.ToArray();
        }

        void OnDrawGizmos()
        {
            if (_cachedPath == null || _cachedPath.Length < 2)
                return;

            Gizmos.color = Color.yellow;

            for (int i = 1; i < _cachedPath.Length; i++)
            {
                Vector3 a = georeference.transform.TransformPoint(_cachedPath[i - 1]);
                Vector3 b = georeference.transform.TransformPoint(_cachedPath[i]);

                Gizmos.DrawLine(a, b);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(
                georeference.transform.TransformPoint(_cachedPath[^1]),
                2f
            );
        }
    }
}