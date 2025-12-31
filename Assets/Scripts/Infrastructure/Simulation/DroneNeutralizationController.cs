using Core.Data;
using Infrastructure.Providers;
using UnityEngine;

namespace Infrastructure.Simulation
{
    [RequireComponent(typeof(Rigidbody))]
    public class DroneNeutralizationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnityDroneStateProvider droneStateProvider;
        [SerializeField] private UnityWindProvider windProvider;

        [Header("Flight Control Scripts (to disable on neutralization)")]
        [SerializeField] private MonoBehaviour[] flightControllers;

        private Rigidbody _rb;
        private bool _isNeutralized = false;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            // Ensure drone starts in controlled mode
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        /// <summary>
        /// Called by UI manager when neutralization is triggered.
        /// </summary>
        public void NeutralizeNow()
        {
            if (_isNeutralized)
                return;

            _isNeutralized = true;

            // 1 - Disable flight / AI / movement scripts
            foreach (var controller in flightControllers)
            {
                if (controller != null)
                    controller.enabled = false;
            }

            // 2 - Capture current physical state
            DroneState state = droneStateProvider.GetCurrentState();
            Vector3 windVelocity = windProvider.GetWindVelocityWorld();

            Vector3 initialVelocity =
                state.velocity + windVelocity;

            // 3 - Enable physics
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.linearVelocity = initialVelocity;

            Debug.Log(
                $"[NEUTRALIZE] Drone neutralized.\n" +
                $"Initial velocity = {initialVelocity}"
            );
        }

        public bool IsNeutralized => _isNeutralized;
    }
}
