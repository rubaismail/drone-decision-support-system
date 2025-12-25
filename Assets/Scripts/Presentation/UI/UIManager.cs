using CesiumForUnity;
using Infrastructure.Drone;
using Infrastructure.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("=== PANEL STATES ===")]
        public GameObject telemetryPanel;  // Panel_Telemetry
        public GameObject inputPanel;      // Panel_Inputs
        public GameObject outputPanel;     // Panel_Results

        [Header("=== TELEMETRY TEXT (AUTO) ===")]
        public TMP_Text altitudeText;
        public TMP_Text speedText;

        [Header("=== OPERATOR INPUTS ===")]
        public TMP_Dropdown weightDropdown; // kept for later use
        
        public Slider windSpeedSlider;
        public Slider windDirectionSlider;

        public TMP_Text windSpeedText;
        public TMP_Text windDirectionText;

        [Header("=== PREDICTION OUTPUT ===")]
        public TMP_Text impactTimeText;
        public TMP_Text riskLevelText;
        public TMP_Text recommendationText;

        [Header("=== PROVIDERS ===")]
        public UnityWindProvider windProvider;

        [Header("=== DRONE REFERENCES ===")]
        public Transform droneTransform;
        public DroneWanderController droneController;

        private CesiumGlobeAnchor globeAnchor;

        void Awake()
        {
            globeAnchor = droneTransform.GetComponent<CesiumGlobeAnchor>();
        }

        void Start()
        {
            ShowOperatorState();

            // Hook up wind UI
            windSpeedSlider.onValueChanged.AddListener(OnWindSpeedChanged);
            windDirectionSlider.onValueChanged.AddListener(OnWindDirectionChanged);

            // Initialize provider from UI defaults
            OnWindSpeedChanged(windSpeedSlider.value);
            OnWindDirectionChanged(windDirectionSlider.value);
        }

        void Update()
        {
            if (globeAnchor == null)
                return;

            globeAnchor.Sync();
            UpdateTelemetry();
        }

        // =========================
        // PANEL STATE CONTROL
        // =========================

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

        // =========================
        // TELEMETRY
        // =========================

        void UpdateTelemetry()
        {
            if (droneTransform == null || droneController == null || globeAnchor == null)
                return;

            float altitude = (float)globeAnchor.height;
            float speed = droneController.currentSpeed;

            altitudeText.text = $"Altitude: {altitude:F1} m";
            speedText.text = $"Speed: {speed:F1} m/s";
        }

        // =========================
        // WIND UI HANDLERS
        // =========================

        void OnWindSpeedChanged(float value)
        {
            if (windProvider == null)
                return;

            windProvider.SetWindSpeed(value);
            windSpeedText.text = $"Wind Speed: {value:F1} m/s";
        }

        void OnWindDirectionChanged(float value)
        {
            if (windProvider == null)
                return;

            windProvider.SetWindDirection(value);
            windDirectionText.text =
                $"Wind Direction: {value:F0}° ({DegreesToCardinal(value)})";
        }

        string DegreesToCardinal(float degrees)
        {
            if (degrees < 22.5f || degrees >= 337.5f) return "N";
            if (degrees < 67.5f) return "NE";
            if (degrees < 112.5f) return "E";
            if (degrees < 157.5f) return "SE";
            if (degrees < 202.5f) return "S";
            if (degrees < 247.5f) return "SW";
            if (degrees < 292.5f) return "W";
            return "NW";
        }

        // =========================
        // UI BUTTON HANDLERS
        // =========================

        public void OnPredictPressed()
        {
            // Prediction already runs live via PredictionRunner.
            // This button just switches UI state.
            ShowResultsState();
        }

        public void OnBackPressed()
        {
            ShowOperatorState();
        }
    }
}
