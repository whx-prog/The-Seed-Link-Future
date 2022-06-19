using System;
using System.Collections.Generic;
using UnityEngine;


namespace TrailsFX {
    public enum TrailStyle {
        Color,
        TextureStamp,
        Clone,
        Outline,
        SpaceDistortion,
        Dash
    }

    public enum ColorSequence {
        Fixed,
        Cycle,
        PingPong,
        Random,
        FixedRandom
    }

    public enum PositionChangeRelative {
        World,
        OtherGameObject
    }

    public static class TrailStyleProperties {

        public static bool supportsColor(this TrailStyle s) {
            return s != TrailStyle.SpaceDistortion;
        }
    }

    [ExecuteInEditMode]
    [HelpURL("https://kronnect.com/support")]
    [DefaultExecutionOrder(100)]
    public partial class TrailEffect : MonoBehaviour {

        #region Public Properties

        public TrailEffectProfile profile;
        [Tooltip("If enabled, settings will be synced with profile.")]
        public bool profileSync;
        public Transform target;

        [SerializeField]
        bool _active = true;
        public bool active { get { return _active; } set { _active = value; if (!_active) wasInactive = true; } }

        public bool executeInEditMode;
        public int ignoreFrames;
        [Tooltip("The duration of this trail.")]
        public float duration = 0.5f;
        public bool continuous;
        [Tooltip("Use max steps to create a smooth trail if trigger condition is satisfied.")]
        public bool smooth;
        public bool checkWorldPosition;
        public float minDistance = 0.1f;
        public PositionChangeRelative worldPositionRelativeOption = PositionChangeRelative.World;
        public Transform worldPositionRelativeTransform;
        public bool checkScreenPosition = true;
        public int minPixelDistance = 10;
        public int stepsBufferSize = 1023;
        public int maxStepsPerFrame = 12;
        public bool checkTime;
        public float timeInterval = 1f;
        public bool checkCollisions;
        public bool orientToSurface = true;
        public bool ground;
        public float surfaceOffset = 0.05f;
        public LayerMask collisionLayerMask = -1;
        public bool drawBehind = true;
        public UnityEngine.Rendering.CullMode cullMode = UnityEngine.Rendering.CullMode.Back;
        public int subMeshMask = -1;
        public Gradient colorOverTime;
        public ColorSequence colorSequence = ColorSequence.Fixed;
        public Color color = Color.white;
        public float colorCycleDuration = 3f;
        public float pingPongSpeed = 1f;
        public Gradient colorStartPalette;
        public Camera cam;
        public TrailStyle effect = TrailStyle.Color;
        public Texture2D texture;
        public Vector3 scale = Vector3.one, scaleStartRandomMin = Vector3.one, scaleStartRandomMax = Vector3.one;
        public AnimationCurve scaleOverTime;
        public bool scaleUniform;
        public Vector3 localPositionRandomMin, localPositionRandomMax;
        public float laserBandWidth = 0.1f, laserIntensity = 20f, laserFlash = 0.2f;
        public Color trailTint = new Color(0f, 0, 0.1f);

        [Tooltip("Allowed animation states separated by commas")]
        public string animationStates;

        public Transform lookTarget;
        public bool lookToCamera = true;
        [Range(0, 1)]
        public float textureCutOff = 0.25f;
        [Range(0, 1)]
        public float normalThreshold = 0.3f;

        public bool useLastAnimationState;
        public int maxBatches = 50;
        public int meshPoolSize = 256;

        #endregion


        const string SKW_ALPHACLIP = "TRAIL_ALPHACLIP";
        const int MAX_BATCH_INSTANCES = 1023; // max number of instances submitted to GPU in a batch. This limit is defined by Unity.
        const int BAKED_GRADIENTS_LENGTH = 256; // number of baked values for the gradients

        struct SnapshotTransform {
            public Matrix4x4 matrix;
            public float time;
            public int meshIndex;
            public Color color;
        }


        public struct SnapshotIndex {
            public float t;
            public int index;
        }

        static class ShaderParams {
            public static int ColorArrayId = Shader.PropertyToID("_Colors");
        }

        SnapshotTransform[] trail;
        SnapshotIndex[] sortIndices;
        int trailIndex;
        Mesh[] meshPool;
        int meshPoolIndex;
        Material trailMask, trailClearMask;
        Material[] trailMaterial;
        Renderer theRenderer;
        Vector3 lastCornerMinPos, lastCornerMaxPos, lastPosition, lastRandomizedPosition, lastRelativePosition;
        Quaternion lastRotation;
        float lastIntervalTimeCheck;
        MaterialPropertyBlock properties;
        Matrix4x4[] matrices;
        Vector4[] colors;
        static int globalRenderQueue = 3100;
        int renderQueue;
        SkinnedMeshRenderer skinnedMeshRenderer;
        bool isSkinned;
        int bakeTime;
        int batchNumber;
        static Mesh quadMesh;
        Dictionary<string, Material> effectMaterials;
        bool orient;
        Vector3 groundNormal;
        int startFrameCount;
        float smoothDuration;
        Animator animator;
        bool isLimitedToAnimationStates;
        int[] stateHashes;
        bool supportsGPUInstancing;
        MaterialPropertyBlock propertyBlock;
        Color colorRandomAtStart;
        Color[] bakedColorOverTime, bakedColorStartPalette;
        float[] bakedScaleOverTime;
        bool wasInactive;

