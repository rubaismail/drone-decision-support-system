using CesiumForUnity;
using Drone;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DroneUIManager : MonoBehaviour
    {
        [Header("=== PANEL STATES ===")] public GameObject inputPanel; // Panel_InputState
        public GameObject outputPanel; // Panel_OutputState

        [Header("=== TELEMETRY TEXT (AUTO) ===")]
        public TMP_Text altitudeText;

        public TMP_Text speedText;
        public TMP_Text headingText;

        [Header("=== OPERATOR INPUTS ===")] public Slider timeOffsetSlider;
        public TMP_Text timeOffsetValueText;
        public TMP_Dropdown weightDropdown;

        [Header("=== PREDICTION OUTPUT ===")] public TMP_Text impactTimeText;
        public TMP_Text riskLevelText;
        public TMP_Text recommendationText;

        [Header("=== DRONE REFERENCES ===")] public Transform droneTransform;
        public DroneWanderController droneController;
    
        CesiumGlobeAnchor globeAnchor;

        void Awake()
        {
            globeAnchor = droneTransform.GetComponent<CesiumGlobeAnchor>();
        }

        void Start()
        {
            // Initial UI state
            ShowInputState();
        }

        void Update()
        {
            globeAnchor.Sync();
            UpdateTelemetry();
            UpdateInputDisplay();
        }

        public void ShowInputState()
        {
            inputPanel.SetActive(true);
            outputPanel.SetActive(false);
        }

        public void ShowOutputState()
        {
            inputPanel.SetActive(false);
            outputPanel.SetActive(true);
        }

        void UpdateTelemetry()
        {
            if (droneTransform == null || droneController == null || globeAnchor == null)
                return;

            float altitude = (float)globeAnchor.height; 
            float speed = droneController.currentSpeed;
            float heading = droneTransform.eulerAngles.y;

            altitudeText.text = $"Altitude: {altitude:F1} m";
            speedText.text = $"Speed: {speed:F1} m/s";
            headingText.text = $"Heading: {heading:F0}°";
        }

        void UpdateInputDisplay()
        {
            if (timeOffsetSlider != null && timeOffsetValueText != null)
            {
                timeOffsetValueText.text = $"+{timeOffsetSlider.value:F1} s";
            }
        }

        public void OnPredictPressed()
        {
            // Read operator inputs
            float timeOffset = timeOffsetSlider.value;
            float weight = GetDroneMass();

            // TEMP PLACEHOLDER (will be replaced with real prediction) 
            float predictedImpactTime = timeOffset + 2.5f;
            string risk = "MEDIUM";
            string recommendation = "WAIT 1.5 SECONDS";

            // Update output UI
            impactTimeText.text = $"Impact Time: {predictedImpactTime:F1} s";
            riskLevelText.text = $"Risk Level: {risk}";
            recommendationText.text = recommendation;

            ShowOutputState();
        }

        public void OnBackPressed()
        {
            ShowInputState();
        }

        float GetDroneMass()
        {
            switch (weightDropdown.value)
            {
                case 0: return 0.7f; // Light
                case 1: return 2.0f; // Medium
                case 2: return 4.5f; // Heavy
                default: return 2.0f;
            }
        }
    }
}