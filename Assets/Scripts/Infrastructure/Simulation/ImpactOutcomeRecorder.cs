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
        
        [SerializeField] private SimulationStateController simulationStateController;
        
        [SerializeField] private AudioSource impactAudio;

        [Header("Optional Filtering")]
        [Tooltip("Optional: restrict impact detection to these layers (e.g. Ground / CesiumTerrain). Leave empty to accept any collision.")]
        [SerializeField] private LayerMask groundLayers;

        private bool _isTracking;
        private bool _impactConfirmed;
        private float _neutralizeTime;
        private Vector3 _predictedImpactPointXZ;

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
            // Lock predicted point at neutralization time
            _predictedImpactPointXZ =
                predictionRunner.LatestPredictedImpactPointSnapped;
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
            Vector3 impactVelocity = collision.relativeVelocity;;

            float actualImpactEnergy =
                0.5f * rb.mass * impactVelocity.sqrMagnitude;
            
            if (impactAudio != null)
            {
                impactAudio.Stop();
                impactAudio.time = 0f;

                impactAudio.volume = Mathf.Clamp01(actualImpactEnergy / 200f);
                impactAudio.Play();
            }
            
            Vector3 impactPoint = transform.position;
            // Snap actual impact to ground using same provider
            if (predictionRunner != null &&
                predictionRunner.groundHeightProvider != null &&
                predictionRunner.groundHeightProvider.TryGetGroundHeight(transform.position, out float groundY))
            {
                impactPoint = new Vector3(transform.position.x, groundY, transform.position.z);
            }
            
            //ector3 predictedPoint = predictionRunner.LatestPredictedImpactPointSnapped;

            //float positionError = Vector3.Distance(predictedPoint, impactPoint);
            Vector2 predictedXZ =
                new Vector2(_predictedImpactPointXZ.x, _predictedImpactPointXZ.z);

            Vector2 actualXZ =
                new Vector2(impactPoint.x, impactPoint.z);

            float positionError =
                Vector2.Distance(predictedXZ, actualXZ);

            Debug.Log(
                $"[IMPACT ANALYSIS CONFIRMED]\n" +
                $"Predicted TTI: {_predicted.timeToImpact:F2}s\n" +
                $"Actual TTI: {actualTimeToImpact:F2}s\n\n" +
                $"Predicted Energy: {_predicted.impactEnergy:F1} J\n" +
                $"Actual Energy: {actualImpactEnergy:F1} J\n\n" +
                $"Impact Error Distance: {positionError:F2} m"
            );
            
            simulationStateController.OnImpactConfirmed(new ImpactComparisonData
            {
                predictedTTI = _predicted.timeToImpact,
                actualTTI = actualTimeToImpact,
                predictedEnergy = _predicted.impactEnergy,
                actualEnergy = actualImpactEnergy,
                positionErrorMeters = positionError
            });
        }
    }
}