        void OnEnable() {

            CheckEditorSettings();

            // setup materials
            renderQueue = globalRenderQueue;
            globalRenderQueue += maxBatches + 2;
            if (globalRenderQueue > 3500) {
                globalRenderQueue = 3100;
            }
            if (trailMask == null) {
                trailMask = new Material(Shader.Find("TrailsFX/Mask"));
                trailMask.hideFlags = HideFlags.DontSave;
            }
            trailMask.renderQueue = renderQueue;
            if (trailClearMask == null) {
                trailClearMask = Instantiate(Resources.Load<Material>("TrailsFX/TrailClearMask"));
                trailClearMask.hideFlags = HideFlags.DontSave;
            }

            if (properties == null) {
                properties = new MaterialPropertyBlock();
            } else {
                properties.Clear();
            }
            supportsGPUInstancing = SystemInfo.supportsInstancing;
            if (!supportsGPUInstancing) {
                if (propertyBlock == null) {
                    propertyBlock = new MaterialPropertyBlock();
                } else {
                    propertyBlock.Clear();
                }
            }

            if (profileSync && profile != null) {
                profile.Load(this);
            }
            Clear();
        }

        void DestroyMaterial(Material mat) {
            if (mat != null) {
                DestroyImmediate(mat);
            }
        }

        void OnDestroy() {
            DestroyMaterial(trailMask);
            DestroyMaterial(trailClearMask);
            if (trailMaterial != null) {
                for (int k = 0; k < trailMaterial.Length; k++) {
                    DestroyMaterial(trailMaterial[k]);
                }
            }
            if (effectMaterials != null) {
                foreach (KeyValuePair<string, Material> kvp in effectMaterials) {
                    DestroyMaterial(kvp.Value);
                }
            }
            if (isSkinned && meshPool != null) {
                for(int k=0;k<meshPool.Length;k++) {
                    if (meshPool[k] != null) {
                        DestroyImmediate(meshPool[k]);
                    }
                }
            }
        }

        void OnValidate() {
            CheckEditorSettings();
        }

        void Start() {

            startFrameCount = Time.frameCount;
            if (executeInEditMode || Application.isPlaying) {
                UpdateMaterialProperties();
            }
            colorRandomAtStart = bakedColorStartPalette[UnityEngine.Random.Range(0, BAKED_GRADIENTS_LENGTH)];
        }


#if UNITY_EDITOR
        private void OnDisable() {
            UnityEditor.EditorApplication.update -= ExecuteInEditor;
        }

        void ExecuteInEditor() {
            UnityEditor.EditorUtility.SetDirty(this);

        }
#endif


        void LateUpdate() {

            if (!executeInEditMode && !Application.isPlaying)
                return;

            if (trail == null)
                return;

            if (cam == null) {
                cam = Camera.main;
                if (cam == null) {
                    cam = FindObjectOfType<Camera>();
                    if (cam == null)
                        return;
                }
            }

            AddSnapshot();

            RenderTrail();
        }


        void OnCollisionEnter(Collision collision) {
            if (!checkCollisions || !active)
                return;

            if (((1 << collision.gameObject.layer) & collisionLayerMask) == 0)
                return;

            Quaternion rotation;
            ContactPoint contact = collision.contacts[0];
            Vector3 pos = contact.point;
            pos += contact.normal * surfaceOffset;
            if (orientToSurface) {
                rotation = Quaternion.LookRotation(-contact.normal);
            } else {
                if (lookTarget != null) {
                    rotation = Quaternion.LookRotation(pos - lookTarget.transform.position);
                } else if (lookToCamera) {
                    Camera camera = cam;
                    if (camera == null) {
                        camera = Camera.main;
                    }
                    if (camera != null) {
                        rotation = Quaternion.LookRotation(pos - camera.transform.position);
                    } else {
                        rotation = target.rotation;
                    }
                } else {
                    rotation = target.rotation;
                }
            }
            AddSnapshot(pos, rotation);
        }



