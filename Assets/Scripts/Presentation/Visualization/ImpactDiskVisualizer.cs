using Core.Data;
using Infrastructure.Simulation;
using UnityEngine;

namespace Presentation.Visualization
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ImpactDiskVisualizer : MonoBehaviour
    {
        [Header("Data Source")]
        [SerializeField] private PredictionRunner predictionRunner;

        [Header("Disk Placement")]
        [SerializeField] private float yOffset = 0.15f; // lift slightly above surface to avoid z-fighting

        [Header("Disk Appearance")]
        [SerializeField] private float minRadiusMeters = 1.0f;
        [SerializeField] private float alpha = 0.55f;

        [Header("Risk Colors")]
        [SerializeField] private Color lowRiskColor = new Color(0.1f, 1.0f, 0.1f, 1f);
        [SerializeField] private Color mediumRiskColor = new Color(1.0f, 0.8f, 0.1f, 1f);
        [SerializeField] private Color highRiskColor = new Color(1.0f, 0.15f, 0.15f, 1f);

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private Material _material;
        private static Texture2D _radialTexture;

        void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            // Build a simple quad (flat disk carrier)
            _meshFilter.mesh = BuildQuad();

            // Create a material that can show a transparent texture.
            // Works in Built-in; in URP you may want "Universal Render Pipeline/Unlit"
            Shader shader = Shader.Find("Unlit/Transparent");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Unlit");

            _material = new Material(shader);

            // Generate a radial falloff texture once (shared)
            if (_radialTexture == null)
                _radialTexture = GenerateRadialTexture(256);

            _material.mainTexture = _radialTexture;
            _meshRenderer.material = _material;
        }

        void Update()
        {
            if (predictionRunner == null) return;

            FallPredictionResult p = predictionRunner.LatestPrediction;
            if (!p.isValid)
            {
                _meshRenderer.enabled = false;
                return;
            }

            _meshRenderer.enabled = true;

            // Position at impact point (slightly above ground)
            Vector3 pos = p.impactPointWorld;
            transform.position = new Vector3(pos.x, pos.y + yOffset, pos.z);

            // Scale disk by drift radius
            float r = Mathf.Max(minRadiusMeters, p.horizontalDriftRadius);
            // Quad is 1x1, so scale x/z to diameter
            transform.localScale = new Vector3(r * 2f, 1f, r * 2f);

            // Color by risk (tinted radial texture)
            Color tint = RiskToColor(p.riskLevel);
            tint.a = alpha;
            _material.color = tint;

            // Keep it flat
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        
        public void RefreshNow()
        {
            Update();
        }

        private Color RiskToColor(RiskLevel risk)
        {
            return risk switch
            {
                RiskLevel.Low => lowRiskColor,
                RiskLevel.Medium => mediumRiskColor,
                RiskLevel.High => highRiskColor,
                _ => mediumRiskColor
            };
        }

        private static Mesh BuildQuad()
        {
            Mesh m = new Mesh();

            // XY plane quad; we rotate it later
            m.vertices = new[]
            {
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3( 0.5f, 0f, -0.5f),
                new Vector3(-0.5f, 0f,  0.5f),
                new Vector3( 0.5f, 0f,  0.5f),
            };

            m.uv = new[]
            {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),
            };

            m.triangles = new[]
            {
                0,2,1,
                2,3,1
            };

            m.RecalculateNormals();
            return m;
        }

        private static Texture2D GenerateRadialTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float center = (size - 1) * 0.5f;
            float maxDist = center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) / maxDist; // 0..1

                    // Make a smooth heat falloff (strong center -> transparent edge)
                    float a = Mathf.Clamp01(1f - d);
                    a *= a; // emphasize center

                    // Hard cutoff outside circle
                    if (d > 1f) a = 0f;

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }

            tex.Apply();
            return tex;
        }
    }
}