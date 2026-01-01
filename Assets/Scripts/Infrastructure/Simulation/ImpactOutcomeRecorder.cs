using Core.Data;
using UnityEngine;

namespace Infrastructure.Simulation
{
    [RequireComponent(typeof(Rigidbody))]
    public class ImpactOutcomeRecorder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PredictionRunner predictionRunner;
        [SerializeField] private Rigidbody rb;

        [Header("Optional Filtering")]
        [Tooltip("Optional: restrict impact detection to these layers (e.g. Ground / CesiumTerrain). Leave empty to accept any collision.")]
        [SerializeField] private LayerMask groundLayers;

        private bool _isTracking;
        private bool _impactConfirmed;
        private float _neutralizeTime;

        private FallPredictionResult _predicted;

        private void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Called when the drone is neutralized and begins falling.
        /// </summary>
        public void OnNeutralized()
        {
            Debug.Log("[IMPACT RECORDER] OnNeutralized called");
            
            if (predictionRunner == null)
                return;

            _predicted = predictionRunner.LatestPrediction;
            _neutralizeTime = Time.time;

            _isTracking = true;
            _impactConfirmed = false;
        }

        /// <summary>
        /// Authoritative impact trigger.
        /// Uses collision events only — no inferred thresholds.
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            // debugging
            Debug.Log($"[IMPACT RECORDER] isTracking={_isTracking}");

            if (!_isTracking)
                return;

            // Latch immediately
            _isTracking = false;
            _impactConfirmed = true;

            float actualTimeToImpact = Time.time - _neutralizeTime;
            Vector3 impactVelocity = rb.linearVelocity;

            float actualImpactEnergy =
                0.5f * rb.mass * impactVelocity.sqrMagnitude;

            Vector3 impactPoint =
                collision.contactCount > 0
                    ? collision.contacts[0].point
                    : transform.position;

            float positionError =
                Vector3.Distance(_predicted.impactPointWorld, impactPoint);

            Debug.Log(
                $"[IMPACT ANALYSIS CONFIRMED]\n" +
                $"Predicted TTI: {_predicted.timeToImpact:F2}s\n" +
                $"Actual TTI: {actualTimeToImpact:F2}s\n\n" +
                $"Predicted Energy: {_predicted.impactEnergy:F1} J\n" +
                $"Actual Energy: {actualImpactEnergy:F1} J\n\n" +
                $"Impact Error Distance: {positionError:F2} m"
            );
            // TODO:
            // Notify SimulationStateController here
            // simulationStateController.OnImpactConfirmed(...)
        }
    }
}