        public void CheckEditorSettings() {
            if (target == null) {
                target = transform;
            }
            if (colorOverTime == null) {
                colorOverTime = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[2];
                colorKeys[0].color = Color.yellow;
                colorKeys[0].time = 0f;
                colorKeys[1].color = Color.yellow;
                colorKeys[1].time = 1f;
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0].alpha = 1f;
                alphaKeys[0].time = 0f;
                alphaKeys[1].alpha = 1f;
                alphaKeys[1].time = 10f;
                colorOverTime.SetKeys(colorKeys, alphaKeys);
            }
            if (scaleOverTime == null) {
                scaleOverTime = new AnimationCurve();
                Keyframe[] keys = new Keyframe[2];
                keys[0].value = 1f;
                keys[1].time = 0;
                keys[1].value = 1f;
                keys[1].time = 1;
                scaleOverTime.keys = keys;
            }
            if (colorStartPalette == null) {
                colorStartPalette = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[3];
                colorKeys[0].color = Color.red;
                colorKeys[0].time = 0f;
                colorKeys[1].color = Color.green;
                colorKeys[1].time = 0.5f;
                colorKeys[2].color = Color.blue;
                colorKeys[2].time = 1f;
                colorStartPalette.colorKeys = colorKeys;
            }
        }

        public void Clear() {
            UpdateMaterialProperties();
            if (theRenderer != null) {
                StoreCurrentPositions();
            }
            lastRandomizedPosition = GetRandomizedPosition();
            lastRotation = target.rotation;
            meshPoolIndex = 0;
            if (trail != null) {
                for (int k = 0; k < trail.Length; k++) {
                    trail[k].time = float.MinValue;
                }
            }
            trailIndex = -1;
            startFrameCount = Time.frameCount;
        }

        public void UpdateMaterialProperties() {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= ExecuteInEditor;
            if (executeInEditMode) {
                UnityEditor.EditorApplication.update += ExecuteInEditor;
            }

#endif
            CheckEditorSettings();
            if (bakedColorOverTime == null || bakedColorOverTime.Length == 0) {
                bakedColorOverTime = new Color[BAKED_GRADIENTS_LENGTH];
            }
            if (bakedScaleOverTime == null || bakedScaleOverTime.Length == 0) {
                bakedScaleOverTime = new float[BAKED_GRADIENTS_LENGTH];
            }
            if (bakedColorStartPalette == null || bakedColorStartPalette.Length == 0) {
                bakedColorStartPalette = new Color[BAKED_GRADIENTS_LENGTH];
            }
            for (int k = 0; k < BAKED_GRADIENTS_LENGTH; k++) {
                float t = (float)k / BAKED_GRADIENTS_LENGTH;
                bakedColorOverTime[k] = colorOverTime.Evaluate(t);
                bakedScaleOverTime[k] = scaleOverTime.Evaluate(t);
                bakedColorStartPalette[k] = colorStartPalette.Evaluate(t);
            }

            groundNormal = Vector3.up;
            skinnedMeshRenderer = null;
            theRenderer = target.GetComponentInChildren<Renderer>();
            if (theRenderer == null) {
                trail = null;
                if (Application.isPlaying) {
                    enabled = false;
                }
                return;
            }

            isLimitedToAnimationStates = false;
            if (!string.IsNullOrEmpty(animationStates)) {
                animator = target.GetComponentInChildren<Animator>();
                if (animator == null) {
                    animator = target.GetComponentInParent<Animator>();
                }
                isLimitedToAnimationStates = animator != null;
                if (isLimitedToAnimationStates) {
                    string[] names = animationStates.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    int hashCount = names.Length;
                    stateHashes = new int[hashCount];
                    for (int k = 0; k < hashCount; k++) {
                        stateHashes[k] = Animator.StringToHash(names[k].Trim());
                    }
                }
            }

            isSkinned = theRenderer is SkinnedMeshRenderer;
            if (isSkinned) {
                skinnedMeshRenderer = (SkinnedMeshRenderer)theRenderer;
                int poolSize = useLastAnimationState ? 1 : meshPoolSize;
                if (meshPool == null || meshPool.Length != poolSize) {
                    meshPool = new Mesh[meshPoolSize];
                }
                for (int k = 0; k < meshPool.Length; k++) {
                    if (meshPool[k] == null) {
                        meshPool[k] = new Mesh();
                        meshPool[k].hideFlags = HideFlags.DontSave;
                    }
                }
            } else {
                MeshCollider mc = theRenderer.GetComponent<MeshCollider>();
                if (meshPool == null || meshPool.Length != 1) {
                    meshPool = new Mesh[1];
                }
                if (mc != null) {
                    meshPool[0] = mc.sharedMesh;
                } else {
                    MeshFilter mf = theRenderer.GetComponent<MeshFilter>();
                    if (mf != null) {
                        meshPool[0] = mf.sharedMesh;
                    }
                }
            }

            // Runtime only setup
            if (!executeInEditMode && !Application.isPlaying)
                return;


            orient = false;
            if (trailMask == null) return;
            trailMask.DisableKeyword(SKW_ALPHACLIP);
            trailMask.mainTexture = null;
            trailMask.SetInt("_Cull", (int)cullMode);

            Material trailMat = null;
            switch (effect) {
                case TrailStyle.Color:
                    trailMat = GetEffectMaterial("TrailEffectColor");
                    break;
                case TrailStyle.TextureStamp:
                    trailMat = GetEffectMaterial("TrailEffectTextureStamp");
                    if (quadMesh == null) {
                        quadMesh = BuildQuadMesh();
                    }
                    orient = (ground && orientToSurface) || lookToCamera || lookTarget != null;
                    break;
                case TrailStyle.Clone:
                    trailMat = GetEffectMaterial("TrailEffectClone");
                    break;
                case TrailStyle.Outline:
                    trailMat = GetEffectMaterial("TrailEffectOutline");
                    break;
                case TrailStyle.SpaceDistortion:
                    trailMat = GetEffectMaterial("TrailEffectDistort");
                    break;
                case TrailStyle.Dash:
                    trailMat = GetEffectMaterial("TrailEffectLaser");
                    break;
            }
            if (trailMat == null) {
                trail = null;
                enabled = false;
                return;
            }
            trailMat.SetInt("_Cull", (int)cullMode);


            if (trailMaterial == null || trailMaterial.Length != maxBatches) {
                if (trailMaterial != null) {
                    for (int k = 0; k < trailMaterial.Length; k++) {
                        DestroyMaterial(trailMaterial[k]);
                    }
                }
                trailMaterial = new Material[maxBatches];
            }
            for (int k = 0; k < trailMaterial.Length; k++) {
                if (trailMaterial[k] == null || trailMaterial[k].shader != trailMat.shader) {
                    trailMaterial[k] = Instantiate(trailMat);
                    trailMaterial[k].hideFlags = HideFlags.DontSave;
                }
                SetMaterialProperties(trailMaterial[k]);
                trailMaterial[k].renderQueue = renderQueue + k + 1;
            }
            trailClearMask.renderQueue = renderQueue + maxBatches + 1;
            trailClearMask.SetInt("_Cull", (int)cullMode);

            if (trail == null || trail.Length != stepsBufferSize) {
                trail = new SnapshotTransform[stepsBufferSize];
                for (int k = 0; k < trail.Length; k++) {
                    trail[k].time = float.MinValue;
                }
                trailIndex = -1;
            }
            if (sortIndices == null || sortIndices.Length != stepsBufferSize) {
                sortIndices = new SnapshotIndex[stepsBufferSize];
            }
            if (matrices == null || matrices.Length != MAX_BATCH_INSTANCES) {
                matrices = new Matrix4x4[MAX_BATCH_INSTANCES];
            }
            if (colors == null || colors.Length != MAX_BATCH_INSTANCES) {
                colors = new Vector4[MAX_BATCH_INSTANCES];
            }

            StoreCurrentPositions();
        }

