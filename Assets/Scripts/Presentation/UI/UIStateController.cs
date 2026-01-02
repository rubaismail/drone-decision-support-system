using Core.Data;
using UnityEngine;

namespace Presentation.UI
{
    public class UIStateController : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;
        [SerializeField] private ComparisonPanelController comparisonPanel;

        // =========================
        // OPERATOR / LIVE
        // =========================

        public void ShowOperatorUI()
        {
            uiManager.SendMessage("ShowOperatorState");
            if (comparisonPanel != null)
                comparisonPanel.Hide();
        }

        public void HideOperatorUI()
        {
            uiManager.telemetryPanel.SetActive(false);
            uiManager.inputPanel.SetActive(false);
        }

        // =========================
        // RESULTS (IMPACT ANALYSIS)
        // =========================

        public void ShowImpactResults(ImpactComparisonData data)
        {
            // Hide normal operator UI
            HideOperatorUI();

            // Show your existing output panel if you want
            uiManager.outputPanel.SetActive(true);

            // Populate the NEW comparison panel
            if (comparisonPanel != null)
                comparisonPanel.Show(data);
        }

        public void HideImpactResults()
        {
            if (comparisonPanel != null)
                comparisonPanel.Hide();
        }
    }
}