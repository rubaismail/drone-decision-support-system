using UnityEngine;

namespace Infrastructure.Drone
{
    public class DroneWanderController : MonoBehaviour
    {
        public float minSpeed = 1f;
        public float maxSpeed = 6f;
        public float acceleration = 2f;

        public float directionChangeInterval = 2f;

        public Transform roamCenter;     // set for the addition arena 
        public float wanderRadius = 40f;  // << drone stays inside this radius

        private Vector3 targetDirection;
        private float targetSpeed;
        public float currentSpeed;

        public DronePropellerController propellers;

        void Start()
        {
            PickNewDirection();
        }

        void Update()
        {
            // Smooth speed transition
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);

            // Movement
            transform.position += targetDirection * (currentSpeed * Time.deltaTime);

            // Change direction occasionally
            directionChangeInterval -= Time.deltaTime;
            if (directionChangeInterval <= 0f)
            {
                directionChangeInterval = Random.Range(2f, 5f);
                PickNewDirection();
            }
        
            KeepWithinRadius();

            // Spin propellers based on velocity
            if (propellers != null)
                propellers.currentSpinMultiplier = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);
        }

        void PickNewDirection()
        {
            targetDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-0.3f, 0.3f),
                Random.Range(-1f, 1f)
            ).normalized;

            targetSpeed = Random.Range(minSpeed, maxSpeed);
        }

        void KeepWithinRadius()
        {
            if (roamCenter == null)
                return;

            Vector3 toCenter = roamCenter.position - transform.position;
            float distance = toCenter.magnitude;

            // If drone is outside allowed radius, steer back toward center
            if (distance > wanderRadius)
            {
                // Strong steering back inward
                targetDirection = toCenter.normalized;

                // Increase speed slightly when returning
                targetSpeed = maxSpeed;
            }
        }
    }
}