        /// <summary>
        /// Loads and applies a different profile
        /// </summary>
        public void SetProfile(TrailEffectProfile profile) {
            if (profile != null) {
                profile.Load(this);
            }
        }

        void SetMaterialProperties(Material trailMat) {
            switch (effect) {
                case TrailStyle.Color:
                    break;
                case TrailStyle.TextureStamp:
                    trailMat.renderQueue = renderQueue + 1;
                    trailMat.mainTexture = texture;
                    trailMat.SetFloat("_CutOff", textureCutOff);
                    trailMask.mainTexture = texture;
                    trailMask.SetFloat("_CutOff", textureCutOff);
                    trailMask.EnableKeyword(SKW_ALPHACLIP);
                    break;
                case TrailStyle.Clone:
                    Material origMat = theRenderer.sharedMaterial;
                    if (origMat != null) {
                        trailMat.mainTexture = origMat.mainTexture;
                        trailMat.mainTextureScale = origMat.mainTextureScale;
                        trailMat.mainTextureOffset = origMat.mainTextureOffset;
                        trailMat.SetFloat("_CutOff", textureCutOff);
                        if (textureCutOff > 0) {
                            trailMat.EnableKeyword(SKW_ALPHACLIP);
                        } else {
                            trailMat.DisableKeyword(SKW_ALPHACLIP);
                        }
                    }
                    break;
                case TrailStyle.Outline:
                    trailMat.SetFloat("_NormalThreshold", normalThreshold);
                    break;
                case TrailStyle.SpaceDistortion:
                    trailMat.SetColor("_AdditiveTint", trailTint);
                    break;
                case TrailStyle.Dash:
                    trailMat.SetVector("_LaserData", new Vector3(laserBandWidth, laserIntensity, laserFlash));
                    break;
            }
        }

        Material GetEffectMaterial(string materialName) {
            if (effectMaterials == null) {
                effectMaterials = new Dictionary<string, Material>();
            }
            Material mat;
            if (!effectMaterials.TryGetValue(materialName, out mat)) {
                mat = Resources.Load<Material>("TrailsFX/" + materialName);
                if (mat == null) {
                    Debug.LogError("Could not find trail material " + materialName);
                    return null;
                }
                mat = Instantiate(mat);
                mat.hideFlags = HideFlags.DontSave;
                effectMaterials[materialName] = mat;
            }
            return mat;
        }


