using UnityEngine;
using Infrastructure.Drone;

namespace Infrastructure.Simulation
{
    public class DroneResetController : MonoBehaviour
    {
        [SerializeField] private Transform drone;
        [SerializeField] private DroneWanderController wander;
        [SerializeField] private Rigidbody rb;

        [SerializeField] private float liftHeight = 10f;
        [SerializeField] private float liftDuration = 1.5f;

        public void ResetDrone(System.Action onComplete)
        {
            StartCoroutine(ResetRoutine(onComplete));
        }

        private System.Collections.IEnumerator ResetRoutine(System.Action onComplete)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            
            var propellers = drone.GetComponentInChildren<DronePropellerController>();
            if (propellers != null)
                propellers.enabled = true;

            Vector3 start = drone.position;
            Vector3 target = start + Vector3.up * liftHeight;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / liftDuration;
                drone.position = Vector3.Lerp(start, target, t);
                yield return null;
            }

            wander.enabled = true;
            
            onComplete?.Invoke();
        }
    }
}