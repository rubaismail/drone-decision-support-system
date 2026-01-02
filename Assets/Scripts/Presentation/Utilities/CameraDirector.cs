using UnityEngine;

namespace Presentation.Utilities
{
    public class CameraDirector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private FreeFlyCamera freeFlyCamera;
        [SerializeField] private Transform droneTransform;

        [Header("Impact View")]
        [SerializeField] private Vector3 impactOffset = new Vector3(0, 6, -10);
        [SerializeField] private float moveDuration = 1.5f;

        private Vector3 _startPos;
        private Quaternion _startRot;

        public void FocusOnImpact()
        {
            // freeFlyCamera.enabled = false;
            //
            // _startPos = mainCamera.transform.position;
            // _startRot = mainCamera.transform.rotation;
            //
            // StopAllCoroutines();
            // StartCoroutine(MoveCamera(
            //     droneTransform.position + impactOffset,
            //     Quaternion.LookRotation(droneTransform.position - (_startPos)),
            //     moveDuration
            // ));
            
            freeFlyCamera.enabled = false;

            _startPos = mainCamera.transform.position;
            _startRot = mainCamera.transform.rotation;

            Vector3 targetPos =
                droneTransform.position
                - droneTransform.forward * 12f   // pull back
                + Vector3.up * 6f;               // lift camera

            Quaternion targetRot =
                Quaternion.LookRotation(
                    droneTransform.position - targetPos,
                    Vector3.up
                );

            StopAllCoroutines();
            StartCoroutine(MoveCamera(targetPos, targetRot, moveDuration));
        }

        public void ResetView()
        {
            StopAllCoroutines();
            StartCoroutine(MoveCamera(_startPos, _startRot, moveDuration, () =>
            {
                freeFlyCamera.enabled = true;
            }));
        }

        private System.Collections.IEnumerator MoveCamera(
            Vector3 targetPos,
            Quaternion targetRot,
            float duration,
            System.Action onComplete = null)
        {
            float t = 0f;
            Vector3 startPos = mainCamera.transform.position;
            Quaternion startRot = mainCamera.transform.rotation;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
                mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }

            onComplete?.Invoke();
        }
    }
}