        Mesh BuildQuadMesh() {
            Mesh mesh = new Mesh();
            mesh.name = "TrailQuadMesh";

            // Setup vertices
            Vector3[] newVertices = new Vector3[4];
            float halfHeight = 0.5f;
            float halfWidth = 0.5f;
            newVertices[0] = new Vector3(-halfWidth, -halfHeight, 0);
            newVertices[1] = new Vector3(-halfWidth, halfHeight, 0);
            newVertices[2] = new Vector3(halfWidth, -halfHeight, 0);
            newVertices[3] = new Vector3(halfWidth, halfHeight, 0);

            // Setup UVs
            Vector2[] newUVs = new Vector2[newVertices.Length];
            newUVs[0] = new Vector2(0, 0);
            newUVs[1] = new Vector2(0, 1);
            newUVs[2] = new Vector2(1, 0);
            newUVs[3] = new Vector2(1, 1);

            // Setup triangles
            int[] newTriangles = new int[] { 0, 1, 2, 3, 2, 1 };

            // Setup normals
            Vector3[] newNormals = new Vector3[newVertices.Length];
            for (int i = 0; i < newNormals.Length; i++) {
                newNormals[i] = Vector3.forward;
            }

            // Create quad
            mesh.vertices = newVertices;
            mesh.uv = newUVs;
            mesh.triangles = newTriangles;
            mesh.normals = newNormals;

            mesh.RecalculateBounds();

            return mesh;
        }


        Vector3 GetSnapshotScale() {
            if (scaleUniform) {
                float t = UnityEngine.Random.Range(scaleStartRandomMin.x, scaleStartRandomMax.x);
                if (isSkinned) {
                    return new Vector3(t * scale.x, t * scale.y, t * scale.z);
                } else {
                    Vector3 lossyScale = target.lossyScale;
                    return new Vector3(lossyScale.x * scale.x * t, lossyScale.y * scale.y * t, lossyScale.z * scale.z * t);
                }
            } else {
                if (isSkinned) {
                    return new Vector3(scale.x * UnityEngine.Random.Range(scaleStartRandomMin.x, scaleStartRandomMax.x),
                        scale.y * UnityEngine.Random.Range(scaleStartRandomMin.y, scaleStartRandomMax.y),
                        scale.z * UnityEngine.Random.Range(scaleStartRandomMin.z, scaleStartRandomMax.z));
                } else {
                    Vector3 lossyScale = target.lossyScale;
                    return new Vector3(lossyScale.x * scale.x * UnityEngine.Random.Range(scaleStartRandomMin.x, scaleStartRandomMax.x),
                        lossyScale.y * scale.y * UnityEngine.Random.Range(scaleStartRandomMin.y, scaleStartRandomMax.y),
                        lossyScale.z * scale.z * UnityEngine.Random.Range(scaleStartRandomMin.z, scaleStartRandomMax.z));
                }
            }
        }


        Vector3 GetRandomizedPosition() {
            Vector3 localPos = new Vector3(UnityEngine.Random.Range(localPositionRandomMin.x, localPositionRandomMax.x),
                                            UnityEngine.Random.Range(localPositionRandomMin.y, localPositionRandomMax.y),
                                            UnityEngine.Random.Range(localPositionRandomMin.z, localPositionRandomMax.z));
            Vector3 wpos = target.position;
            Vector3 pos;
            if (lastPosition == wpos) {
                pos = localPos + wpos;
            } else {
                pos = (Quaternion.LookRotation(wpos - lastPosition) * localPos) + wpos;
            }
            if (ground) {
                Ray ray = new Ray(target.position, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) {
                    pos = hit.point + pos - target.position;
                    groundNormal = hit.normal;
                }
            } else if (effect == TrailStyle.TextureStamp) {
                pos += theRenderer.bounds.center - target.position;
            }

            return pos;
        }

        Color GetSnapshotColor() {
            if (effect == TrailStyle.SpaceDistortion) {
                Vector2 scrPos0 = cam.WorldToViewportPoint(target.position);
                Vector2 scrPos1 = cam.WorldToViewportPoint(lastPosition);
                Vector2 diff = (scrPos0 - scrPos1).normalized;
                diff.x += 0.5f;
                diff.y += 0.5f;
                return new Color(diff.x, diff.y, 0);
            } else {
                switch (colorSequence) {
                    case ColorSequence.Random:
                        return bakedColorStartPalette[UnityEngine.Random.Range(0, BAKED_GRADIENTS_LENGTH)];
                    case ColorSequence.FixedRandom:
                        return colorRandomAtStart;
                    case ColorSequence.Cycle: {
                            if (colorCycleDuration < 0) {
                                colorCycleDuration = 0.01f;
                            }
                            float t = Time.time / colorCycleDuration;
                            int it = (int)((t - (int)t) * BAKED_GRADIENTS_LENGTH);
                            return bakedColorStartPalette[it];
                        }
                    case ColorSequence.PingPong: {
                            float t = Mathf.PingPong(Time.time * pingPongSpeed, 1f);
                            int it = (int)(t * BAKED_GRADIENTS_LENGTH);
                            return bakedColorStartPalette[it];
                        }
                    default:
                        return color;
                }
            }
        }

