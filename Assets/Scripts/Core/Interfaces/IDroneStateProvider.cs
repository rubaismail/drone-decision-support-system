using Core.Data;

namespace Core.Interfaces
{
    public interface IDroneStateProvider
    { 
        DroneState GetCurrentState();
    }
}