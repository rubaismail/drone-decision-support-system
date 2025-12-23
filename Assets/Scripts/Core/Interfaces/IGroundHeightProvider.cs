using UnityEngine;
namespace Core.Interfaces
{
    public interface IGroundHeightProvider
    {
        /// <summary>
        /// Returns ground height (world Y) directly below the given position.
        /// </summary>
        bool TryGetGroundHeight(Vector3 worldPosition, out float groundHeight);
        
    }
}