        void StoreCurrentPositions() {
            if (executeInEditMode || Application.isPlaying) {
                Bounds bounds = theRenderer.bounds;
                lastCornerMinPos = bounds.min;
                lastCornerMaxPos = bounds.max;
                lastPosition = target.position;
                lastRelativePosition = lastPosition;
                if (worldPositionRelativeOption == PositionChangeRelative.OtherGameObject && worldPositionRelativeTransform != null) {
                    lastRelativePosition -= worldPositionRelativeTransform.position;

                }
                lastRotation = target.rotation;
            }
        }


        void AddSnapshot() {
            if (!active || !theRenderer.enabled) {
                wasInactive = true;
                return;
            }

            if (wasInactive) {
                wasInactive = false;
                lastRandomizedPosition = GetRandomizedPosition();
                StoreCurrentPositions();
            }

            bool skip = Time.frameCount - startFrameCount < ignoreFrames;
            if (isLimitedToAnimationStates && !skip) {
                skip = true;
                int layersCount = animator.layerCount;
                for (int l = 0; l < layersCount && skip; l++) {
                    int hash = animator.GetCurrentAnimatorStateInfo(l).shortNameHash;
                    for (int k = 0; k < stateHashes.Length; k++) {
                        if (stateHashes[k] == hash) {
                            skip = false;
                            break;
                        }
                    }
                }
            }
            if (skip) {
                lastRandomizedPosition = GetRandomizedPosition();
                StoreCurrentPositions();
                return;
            }

            float now = Time.time;

            int steps = continuous ? maxStepsPerFrame : 0;
            if (steps == 0) {
                if (checkWorldPosition) {
                    Vector3 referencePosition = target.position;
                    Vector3 referenceLastPos = lastPosition;
                    if (worldPositionRelativeOption == PositionChangeRelative.OtherGameObject && worldPositionRelativeTransform != null) {
                        referencePosition -= worldPositionRelativeTransform.position;
                        referenceLastPos = lastRelativePosition;
                    }
                    float distance = Vector3.Distance(referencePosition, referenceLastPos);
                    if (distance >= minDistance) {
                        if (smooth) {
                            smoothDuration = now + 1f;
                        } else {
                            steps = (int)(distance / minDistance);
                        }
                    }
                }

                if (checkScreenPosition) {
                    if (minPixelDistance <= 0) {
                        minPixelDistance = 1;
                    }

                    // Difference of corners in viewport from last frame
                    Vector2 viewportPos0 = cam.WorldToViewportPoint(lastCornerMinPos);
                    Vector2 viewportPos1 = cam.WorldToViewportPoint(theRenderer.bounds.min);
                    int pixelDistance = Mathf.Max(Mathf.CeilToInt(Mathf.Abs(viewportPos1.x - viewportPos0.x) * cam.pixelWidth), Mathf.CeilToInt(Mathf.Abs(viewportPos1.y - viewportPos0.y) * cam.pixelHeight));
                    int stepsCornerMin = pixelDistance / minPixelDistance;

                    viewportPos0 = cam.WorldToViewportPoint(lastCornerMaxPos);
                    viewportPos1 = cam.WorldToViewportPoint(theRenderer.bounds.max);
                    pixelDistance = Mathf.Max((int)(Mathf.Abs(viewportPos1.x - viewportPos0.x) * cam.pixelWidth), (int)(Mathf.Abs(viewportPos1.y - viewportPos0.y) * cam.pixelHeight));
                    if (pixelDistance >= minPixelDistance) {
                        if (smooth) {
                            smoothDuration = now + 1f;
                        } else {
                            int stepsCornerMax = pixelDistance / minPixelDistance;
                            steps = Mathf.Max(steps, Mathf.Max(stepsCornerMax, stepsCornerMin));
                        }
                    }
                }

                if (checkTime) {
                    if (now - lastIntervalTimeCheck >= timeInterval) {
                        lastIntervalTimeCheck = now;
                        steps = Mathf.Max(1, steps);
                    }
                }
            }

            if (now < smoothDuration) {
                steps = maxStepsPerFrame;
            }

            if (steps <= 0)
                return;

            if (steps > maxStepsPerFrame) {
                steps = maxStepsPerFrame;
            }

            SetupMesh();

            Vector3 pos = GetRandomizedPosition();
            Vector3 targetPos = Vector3.zero;
            Vector3 upwards = Vector3.up;
            if (ground && orientToSurface) {
                targetPos = pos + groundNormal;
                if (target.position != lastPosition) {
                    upwards = target.position - lastPosition;
                } else {
                    upwards = target.forward;
                }
            } else if (orient) {
                if (lookTarget != null) {
                    targetPos = lookTarget.position;
                } else {
                    Camera camera = cam;
                    if (camera == null) {
                        camera = Camera.main;
                    }
                    if (camera != null) {
                        targetPos = camera.transform.position;
                    } else {
                        orient = false;
                    }
                }
            }

            Vector3 scale = GetSnapshotScale();
            Color color = GetSnapshotColor();
            float lastFrameTime = now - Time.deltaTime;

            for (int k = 0; k < steps; k++) {
                trailIndex++;
                if (trailIndex >= trail.Length) {
                    trailIndex = 0;
                }
                float t = (float)(k + 1) / steps;
                Vector3 p = Vector3.Lerp(lastRandomizedPosition, pos, t);
                if (orient) {
                    trail[trailIndex].matrix = Matrix4x4.TRS(p, Quaternion.LookRotation(p - targetPos, upwards), scale);
                } else {
                    Quaternion rot = Quaternion.Slerp(lastRotation, target.rotation, t);
                    trail[trailIndex].matrix = Matrix4x4.TRS(p, rot, scale);
                }
                trail[trailIndex].time = (lastFrameTime * (1f - t)) + now * t;
                trail[trailIndex].meshIndex = meshPoolIndex;
                trail[trailIndex].color = color;
            }

            lastRandomizedPosition = pos;
            StoreCurrentPositions();

        }

