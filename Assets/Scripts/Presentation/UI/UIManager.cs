using CesiumForUnity;
using Core.Data;
using Infrastructure.Drone;
using Infrastructure.Providers;
using Infrastructure.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("=== ROOT PANELS ===")]
        public GameObject operatorRootPanel; // Panel_OperatorUI
        
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
        public TMP_Text windSpeedValue;
        
        public Slider windDirectionSlider;
        public TMP_Text windDirectionValue;

        [Header("=== PREDICTION OUTPUT ===")]
        public TMP_Text impactTimeText;
        public TMP_Text riskLevelText;
        public TMP_Text recommendationText;

        [Header("=== PROVIDERS ===")]
        public UnityWindProvider windProvider;
        
        [Header("Prediction")]
        public PredictionRunner predictionRunner;

        [Header("=== DRONE REFERENCES ===")]
        public Transform droneTransform;
        public DroneWanderController droneController;
        
        [Header("Neutralization")]
        [SerializeField] private DroneNeutralizationController neutralizationController;

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
            
            weightDropdown.ClearOptions();
            weightDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Light (250 g)",
                "Medium (1 kg)",
                "Heavy (3.5 kg)"
            });

            weightDropdown.onValueChanged.AddListener(OnWeightChanged);

            // Initialize mass from default dropdown value
            OnWeightChanged(weightDropdown.value);
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
            Debug.Log("[UI] ShowOperatorState");
            telemetryPanel.SetActive(true);
            inputPanel.SetActive(true);
            outputPanel.SetActive(false);
        }

        void ShowResultsState()
        {
            Debug.Log("[UI] ShowResultsState");
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

            float altitude = 0f;

            if (predictionRunner != null)
            {
                DroneState liveState = predictionRunner.GetLiveDroneState();
                altitude = liveState.altitudeAboveGround;
            }
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
            windSpeedValue.text = $"{value:F1} m/s";
        }

        void OnWindDirectionChanged(float value)
        {
            if (windProvider == null)
                return;

            windProvider.SetWindDirection(value);
            windDirectionValue.text =
                $"{value:F0}° ({DegreesToCardinal(value)})";
        }
        
        void OnWeightChanged(int index)
        {
            if (predictionRunner == null || predictionRunner.droneStateProvider == null)
                return;

            float massKg = index switch
            {
                (int)DroneWeightClass.Light  => 0.25f,
                (int)DroneWeightClass.Medium => 1.00f,
                (int)DroneWeightClass.Heavy  => 3.50f,
                _ => 1.00f
            };

            predictionRunner.droneStateProvider.SetMassKg(massKg);

            Debug.Log($"[UI] Drone mass set to {massKg} kg");
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
            Debug.Log("PREDICT BUTTON CLICKED");
            
            var prediction = predictionRunner.ComputePredictionNow();

            if (!prediction.isValid)
            {
                Debug.LogWarning("[UI] Prediction INVALID — staying in Operator state");
                impactTimeText.text = "Time to Impact: --";
                riskLevelText.text = "Risk Level: --";
                recommendationText.text = "Recommendation: Invalid prediction";
                return;
            }

            impactTimeText.text =
                $"Time to Impact: {prediction.timeToImpact:F1} s";

            riskLevelText.text =
                $"Risk Level: {prediction.riskLevel}";

            if (prediction.recommendedDelaySeconds > 0.5f)
            {
                recommendationText.text =
                    $"Delay {prediction.recommendedDelaySeconds:F1}s\n" +
                    $"Relative Risk ↓ {prediction.riskReductionPercent:F0}%";
            }
            else
            {
                recommendationText.text =
                    "Immediate neutralization recommended";
            }

            ShowResultsState();
        }
        
        public void OnNeutralizePressed()
        {
            neutralizationController.NeutralizeNow();
        }

        public void OnBackPressed()
        {
            Debug.Log("[UI] Back pressed");
            predictionRunner.HideImpactVisualization();
            ShowOperatorState();
        }
    }
}

