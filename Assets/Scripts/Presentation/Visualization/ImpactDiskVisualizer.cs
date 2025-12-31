using System;
using System.Reflection;
using Core.Data;
using Infrastructure.Simulation;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Presentation.Visualization
{
    public class ImpactDiskVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PredictionRunner predictionRunner;
        [SerializeField] private DecalProjector decalProjector;

        [Header("Decal Material (Shader Graph)")]
        [Tooltip("Material asset that uses your Decal Shader Graph with _HeatmapTex, _TintColor, _Opacity.")]
        [SerializeField] private Material decalMaterial;

        [Header("Sizing")]
        [Tooltip("Extra padding added to the predicted drift radius (meters).")]
        [SerializeField] private float radiusPadding = 0f;

        [Tooltip("Clamps the radius so a bad value doesn't create a gigantic decal (meters).")]
        [SerializeField] private float maxRadius = 4000f;

        [Tooltip("Minimum radius so it is always visible (meters).")]
        [SerializeField] private float minRadiusMeters = 15f;

        [Tooltip("Projection depth (meters). Keep big enough for uneven Cesium terrain.")]
        [SerializeField] private float projectionDepth = 500f;

        [Tooltip("Lift the projector slightly above the terrain to avoid clipping issues.")]
        [SerializeField] private float heightOffset = 5f;

        [Header("Orientation")]
        [Tooltip("If enabled, forces the decal projector to aim straight down.")]
        [SerializeField] private bool forceDownRotation = true;

        [Header("Risk → Visual Mapping")]
        [Tooltip("Opacity at risk01 = 0.")]
        [Range(0f, 1f)]
        [SerializeField] private float minOpacity = 0.15f;

        [Tooltip("Opacity at risk01 = 1.")]
        [Range(0f, 1f)]
        [SerializeField] private float maxOpacity = 0.9f;

        [Tooltip("Risk-to-color gradient (0 = low, 1 = high).")]
        [SerializeField] private Gradient riskGradient;

        [SerializeField] private LayerMask groundLayerMask = ~0; // default: everything

        // --- MaterialPropertyBlock plumbing ---
        private MaterialPropertyBlock _mpb;
        private Action<MaterialPropertyBlock> _applyBlockToProjector; // reflection-safe setter

        private static readonly int TintColorId = Shader.PropertyToID("_TintColor");
        private static readonly int OpacityId = Shader.PropertyToID("_Opacity");
        
        private bool _hasPrediction;
        private FallPredictionResult _last;

        private void Awake()
        {
            if (decalProjector == null)
                decalProjector = GetComponent<DecalProjector>();

            // Ensure projector has a material assigned (asset material, NOT instanced).
            if (decalProjector != null && decalMaterial != null)
                decalProjector.material = decalMaterial;

            // Create MPB + find a way to apply it to DecalProjector (API differs by Unity/URP versions).
            _mpb = new MaterialPropertyBlock();
            _applyBlockToProjector = BuildProjectorPropertyBlockSetter(decalProjector);

            // Default gradient if empty.
            if (riskGradient == null || riskGradient.colorKeys == null || riskGradient.colorKeys.Length == 0)
            {
                riskGradient = new Gradient();
                var colors = new GradientColorKey[3];
                colors[0] = new GradientColorKey(Color.green, 0f);
                colors[1] = new GradientColorKey(Color.yellow, 0.5f);
                colors[2] = new GradientColorKey(Color.red, 1f);

                var alphas = new GradientAlphaKey[2];
                alphas[0] = new GradientAlphaKey(1f, 0f);
                alphas[1] = new GradientAlphaKey(1f, 1f);

                riskGradient.SetKeys(colors, alphas);
            }

            // Start hidden until a valid prediction is shown.
            SetVisible(false);
        }

        /// <summary>
        /// Called by PredictionRunner after ComputePredictionNow().
        /// Pulls LatestPrediction and updates the decal projector.
        /// </summary>
        public void RefreshNow()
        {
            if (predictionRunner == null || decalProjector == null)
                return;

            FallPredictionResult p = predictionRunner.LatestPrediction;
            if (!p.isValid)
            {
                SetVisible(false);
                return;
            }
            
            _last = p;
            _hasPrediction = true;

            // --- Position & terrain snap ---
            Transform droneTf = predictionRunner.droneStateProvider.DroneTransform;

            // predicted impact (NO brute offset here)
            Vector3 worldImpact = droneTf.position + p.impactOffsetXZ;

            // Raycast straight down to terrain
            Vector3 rayStart = worldImpact + Vector3.up * 200f;
            Vector3 finalPos = worldImpact;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 5000f, groundLayerMask))
                finalPos = hit.point;

            transform.position = finalPos + Vector3.up * heightOffset;

            if (forceDownRotation)
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            
            // --- Size ---
            float radius = Mathf.Clamp(
                p.horizontalDriftRadius + radiusPadding,
                minRadiusMeters,
                maxRadius
            );
            float diameter = radius * 2f;
            decalProjector.size = new Vector3(diameter, diameter, projectionDepth);

            // --- Risk -> Tint/Opacity (via MPB, NOT material.SetX) ---
            float risk01 = Mathf.Clamp01(p.risk01);
            Color tint = riskGradient.Evaluate(risk01);
            float opacity = Mathf.Lerp(minOpacity, maxOpacity, risk01);

            ApplyVisuals(tint, opacity);
            
            SetVisible(true);
        }

        /// <summary>
        /// Hides the decal (called from Back button).
        /// </summary>
        public void Hide()
        {
            SetVisible(false);
        }

        private void ApplyVisuals(Color tintColor, float opacity)
        {
            if (_mpb == null)
                return;

            _mpb.Clear();
            _mpb.SetColor(TintColorId, tintColor);
            _mpb.SetFloat(OpacityId, opacity);
            
            _applyBlockToProjector?.Invoke(_mpb);
        }

        private void SetVisible(bool visible)
        {
            if (decalProjector != null)
                decalProjector.enabled = visible;
        }

        /// <summary>
        /// URP DecalProjector property-block API differs across versions.
        /// This finds a compatible method/property at runtime so your project compiles + works.
        /// </summary>
        private static Action<MaterialPropertyBlock> BuildProjectorPropertyBlockSetter(DecalProjector projector)
        {
            if (projector == null)
                return null;

            Type t = projector.GetType();

            // 1) Try method: SetPropertyBlock(MaterialPropertyBlock)
            MethodInfo mSet = t.GetMethod("SetPropertyBlock", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, new[] { typeof(MaterialPropertyBlock) }, null);
            if (mSet != null)
            {
                return (block) =>
                {
                    try { mSet.Invoke(projector, new object[] { block }); }
                    catch { /* swallow */ }
                };
            }

            // 2) Try property: materialPropertyBlock { get; set; }
            PropertyInfo pBlock = t.GetProperty("materialPropertyBlock", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (pBlock != null && pBlock.CanWrite)
            {
                return (block) =>
                {
                    try { pBlock.SetValue(projector, block); }
                    catch { /* swallow */ }
                };
            }

            // 3) Try field: m_MaterialPropertyBlock or similar (last resort)
            FieldInfo fBlock = t.GetField("m_MaterialPropertyBlock", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fBlock != null)
            {
                return (block) =>
                {
                    try { fBlock.SetValue(projector, block); }
                    catch { /* swallow */ }
                };
            }

            // If nothing exists, we can't apply MPB on this version.
            // (In that case decals will usually revert to default/white.)
            Debug.LogWarning("[HEATMAP] DecalProjector does not expose a MaterialPropertyBlock API in this Unity/URP build.");
            return null;
        }
    }
}
