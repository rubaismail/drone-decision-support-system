using Core.Interfaces;
using UnityEngine;

namespace Infrastructure.Debugging
{
    public class GroundHeightDebug : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour groundHeightProviderBehaviour;

        private IGroundHeightProvider _provider;
        private float _lastValidGroundY;
        private bool _hasGround = false;

        void Awake()
        {
            _provider = groundHeightProviderBehaviour as IGroundHeightProvider;

            if (_provider == null)
            {
                Debug.LogError(
                    "[GroundHeightDebug] Assigned object does not implement IGroundHeightProvider"
                );
            }
        }

        void Update()
        {
            if (_provider == null) return;

            if (_provider.TryGetGroundHeight(transform.position, out float groundY))
            {
                _lastValidGroundY = groundY;
                _hasGround = true;

                float altitudeAGL = transform.position.y - groundY;
                Debug.Log($"[GroundHeightDebug] Altitude AGL: {altitudeAGL:F2} m");
            }
            else if (_hasGround)
            {
                float altitudeAGL = transform.position.y - _lastValidGroundY;
                Debug.Log($"[GroundHeightDebug] (cached) Altitude AGL: {altitudeAGL:F2} m");
            }
            else
            {
                Debug.LogWarning("[GroundHeightDebug] No ground hit (no cached value)");
            }

            Debug.DrawRay(transform.position, Vector3.down * 200f, Color.red);
        }
    }
}
