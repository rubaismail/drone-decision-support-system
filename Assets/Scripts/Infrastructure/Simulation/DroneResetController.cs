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
        [SerializeField] private AudioSource liftAudio;

        [SerializeField] private float liftHeight = 10f;
        [SerializeField] private float liftDuration = 3.5f;

        public void ResetDrone(System.Action onComplete)
        {
            StartCoroutine(ResetRoutine(onComplete));
        }

        private System.Collections.IEnumerator ResetRoutine(System.Action onComplete)
        {
            // Disable drone wander controller
            wander.enabled = false;

            // Disable physics
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
            
            if (liftAudio != null && !liftAudio.isPlaying)
                liftAudio.Play();

            // Compute reset altitude
            Vector3 target =
                wander.roamCenter.position +
                Vector3.up * (wander.minAltitude + liftHeight);

            Vector3 startPos = drone.position;
            Quaternion startRot = drone.rotation;
            Quaternion targetRot = Quaternion.identity;
            
            var propellers = drone.GetComponentInChildren<DronePropellerController>();
            if (propellers != null)
                propellers.enabled = true;
             
            // Lift + reorient
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / liftDuration;
                drone.position = Vector3.Lerp(startPos, target, t);
                drone.rotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }

            // Snap to safe state
            drone.position = target;
            drone.rotation = targetRot;
            
            wander.SendMessage("PickNewDirection", SendMessageOptions.DontRequireReceiver);

            // Re-enable wander
            wander.enabled = true;
            neutralizationController.RestoreFlight();
            
            // if (liftAudio != null)
            //     liftAudio.Stop();

            onComplete?.Invoke();
        }
    }

}