        void AddSnapshot(Vector3 pos, Quaternion rotation) {
            if (!active || !theRenderer.enabled)
                return;

            SetupMesh();

            Vector3 scale = GetSnapshotScale();
            trailIndex++;
            if (trailIndex >= trail.Length) {
                trailIndex = 0;
            }
            trail[trailIndex].matrix = Matrix4x4.TRS(pos, rotation, scale);
            trail[trailIndex].time = Time.time;
            trail[trailIndex].meshIndex = meshPoolIndex;
            trail[trailIndex].color = GetSnapshotColor();
        }

        void RenderTrail() {
            if (duration < 0) {
                duration = 0.001f;
            }
            int count = 0;
            float now = Time.time;

            // Pick entries and compute transition 
            for (int i = 0; i < trail.Length; i++) {
                float t = now - trail[i].time;
                if (t < duration) {
                    sortIndices[count].t = t / duration;
                    sortIndices[count].index = i;
                    count++;
                    if (count >= stepsBufferSize)
                        break;
                }
            }

            if (count == 0)
                return;

            // Sort indices
            QuickSort(0, count - 1);

            // Build batches
            batchNumber = 0;
            bool singleBatch = useLastAnimationState || effect == TrailStyle.TextureStamp;
            if (singleBatch && count <= MAX_BATCH_INSTANCES) {
                SendToGPU(meshPoolIndex, 0, count);
            } else {
                int batchMeshIndex = trail[sortIndices[0].index].meshIndex;
                int batchStartIndex = 0;
                int batchInstancesCount = 1;

                for (int k = 1; k < count; k++) {
                    int i = sortIndices[k].index;
                    int meshIndex = trail[i].meshIndex;
                    if (meshIndex != batchMeshIndex || batchInstancesCount >= MAX_BATCH_INSTANCES) {
                        // send previous batch
                        SendToGPU(batchMeshIndex, batchStartIndex, batchInstancesCount);
                        // prepare new batch
                        batchMeshIndex = meshIndex;
                        batchStartIndex += batchInstancesCount;
                        batchInstancesCount = 0;
                    }
                    batchInstancesCount++;
                }
                if (batchInstancesCount > 0) {
                    // send last batch
                    SendToGPU(batchMeshIndex, batchStartIndex, batchInstancesCount);
                }
            }
        }

