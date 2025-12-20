using CesiumForUnity;
using Drone;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DroneUIManager : MonoBehaviour
    {
        [Header("=== PANEL STATES ===")] 
        public GameObject telemetryPanel;  //Panel_Telemetry
        public GameObject inputPanel;      // Panel_Inputs
        public GameObject outputPanel;     // Panel_Results

        [Header("=== TELEMETRY TEXT (AUTO) ===")]
        public TMP_Text altitudeText;
        public TMP_Text speedText;
        //public TMP_Text headingText;

        [Header("=== OPERATOR INPUTS ===")] 
        //public Slider timeOffsetSlider;
        //public TMP_Text timeOffsetValueText;
        public TMP_Dropdown weightDropdown;

        [Header("=== PREDICTION OUTPUT ===")] 
        public TMP_Text impactTimeText;
        public TMP_Text riskLevelText;
        public TMP_Text recommendationText;

        [Header("=== DRONE REFERENCES ===")] 
        public Transform droneTransform;
        public DroneWanderController droneController;
    
        CesiumGlobeAnchor globeAnchor;

        void Awake()
        {
            globeAnchor = droneTransform.GetComponent<CesiumGlobeAnchor>();
        }

        void Start()
        {
            // Initial UI state
            ShowOperatorState();
        }

        void Update()
        {
            if (globeAnchor == null) return;
            
            globeAnchor.Sync();
            UpdateTelemetry();
        }
        
        void ShowOperatorState()
        {
            telemetryPanel.SetActive(true);
            inputPanel.SetActive(true);
            outputPanel.SetActive(false);
        }

        void ShowResultsState()
        {
            telemetryPanel.SetActive(false);
            inputPanel.SetActive(false);
            outputPanel.SetActive(true);
        }
        

        void UpdateTelemetry()
        {
            if (droneTransform == null || droneController == null || globeAnchor == null)
                return;

            float altitude = (float)globeAnchor.height; 
            float speed = droneController.currentSpeed;

            altitudeText.text = $"Altitude: {altitude:F1} m";
            speedText.text = $"Speed: {speed:F1} m/s";
        }

        public void OnPredictPressed()
        {
            float altitude = (float)globeAnchor.height;
            float impactTime = PredictImpactTime(altitude);

            string risk = impactTime < 2f ? "HIGH" : "MEDIUM";
            string recommendation =
                impactTime < 2f ? "IMMEDIATE STRIKE" : "WAIT";

            impactTimeText.text = $"Impact Time: {impactTime:F2} s";
            riskLevelText.text = $"Risk Level: {risk}";
            recommendationText.text = recommendation;

            ShowResultsState();
        }

        public void OnBackPressed()
        {
            ShowOperatorState();
        }

        float PredictImpactTime(float altitudeMeters)
        {
            const float gravity = 9.81f;

            if (altitudeMeters <= 0f)
                return 0f;

            return Mathf.Sqrt((2f * altitudeMeters) / gravity);
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