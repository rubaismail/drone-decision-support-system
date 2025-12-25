using Infrastructure.Providers;
using UnityEngine;

namespace Infrastructure.Simulation
{
    public class WindForceApplier : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private UnityWindProvider windProvider;

        void FixedUpdate()
        {
            if (rb == null || windProvider == null)
                return;

            Vector3 windVelocity = windProvider.GetWindVelocityWorld();

            rb.AddForce(windVelocity, ForceMode.Acceleration);
        }
    }
}