        void SendToGPU(int meshIndex, int startIndex, int count) {
            if (meshIndex < 0)
                return;

            Mesh batchMesh = effect == TrailStyle.TextureStamp ? quadMesh : meshPool[meshIndex];
            if (batchMesh == null)
                return;

            int layer = target.gameObject.layer;

            if (drawBehind && batchNumber == 0) {
                Vector3 pos = target.position;
                Vector3 sca;
                Mesh mesh;
                if (isSkinned) {
                    mesh = SetupMesh();
                    sca = Vector3.one;
                } else {
                    mesh = meshPool[meshIndex];
                    sca = target.lossyScale;
                }
                if (mesh != null) {
                    Matrix4x4 m = Matrix4x4.TRS(pos, target.rotation, sca);
                    if (subMeshMask > 0) {
                        int subMeshCount = mesh.subMeshCount;
                        for (int k = 0; k < subMeshCount; k++) {
                            if (((1 << k) & subMeshMask) != 0) {
                                Graphics.DrawMesh(mesh, m, trailMask, layer, null, k); // runs first in render queue
                                Graphics.DrawMesh(mesh, m, trailClearMask, layer, null, k); // runs last in render queue
                            }
                        }
                    } else {
                        Graphics.DrawMesh(mesh, m, trailMask, layer); // runs first in render queue
                        Graphics.DrawMesh(mesh, m, trailClearMask, layer); // runs last in render queue
                    }
                }
            }

            // Pack for instancing
            for (int o = 0; o < count; o++, startIndex++) {
                int index = sortIndices[startIndex].index;
                float t = sortIndices[startIndex].t;
                int it = (int)(BAKED_GRADIENTS_LENGTH * t) % BAKED_GRADIENTS_LENGTH;

                // Assign RGBA
                Color baseColor = trail[index].color;
                Color color = bakedColorOverTime[it];
                colors[o].x = color.r * baseColor.r;
                colors[o].y = color.g * baseColor.g;
                colors[o].z = color.b * baseColor.b;
                colors[o].w = color.a * baseColor.a * (1f - t);

                // Set matrix
                float scale = bakedScaleOverTime[it];

                matrices[o].m00 = trail[index].matrix.m00 * scale;
                matrices[o].m01 = trail[index].matrix.m01 * scale;
                matrices[o].m02 = trail[index].matrix.m02 * scale;
                matrices[o].m03 = trail[index].matrix.m03;
                matrices[o].m10 = trail[index].matrix.m10 * scale;
                matrices[o].m11 = trail[index].matrix.m11 * scale;
                matrices[o].m12 = trail[index].matrix.m12 * scale;
                matrices[o].m13 = trail[index].matrix.m13;
                matrices[o].m20 = trail[index].matrix.m20 * scale;
                matrices[o].m21 = trail[index].matrix.m21 * scale;
                matrices[o].m22 = trail[index].matrix.m22 * scale;
                matrices[o].m23 = trail[index].matrix.m23;
                matrices[o].m30 = trail[index].matrix.m30;
                matrices[o].m31 = trail[index].matrix.m31;
                matrices[o].m32 = trail[index].matrix.m32;
                matrices[o].m33 = trail[index].matrix.m33;

            }

            // Send batch to pipeline
            properties.SetVectorArray(ShaderParams.ColorArrayId, colors);
            if (batchNumber < trailMaterial.Length - 1) {
                batchNumber++;
            }
            if (supportsGPUInstancing) {
                for (int s = 0; s < batchMesh.subMeshCount; s++) {
                    if (((1 << s) & subMeshMask) != 0) {
                        Graphics.DrawMeshInstanced(batchMesh, s, trailMaterial[batchNumber], matrices, count, properties, UnityEngine.Rendering.ShadowCastingMode.Off, false, layer);
                    }
                }
                // Clear stencil buffer
                for (int s = 0; s < batchMesh.subMeshCount; s++) {
                    if (((1 << s) & subMeshMask) != 0) {
                        Graphics.DrawMeshInstanced(batchMesh, s, trailClearMask, matrices, count, null, UnityEngine.Rendering.ShadowCastingMode.Off, false, layer);
                    }
                }
            } else {
                // Fallback for GPUs not supporting instancing; better than nothing :(
                for (int i = 0; i < count; i++) {
                    propertyBlock.SetVector(ShaderParams.ColorArrayId, colors[i]);
                    for (int s = 0; s < batchMesh.subMeshCount; s++) {
                        if (((1 << s) & subMeshMask) != 0) {
                            Graphics.DrawMesh(batchMesh, matrices[i], trailMaterial[batchNumber], layer, null, s, propertyBlock, false, false);
                        }
                    }
                }
                // Clear stencil buffer
                for (int s = 0; s < batchMesh.subMeshCount; s++) {
                    if (((1 << s) & subMeshMask) != 0) {
                        for (int i = 0; i < count; i++) {
                            Graphics.DrawMesh(batchMesh, matrices[i], trailClearMask, layer, null, s, null, false, false);
                        }
                    }
                }
            }
        }


        Mesh SetupMesh() {
            if (isSkinned) {
                int thisFrame = Time.frameCount;
                if (thisFrame != bakeTime) {
                    meshPoolIndex++;
                    if (meshPoolIndex >= meshPool.Length) {
                        meshPoolIndex = 0;
                    }
                    skinnedMeshRenderer.BakeMesh(meshPool[meshPoolIndex]);
                    bakeTime = thisFrame;
                }
            }
            return meshPool[meshPoolIndex];
        }

        void QuickSort(int min, int max) {
            int i = min;
            int j = max;

            float x = sortIndices[(min + max) / 2].t;

            do {
                while (sortIndices[i].t < x) {
                    i++;
                }
                while (sortIndices[j].t > x) {
                    j--;
                }
                if (i <= j) {
                    SnapshotIndex h = sortIndices[i];
                    sortIndices[i] = sortIndices[j];
                    sortIndices[j] = h;
                    i++;
                    j--;
                }
            } while (i <= j);

            if (min < j) {
                QuickSort(min, j);
            }
            if (i < max) {
                QuickSort(i, max);
            }
        }

    }



}

