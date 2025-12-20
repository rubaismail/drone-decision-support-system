using UnityEngine;

namespace Drone
{
    public class DronePropellerController : MonoBehaviour
    {
        public Transform propellerFL;
        public Transform propellerFR;
        public Transform propellerBL;
        public Transform propellerBR;

        public float baseSpinSpeed = 2500f;
    
        [HideInInspector]
        public float currentSpinMultiplier = 0f;  // 0 = idle, 1 = max speed

        void Update()
        {
            //float dynamicSpeed = baseSpinSpeed * (0.4f + currentSpinMultiplier * 1.6f);  
            //float speed = dynamicSpeed * Time.deltaTime;
        
            float speed = Mathf.Lerp(baseSpinSpeed * 0.8f, baseSpinSpeed * 3.2f, currentSpinMultiplier);

            // Front Left - CW
            if (propellerFL)
                propellerFL.Rotate(0, 0, speed, Space.Self);

            // Front Right - CCW
            if (propellerFR)
                propellerFR.Rotate(0, 0, -speed, Space.Self);

            // Back Left - CCW
            if (propellerBL)
                propellerBL.Rotate(0, 0, -speed, Space.Self);

            // Back Right - CW
            if (propellerBR)
                propellerBR.Rotate(0, 0, speed, Space.Self);
        }
    }
}
