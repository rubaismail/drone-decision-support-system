using UnityEngine;
using Core.Data;
using Infrastructure.Providers;

namespace Infrastructure.Simulation
{
    [RequireComponent(typeof(Rigidbody))]
    public class ImpactOutcomeRecorder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PredictionRunner predictionRunner;
        [SerializeField] private Rigidbody rb;
        
        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float groundProbeDistance = 2.0f;
        [SerializeField] private float minImpactSpeed = 0.1f;
        [SerializeField] private UnityGroundHeightProvider groundHeightProvider;
        private float _lastAltitudeAGL = float.PositiveInfinity;

        private bool _isTracking;
        private float _neutralizeTime;

        private FallPredictionResult _predicted;

        void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();
        }

        public void OnNeutralized()
        {
            if (predictionRunner == null)
                return;

            _predicted = predictionRunner.LatestPrediction;
            _neutralizeTime = Time.time;
            _isTracking = true;
        }

        void OnCollisionEnter(Collision collision)   // fall-back, but most likely won't work for cesium meshes
        {
            if (!_isTracking)
                return;
            
            int otherLayer = collision.gameObject.layer;
            if ((groundLayers.value & (1 << otherLayer)) == 0)
                return;

            _isTracking = false;

            float actualTimeToImpact = Time.time - _neutralizeTime;
            Vector3 actualImpactPoint = collision.contacts[0].point;
            Vector3 impactVelocity = rb.linearVelocity;

            float actualImpactEnergy =
                0.5f * rb.mass * impactVelocity.sqrMagnitude;

            float positionError =
                Vector3.Distance(
                    _predicted.impactPointWorld,
                    actualImpactPoint
                );

            Debug.Log(
                $"[IMPACT ANALYSIS]\n" +
                $"Predicted TTI: {_predicted.timeToImpact:F2}s\n" +
                $"Actual TTI: {actualTimeToImpact:F2}s\n" +
                $"Time Error: {(actualTimeToImpact - _predicted.timeToImpact):F2}s\n\n" +
                $"Predicted Energy: {_predicted.impactEnergy:F1} J\n" +
                $"Actual Energy: {actualImpactEnergy:F1} J\n\n" +
                $"Impact Error Distance: {positionError:F2} m"
            );
        }
        
        void FixedUpdate()
        {
            if (!_isTracking || groundHeightProvider == null)
                return;

            if (!groundHeightProvider.TryGetGroundHeight(transform.position, out float groundY))
                return;

            float altitudeAGL = transform.position.y - groundY;

            // Consider impact when we're very close to ground
            // Detect ground crossing instead of exact contact
            if (_lastAltitudeAGL > 0f && altitudeAGL <= 0f)
            {
                RegisterImpact(new Vector3(
                    transform.position.x,
                    groundY,
                    transform.position.z
                ));
            }

            _lastAltitudeAGL = altitudeAGL;
        }
        
        private void RegisterImpact(Vector3 impactPoint)
        {
            _isTracking = false;

            float actualTimeToImpact = Time.time - _neutralizeTime;
            Vector3 impactVelocity = rb.linearVelocity;

            float actualImpactEnergy =
                0.5f * rb.mass * impactVelocity.sqrMagnitude;

            float positionError =
                Vector3.Distance(
                    _predicted.impactPointWorld,
                    impactPoint
                );

            Debug.Log(
                $"[IMPACT ANALYSIS]\n" +
                $"Predicted TTI: {_predicted.timeToImpact:F2}s\n" +
                $"Actual TTI: {actualTimeToImpact:F2}s\n" +
                $"Time Error: {(actualTimeToImpact - _predicted.timeToImpact):F2}s\n\n" +
                $"Predicted Energy: {_predicted.impactEnergy:F1} J\n" +
                $"Actual Energy: {actualImpactEnergy:F1} J\n\n" +
                $"Impact Error Distance: {positionError:F2} m"
            );
        }
    }
}
