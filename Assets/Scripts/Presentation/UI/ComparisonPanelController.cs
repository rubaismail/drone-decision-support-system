using Core.Data;
using TMPro;
using UnityEngine;

namespace Presentation.UI
{
    public class ComparisonPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        [SerializeField] private TMP_Text predictedTTI;
        [SerializeField] private TMP_Text actualTTI;
        [SerializeField] private TMP_Text predictedEnergy;
        [SerializeField] private TMP_Text actualEnergy;
        [SerializeField] private TMP_Text errorDistance;

        public void Show(ImpactComparisonData data)
        {
            root.SetActive(true);

            predictedTTI.text = $"{data.predictedTTI:F2}s";
            actualTTI.text = $"{data.actualTTI:F2}s";

            predictedEnergy.text = $"{data.predictedEnergy:F1} J";
            actualEnergy.text = $"{data.actualEnergy:F1} J";

            errorDistance.text = $"{data.positionErrorMeters:F2} m";
        }

        public void Hide()
        {
            root.SetActive(false);
        }
    }
}