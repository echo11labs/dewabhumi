using System;
using UnityEngine;

namespace Dewabhumi.Environment
{
    /// <summary>
    /// Master procedural terrain generator modeling the ancient Kathmandu Valley basin.
    /// Creates a natural alluvial settlement plateau (Founder's Tar), scenic meandering Bagmati River,
    /// crisp stepped agricultural terraces (Khet & Bari), and towering Himalayan foothill ridges.
    /// Strictly complies with docs/world/KATHMANDU_VALLEY.md and docs/art_and_ui/ART_DIRECTION.md.
    /// </summary>
    [ExecuteAlways]
    public class KathmanduTerrainGenerator : MonoBehaviour
    {
        [Header("Terrain Dimensions")]
        [SerializeField] private int _width = 512;
        [SerializeField] private int _length = 512;
        [SerializeField] private float _maxElevation = 55f; // Max ridge height in metres
        [SerializeField] private int _heightmapResolution = 513;

        [Header("Founder's Tar Plateau (Settlement Tableland)")]
        [SerializeField] private Vector2 _plateauCenter = new Vector2(256f, 230f);
        [SerializeField] private float _plateauRadius = 65f;
        [SerializeField] private float _plateauElevation = 15.5f;

        [Header("Bagmati River Meander")]
        [SerializeField] private float _riverBaseX = 135f;
        [SerializeField] private float _riverWidth = 22f;
        [SerializeField] private float _riverDepth = 2.2f;
        [SerializeField] private float _waterSurfaceElevation = 9.5f;

        [Header("Stepped Agricultural Terraces (Khet & Bari)")]
        [SerializeField] private bool _generateTerraces = true;
        [SerializeField] private float _terraceStepHeight = 3.2f;
        [SerializeField] private float _terraceMinElevation = 17.5f;
        [SerializeField] private float _terraceMaxElevation = 34.0f;

        [Header("Himalayan Foothills (Shivapuri Ridge)")]
        [SerializeField] private float _macroNoiseScale = 0.006f;
        [SerializeField] private float _macroNoiseStrength = 24f;
        [SerializeField] private float _microNoiseScale = 0.022f;
        [SerializeField] private float _microNoiseStrength = 4.0f;

        public float WaterSurfaceElevation => _waterSurfaceElevation;
        public Vector2 PlateauCenter => _plateauCenter;
        public float PlateauElevation => _plateauElevation;

        private void Awake()
        {
            EnsureTerrain();
        }

        private void OnEnable()
        {
            EnsureTerrain();
        }

        private void EnsureTerrain()
        {
            Terrain terrain = GetComponent<Terrain>();
            if (terrain == null || terrain.terrainData == null || terrain.terrainData.heightmapResolution < 2)
            {
                GenerateKathmanduTerrain();
            }
        }

        /// <summary>
        /// Generates or updates the Terrain component on this GameObject.
        /// </summary>
        public Terrain GenerateKathmanduTerrain()
        {
            Terrain terrain = GetComponent<Terrain>();
            if (terrain == null) terrain = gameObject.AddComponent<Terrain>();

            TerrainCollider collider = GetComponent<TerrainCollider>();
            if (collider == null) collider = gameObject.AddComponent<TerrainCollider>();

            TerrainData terrainData = terrain.terrainData;
            if (terrainData == null)
            {
                terrainData = new TerrainData();
                terrain.terrainData = terrainData;
            }
            collider.terrainData = terrainData;

            // Configure dimensions
            terrainData.heightmapResolution = _heightmapResolution;
            terrainData.size = new Vector3(_width, _maxElevation, _length);

            // 1. Generate Heightmap
            float[,] heights = GenerateHeights();
            terrainData.SetHeights(0, 0, heights);

            // 2. Setup Terrain Layers & Splatmap
            SetupTerrainLayers(terrainData);
            GenerateSplatmap(terrainData);

            // 3. Create or update Water Plane
            SetupWaterPlane();

            Debug.Log($"[KathmanduTerrainGenerator] Successfully generated complete Kathmandu Valley Terrain ({_width}x{_length}m, Elevation: {_maxElevation}m).");
            return terrain;
        }

