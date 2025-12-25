using Core.Data;
using UnityEngine;

namespace Core.Interfaces
{
   public interface IWindProvider
   {
      WindData GetWind();
      Vector3 GetWindVelocityWorld();
      void SetWindSpeed(float windSpeed);
      void SetWindDirection(float degrees);
   }
}
