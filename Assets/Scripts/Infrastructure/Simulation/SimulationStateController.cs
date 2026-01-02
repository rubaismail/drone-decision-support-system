using Core.Data;
using Presentation.UI;
using Presentation.Utilities;
using UnityEngine;

namespace Infrastructure.Simulation
{
    public class SimulationStateController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraDirector cameraDirector;
        [SerializeField] private UIStateController uiStateController;
        [SerializeField] private DroneResetController resetController;

        public SimulationState CurrentState { get; private set; } = SimulationState.Live;

        public void OnNeutralized()
        {
            CurrentState = SimulationState.Neutralized;
            uiStateController.HideOperatorUI();
        }

        public void OnImpactConfirmed(ImpactComparisonData data)
        {
            CurrentState = SimulationState.ImpactResult;

            cameraDirector.FocusOnImpact();
            uiStateController.ShowImpactResults(data);
        }

        public void ResetSimulation()
        {
            CurrentState = SimulationState.Resetting;

            uiStateController.HideImpactResults();
            cameraDirector.ResetView();
            resetController.ResetDrone(() =>
            {
                CurrentState = SimulationState.Live;
                uiStateController.ShowOperatorUI();
            });
        }
    }
}