        private float[,] GenerateHeights()
        {
            float[,] heights = new float[_heightmapResolution, _heightmapResolution];
            float invRes = 1.0f / (_heightmapResolution - 1);

            for (int z = 0; z < _heightmapResolution; z++)
            {
                float worldZ = z * invRes * _length;
                float riverCenterX = GetRiverCenterX(worldZ);

                for (int x = 0; x < _heightmapResolution; x++)
                {
                    float worldX = x * invRes * _width;

                    // Base valley floor elevation (gentle 12m floodplain)
                    float elevation = 12.0f;

                    // 1. Founder's Tar Plateau (Gentle, elevated habitable tableland)
                    float distToPlateau = Vector2.Distance(new Vector2(worldX, worldZ), _plateauCenter);
                    if (distToPlateau <= _plateauRadius)
                    {
                        // Perfectly flat tableland for building placement
                        elevation = _plateauElevation;
                    }
                    else if (distToPlateau <= _plateauRadius + 22f)
                    {
                        // Gentle terrace transition down from plateau edge
                        float t = (distToPlateau - _plateauRadius) / 22f;
                        float smoothT = Mathf.SmoothStep(0f, 1f, t);
                        elevation = Mathf.Lerp(_plateauElevation, 12.5f, smoothT);
                    }

                    // 2. Rising Foothills & Mountains
                    // Northern Shivapuri Foothills (Z > 260)
                    if (worldZ > 240f)
                    {
                        float northProgress = Mathf.Clamp01((worldZ - 240f) / (_length - 240f));
                        float northClimb = Mathf.Pow(northProgress, 1.6f) * 38f;

                        // Eastern foothill climb (X > 220)
                        float eastProgress = Mathf.Clamp01((worldX - 200f) / (_width - 200f));
                        float eastClimb = Mathf.Pow(eastProgress, 1.4f) * 16f;

                        // Fractal noise for natural Himalayan ridges
                        float n1 = Mathf.PerlinNoise(worldX * _macroNoiseScale, worldZ * _macroNoiseScale) * _macroNoiseStrength;
                        float n2 = Mathf.PerlinNoise(worldX * _microNoiseScale, worldZ * _microNoiseScale) * _microNoiseStrength;
                        float n3 = Mathf.PerlinNoise(worldX * 0.05f, worldZ * 0.05f) * 1.5f;

                        float hillElevation = 12.0f + northClimb + eastClimb + (n1 + n2 + n3) * northProgress;

                        // Stepped Agricultural Terraces (Khet) on the lower foothills
                        if (_generateTerraces && hillElevation >= _terraceMinElevation && hillElevation <= _terraceMaxElevation)
                        {
                            float tProg = (hillElevation - _terraceMinElevation) / _terraceStepHeight;
                            float stepBase = Mathf.Floor(tProg) * _terraceStepHeight + _terraceMinElevation;
                            float stepFraction = tProg % 1.0f;
                            // 80% flat shelf, 20% steep riser bank
                            float shelf = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((stepFraction - 0.70f) / 0.30f));
                            hillElevation = stepBase + shelf * _terraceStepHeight;
                        }

                        // Blend with plateau if near plateau
                        if (distToPlateau <= _plateauRadius + 18f)
                        {
                            elevation = Mathf.Max(elevation, _plateauElevation);
                        }
                        else
                        {
                            elevation = Mathf.Max(elevation, hillElevation);
                        }
                    }

                    // Western Foothills (Chandragiri Range framing the west, X < 80)
                    if (worldX < 85f)
                    {
                        float westProgress = Mathf.Clamp01((85f - worldX) / 85f);
                        float westClimb = Mathf.Pow(westProgress, 1.5f) * 28f;
                        float westNoise = Mathf.PerlinNoise(worldX * 0.01f + 100f, worldZ * 0.01f + 100f) * 8f;
                        elevation = Mathf.Max(elevation, 12.0f + westClimb + westNoise * westProgress);
                    }

                    // 3. Meandering Bagmati River Carving (flows along west valley)
                    float distToRiver = Mathf.Abs(worldX - riverCenterX);
                    float halfWidth = _riverWidth * 0.5f;

                    if (distToRiver < halfWidth + 14f)
                    {
                        float riverBedTarget = _waterSurfaceElevation - _riverDepth;
                        float bankT = Mathf.Clamp01(distToRiver / (halfWidth + 14f));
                        float smoothBank = Mathf.SmoothStep(0f, 1f, bankT);

                        elevation = Mathf.Lerp(riverBedTarget, elevation, smoothBank);
                    }

                    // 4. Normalize to 0..1 for Unity Terrain
                    heights[z, x] = Mathf.Clamp01(elevation / _maxElevation);
                }
            }

