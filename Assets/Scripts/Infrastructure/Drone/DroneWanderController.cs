using UnityEngine;

namespace Infrastructure.Drone
{
    public class DroneWanderController : MonoBehaviour
    {
        [SerializeField] private DroneAudioController audioController;
        
        [Header("Altitude Safety")]
        public float minAltitude = 5f; // meters above roamCenter Y
        
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
            audioController?.StartMotor();
        }
        void OnEnable()
        {
            audioController?.StartMotor();
        }

        void Update()
        {
            // Smooth speed transition
            //currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                acceleration * Time.deltaTime
            );

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
            ClampAltitude();

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
            currentSpeed *= Random.Range(0.6f, 1.1f);
        }
        
        void ClampAltitude()
        {
            if (roamCenter == null)
                return;

            float minY = roamCenter.position.y + minAltitude;

            if (transform.position.y < minY)
            {
                Vector3 pos = transform.position;
                pos.y = minY;
                transform.position = pos;

                // Force upward movement
                targetDirection.y = Mathf.Abs(targetDirection.y);
            }
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
                //targetDirection = toCenter.normalized;

                // Increase speed slightly when returning
                //targetSpeed = maxSpeed;
                
                targetDirection = Vector3.Lerp(
                    targetDirection,
                    toCenter.normalized,
                    0.5f
                ).normalized;

                targetSpeed = Mathf.Lerp(targetSpeed, maxSpeed, 0.5f);
                
            }
        }
    }
}