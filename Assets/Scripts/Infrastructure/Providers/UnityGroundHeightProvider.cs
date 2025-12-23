using Core.Interfaces;
using UnityEngine;

namespace Infrastructure.Providers
{
    public class UnityGroundHeightProvider : MonoBehaviour, IGroundHeightProvider
    {
        [Header("Raycast Settings")]
        [SerializeField] private float raycastDistance = 20000f;
        [SerializeField] private LayerMask groundLayers = ~0; // default: everything

        public bool TryGetGroundHeight(Vector3 worldPosition, out float groundHeight)
        {
            Vector3 rayOrigin = worldPosition + Vector3.up * 5f;
            Ray ray = new Ray(rayOrigin, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundLayers))
            {
                groundHeight = hit.point.y;
                return true;
            }

            groundHeight = 0f;
            return false;
        }
    }
}
