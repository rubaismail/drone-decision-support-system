namespace Core.Data
{
    public enum SimulationState
    {
        Live,           // Drone wandering, telemetry visible
        Predicted,      // Prediction shown
        Neutralized,    // Drone falling
        ImpactResult,   // Drone crashed, results shown
        Resetting
    }
}