            return heights;
        }

        public float GetRiverCenterX(float worldZ)
        {
            return _riverBaseX + Mathf.Sin(worldZ * 0.014f) * 35f + Mathf.Cos(worldZ * 0.032f) * 12f;
        }

        private void SetupTerrainLayers(TerrainData terrainData)
        {
#if UNITY_EDITOR
            TerrainLayer[] layers = new TerrainLayer[6];

            layers[0] = GetOrCreateTerrainLayer("Assets/Art/Environment/Layer_Grass.terrainlayer", "Assets/Art/Environment/Textures/Grass_Albedo.png", new Vector2(4, 4), new Color(0.38f, 0.55f, 0.28f));
            layers[1] = GetOrCreateTerrainLayer("Assets/Art/Environment/Layer_RockyDirt.terrainlayer", "Assets/Art/Environment/Textures/RockyDirt_Albedo.png", new Vector2(4, 4), new Color(0.55f, 0.44f, 0.32f));
            layers[2] = GetOrCreateTerrainLayer("Assets/Art/Environment/Layer_ForestFloor.terrainlayer", "Assets/Art/Environment/Textures/ForestFloor_Albedo.png", new Vector2(6, 6), new Color(0.28f, 0.38f, 0.22f));
            layers[3] = GetOrCreateTerrainLayer("Assets/Art/Environment/Layer_MossyRock.terrainlayer", "Assets/Art/Environment/Textures/MossyRock_Albedo.png", new Vector2(8, 8), new Color(0.45f, 0.46f, 0.42f));
            layers[4] = GetOrCreateTerrainLayer("Assets/Art/Environment/Layer_FernGround.terrainlayer", "Assets/Art/Environment/Textures/FernGround_Albedo.png", new Vector2(5, 5), new Color(0.32f, 0.50f, 0.25f));
            layers[5] = GetOrCreateTerrainLayer("Assets/Art/Environment/Layer_DirtPath.terrainlayer", "Assets/Art/Environment/Textures/DirtPath_Albedo.png", new Vector2(3, 3), new Color(0.52f, 0.42f, 0.30f));

            terrainData.terrainLayers = layers;
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

#if UNITY_EDITOR
        private TerrainLayer GetOrCreateTerrainLayer(string layerAssetPath, string texturePath, Vector2 tileSize, Color fallbackColor)
        {
            TerrainLayer layer = UnityEditor.AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerAssetPath);
            if (layer == null)
            {
                layer = new TerrainLayer();
                UnityEditor.AssetDatabase.CreateAsset(layer, layerAssetPath);
            }

            Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (tex == null)
            {
                tex = new Texture2D(16, 16);
                Color[] cols = new Color[16 * 16];
                for (int i = 0; i < cols.Length; i++) cols[i] = fallbackColor;
                tex.SetPixels(cols);
                tex.Apply();
            }

            layer.diffuseTexture = tex;
            layer.tileSize = tileSize;
            UnityEditor.EditorUtility.SetDirty(layer);
            return layer;
        }
#endif

