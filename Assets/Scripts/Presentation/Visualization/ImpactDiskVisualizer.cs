using Core.Data;
using UnityEngine;
using Core.Prediction;
using Infrastructure.Simulation;

namespace Presentation.Visualization
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ImpactDiskVisualizer : MonoBehaviour
    {
        [Header("Data Source")] [SerializeField]
        private PredictionRunner predictionRunner;

        [Header("Disk Placement")] [SerializeField]
        private float yOffset = 0.15f;

        [Header("Disk Appearance")] [SerializeField]
        private float minRadiusMeters = 1.0f;

        [SerializeField] private float alpha = 0.55f;

        [Header("Risk Colors")] [SerializeField]
        private Color lowRiskColor = new Color(0.1f, 1.0f, 0.1f, 1f);

        [SerializeField] private Color mediumRiskColor = new Color(1.0f, 0.8f, 0.1f, 1f);
        [SerializeField] private Color highRiskColor = new Color(1.0f, 0.15f, 0.15f, 1f);

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Material _material;
        private bool _initialized;

        private static Texture2D _radialTexture;
        
        void OnEnable()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            if (_meshFilter == null || _meshRenderer == null)
            {
                Debug.LogError("[ImpactDiskVisualizer] Missing Mesh components");
                return;
            }

            if (_meshFilter.mesh == null)
                _meshFilter.mesh = BuildQuad();
            
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                _material = new Material(shader);
                
                Debug.Log("ZTest property exists: " + _material.HasProperty("_ZTest"));

                // FORCE VISIBILITY OVER EVERYTHING
                _material.SetColor("_BaseColor", Color.red);
                _material.SetFloat("_Surface", 1);   // Transparent
                _material.SetFloat("_ZWrite", 0);    // Do NOT write depth
                _material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);

                _material.renderQueue = 5000; // Overlay-level
            }


            // if (_material == null)
            // {
            //     Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            //
            //     if (shader == null)
            //     {
            //         Debug.LogError("[ImpactDiskVisualizer] URP Unlit shader not found");
            //         return;
            //     }
            //
            //     _material = new Material(shader);
            //
            //     // FORCE visibility (URP uses _BaseColor)
            //     _material.SetColor("_BaseColor", new Color(1f, 0f, 0f, 1f));
            //
            //     _material.SetFloat("_Surface", 1);   // Transparent
            //     _material.SetFloat("_Blend", 0);     // Alpha
            //     _material.SetFloat("_ZWrite", 0);
            //     _material.renderQueue = 3000;
            // }

            if (_radialTexture == null)
                _radialTexture = GenerateRadialTexture(256);

            _material.mainTexture = _radialTexture;
            _meshRenderer.material = _material;

            _initialized = true;
        }

        void Update()
        {
            if (!_initialized)
                return;
            
            _meshRenderer.enabled = true;

            transform.position = new Vector3(0f, 10f, 0f);
            transform.localScale = new Vector3(50f, 1f, 50f);
            transform.rotation = Quaternion.identity;

            // ---------- TEMPORARY TEST ----------
            // _meshRenderer.enabled = true;
            //
            // Transform cam = Camera.main.transform;
            // transform.position = cam.position + cam.forward * 15f;
            // transform.position = new Vector3(transform.position.x, 2f, transform.position.z);
            // transform.localScale = new Vector3(20f, 1f, 20f);
            // transform.localScale = new Vector3(15f, 1f, 15f);
            //
            // _material.SetColor("_BaseColor", new Color(1f, 0f, 0f, 1f));
            
            
            

            // ---------- COMMENT THIS BACK IN AFTER TEST ----------
            /*
            if (predictionRunner == null) return;

            FallPredictionResult p = predictionRunner.LatestPrediction;
            if (!p.isValid)
            {
                _meshRenderer.enabled = false;
                return;
            }

            Vector3 pos = p.impactPointWorld;
            transform.position = new Vector3(pos.x, pos.y + yOffset, pos.z);

            float r = Mathf.Max(minRadiusMeters, p.horizontalDriftRadius);
            transform.localScale = new Vector3(r * 2f, 1f, r * 2f);

            Color tint = RiskToColor(p.riskLevel);
            tint.a = alpha;
            _material.color = tint;

            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            */
        }
        
        public void RefreshNow()
        {
            // Force a single evaluation now (useful right after Predict button)
            if (!_initialized)
                OnEnable(); // ensures mesh/material exist if enable order was weird

            // Do one pass of Update logic immediately
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

            m.vertices = new[]
            {
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, -0.5f),
                new Vector3(-0.5f, 0f, 0.5f),
                new Vector3(0.5f, 0f, 0.5f),
            };

            m.uv = new[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
            };

            m.triangles = new[] { 0, 1, 2, 2, 1, 3 };
            m.RecalculateNormals();
            return m;
        }

        private static Texture2D GenerateRadialTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float c = (size - 1) * 0.5f;
            float max = c;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - c;
                float dy = y - c;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / max;

                float a = d > 1f ? 0f : Mathf.Pow(1f - d, 2f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }

            tex.Apply();
            return tex;
        }
    }
}
