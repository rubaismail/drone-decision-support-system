using System.Collections.Generic;
using UnityEngine;
using Infrastructure.Drone;

namespace Infrastructure.Simulation
{
    public class DroneResetController : MonoBehaviour
    {
        [SerializeField] private Transform drone;
        [SerializeField] private DroneWanderController wander;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private DroneNeutralizationController neutralizationController;

        [SerializeField] private float liftHeight = 10f;
        [SerializeField] private float liftDuration = 2.6f;

        public void ResetDrone(System.Action onComplete)
        {
            StartCoroutine(ResetRoutine(onComplete));
        }

        //private System.Collections.IEnumerator ResetRoutine(System.Action onComplete)
        //{
        // rb.isKinematic = true;
        // rb.useGravity = false;
        //
        // var propellers = drone.GetComponentInChildren<DronePropellerController>();
        // if (propellers != null)
        //     propellers.enabled = true;
        //
        // Vector3 start = drone.position;
        // Vector3 target = start + Vector3.up * liftHeight;
        //
        // float t = 0f;
        // while (t < 1f)
        // {
        //     t += Time.deltaTime / liftDuration;
        //     drone.position = Vector3.Lerp(start, target, t);
        //     yield return null;
        // }
        //
        // wander.enabled = true;
        //
        // onComplete?.Invoke();
        // }

        private System.Collections.IEnumerator ResetRoutine(System.Action onComplete)
        {
            // 1. Disable wander so it can't fight us
            wander.enabled = false;

            // 2. Disable physics
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;

            // 3. Compute a VALID reset altitude
            Vector3 target =
                wander.roamCenter.position +
                Vector3.up * (wander.minAltitude + liftHeight);

            Vector3 startPos = drone.position;
            Quaternion startRot = drone.rotation;
            Quaternion targetRot = Quaternion.identity;
            
            var propellers = drone.GetComponentInChildren<DronePropellerController>();
            if (propellers != null)
                propellers.enabled = true;
             
            // 4. Lift + reorient
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / liftDuration;
                drone.position = Vector3.Lerp(startPos, target, t);
                drone.rotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }

            // 5. Snap to safe state
            drone.position = target;
            drone.rotation = targetRot;
            
            wander.SendMessage("PickNewDirection", SendMessageOptions.DontRequireReceiver);

            // 6. Re-enable wander AFTER reset is complete
            wander.enabled = true;
            neutralizationController.RestoreFlight();

            onComplete?.Invoke();
        }
    }

}