        private void GenerateSplatmap(TerrainData terrainData)
        {
            int alphaRes = terrainData.alphamapResolution;
            int numLayers = terrainData.terrainLayers.Length;
            if (numLayers < 6) return;

            float[,,] splatmap = new float[alphaRes, alphaRes, numLayers];

            for (int z = 0; z < alphaRes; z++)
            {
                float normZ = (float)z / (alphaRes - 1);
                float worldZ = normZ * _length;
                float riverX = GetRiverCenterX(worldZ);

                for (int x = 0; x < alphaRes; x++)
                {
                    float normX = (float)x / (alphaRes - 1);
                    float worldX = normX * _width;

                    float steepness = terrainData.GetSteepness(normX, normZ);
                    float elevation = terrainData.GetHeight(Mathf.RoundToInt(normX * (_heightmapResolution - 1)),
                                                           Mathf.RoundToInt(normZ * (_heightmapResolution - 1)));

                    float distToRiver = Mathf.Abs(worldX - riverX);

                    float grassWeight = 0f;
                    float rockyLoamWeight = 0f;
                    float forestFloorWeight = 0f;
                    float mossyRockWeight = 0f;
                    float fernWeight = 0f;
                    float pathWeight = 0f;

                    // 1. Riverbed & Shingle Banks
                    if (distToRiver < _riverWidth * 0.6f)
                    {
                        rockyLoamWeight = 1.0f;
                    }
                    else if (distToRiver < _riverWidth * 0.6f + 10f)
                    {
                        float t = (distToRiver - _riverWidth * 0.6f) / 10f;
                        rockyLoamWeight = 1.0f - t;
                        fernWeight = t;
                    }
                    // 2. Steep Rock Cliffs & High Escarpments
                    else if (steepness > 26f)
                    {
                        mossyRockWeight = 1.0f;
                    }
                    // 3. Upper Mountain Foothills (Forest)
                    else if (elevation > 32f || steepness > 15f)
                    {
                        forestFloorWeight = 0.75f;
                        rockyLoamWeight = 0.25f;
                    }
                    // 4. Stepped Agricultural Terraces
                    else if (elevation >= _terraceMinElevation && elevation <= _terraceMaxElevation)
                    {
                        grassWeight = 0.70f;
                        rockyLoamWeight = 0.30f; // Terrace earthen riser walls
                    }
                    // 5. Founder's Tar Plateau & Meadow
                    else
                    {
                        float distToPlateau = Vector2.Distance(new Vector2(worldX, worldZ), _plateauCenter);
                        if (distToPlateau <= _plateauRadius)
                        {
                            grassWeight = 0.85f;
                            fernWeight = 0.15f;
                        }
                        else
                        {
                            grassWeight = 0.70f;
                            fernWeight = 0.30f;
                        }

                        // Beaten dirt trail connecting Chautara to the river
                        float pathZDist = Mathf.Abs(worldZ - _plateauCenter.y);
                        if (worldX < _plateauCenter.x && worldX > riverX && pathZDist < 3.0f)
                        {
                            pathWeight = 1.0f - (pathZDist / 3.0f);
                            grassWeight *= (1f - pathWeight);
                            fernWeight *= (1f - pathWeight);
                        }
                    }

                    // Normalize weights
                    float total = grassWeight + rockyLoamWeight + forestFloorWeight + mossyRockWeight + fernWeight + pathWeight;
                    if (total > 0f)
                    {
                        splatmap[z, x, 0] = grassWeight / total;
                        splatmap[z, x, 1] = rockyLoamWeight / total;
                        splatmap[z, x, 2] = forestFloorWeight / total;
                        splatmap[z, x, 3] = mossyRockWeight / total;
                        splatmap[z, x, 4] = fernWeight / total;
                        splatmap[z, x, 5] = pathWeight / total;
                    }
                    else
                    {
                        splatmap[z, x, 0] = 1f;
                    }
                }
            }

            terrainData.SetAlphamaps(0, 0, splatmap);
        }

        private void SetupWaterPlane()
        {
            Transform waterTrans = transform.Find("BagmatiRiverWater");
            GameObject waterObj;
            if (waterTrans == null)
            {
                waterObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                waterObj.name = "BagmatiRiverWater";
                waterObj.transform.SetParent(transform, false);
            }
            else
            {
                waterObj = waterTrans.gameObject;
            }

            // Cover the river valley channel
            waterObj.transform.position = new Vector3(_riverBaseX, _waterSurfaceElevation, _length * 0.5f);
            waterObj.transform.localScale = new Vector3(16f, 1f, _length * 0.1f); // 160m width covers all meander turns

            var renderer = waterObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
#if UNITY_EDITOR
                Material defaultLit = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat");
                if (defaultLit != null)
                {
                    Material waterMat = new Material(defaultLit);
                    waterMat.color = new Color(0.16f, 0.44f, 0.54f, 0.82f); // Glacial Himalayan blue-green
                    renderer.material = waterMat;
                }
#endif
            }

            // Remove mesh collider to avoid blocking ground raycasts
            Collider col = waterObj.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);
        }
    }
}
