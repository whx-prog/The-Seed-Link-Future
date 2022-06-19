using UnityEditor;
using UnityEngine;

namespace TrailsFX {

    [CustomEditor(typeof(TrailEffect))]
    [CanEditMultipleObjects]
    public class TrailEffectEditor : Editor {
        SerializedProperty profile, profileSync, executeInEditMode, targetProp, active, ignoreFrames;
        SerializedProperty continuous, smooth;
        SerializedProperty checkWorldPosition, minDistance, worldPositionRelativeOption, worldPositionRelativeTransform;
        SerializedProperty checkScreenPosition, minPixelDistance;
        SerializedProperty checkTime, timeInterval;
        SerializedProperty checkCollisions, collisionLayerMask, orientToSurface, surfaceOffset;
        SerializedProperty duration, maxStepsPerFrame, stepsBufferSize;

        SerializedProperty drawBehind, cullMode, subMeshMask, cam, lookTarget, lookToCamera, ground;
        SerializedProperty colorSequence, color, colorCycleDuration, pingPongSpeed, colorStartPalette, colorOverTime;
        SerializedProperty laserBandWidth, laserIntensity, laserFlash;
        SerializedProperty trailTint;
        SerializedProperty effect, texture, textureCutOff;
        SerializedProperty localPositionRandomMin, localPositionRandomMax;
        SerializedProperty scale, scaleStartRandomMin, scaleStartRandomMax, scaleOverTime, scaleUniform;
        SerializedProperty normalThreshold;
        SerializedProperty useLastAnimationState, maxBatches, meshPoolSize, animationStates;
        bool profileChanged, enableProfileApply;
        TrailEffect thisEffect;

        void OnEnable() {
            profile = serializedObject.FindProperty("profile");
            profileSync = serializedObject.FindProperty("profileSync");
            targetProp = serializedObject.FindProperty("target");
            executeInEditMode = serializedObject.FindProperty("executeInEditMode");
            active = serializedObject.FindProperty("_active");
            ignoreFrames = serializedObject.FindProperty("ignoreFrames");
            continuous = serializedObject.FindProperty("continuous");
            smooth = serializedObject.FindProperty("smooth");
            checkWorldPosition = serializedObject.FindProperty("checkWorldPosition");
            minDistance = serializedObject.FindProperty("minDistance");
            worldPositionRelativeOption = serializedObject.FindProperty("worldPositionRelativeOption");
            worldPositionRelativeTransform = serializedObject.FindProperty("worldPositionRelativeTransform");
            checkScreenPosition = serializedObject.FindProperty("checkScreenPosition");
            minPixelDistance = serializedObject.FindProperty("minPixelDistance");
            checkTime = serializedObject.FindProperty("checkTime");
            checkCollisions = serializedObject.FindProperty("checkCollisions");
            timeInterval = serializedObject.FindProperty("timeInterval");
            collisionLayerMask = serializedObject.FindProperty("collisionLayerMask");
            orientToSurface = serializedObject.FindProperty("orientToSurface");
            surfaceOffset = serializedObject.FindProperty("surfaceOffset");
            duration = serializedObject.FindProperty("duration");
            maxStepsPerFrame = serializedObject.FindProperty("maxStepsPerFrame");
            stepsBufferSize = serializedObject.FindProperty("stepsBufferSize");
            drawBehind = serializedObject.FindProperty("drawBehind");
            cullMode = serializedObject.FindProperty("cullMode");
            subMeshMask = serializedObject.FindProperty("subMeshMask");
            cam = serializedObject.FindProperty("cam");
            colorSequence = serializedObject.FindProperty("colorSequence");
            color = serializedObject.FindProperty("color");
            colorCycleDuration = serializedObject.FindProperty("colorCycleDuration");
            pingPongSpeed = serializedObject.FindProperty("pingPongSpeed");
            colorStartPalette = serializedObject.FindProperty("colorStartPalette");
            colorOverTime = serializedObject.FindProperty("colorOverTime");
            scaleOverTime = serializedObject.FindProperty("scaleOverTime");
            effect = serializedObject.FindProperty("effect");
            texture = serializedObject.FindProperty("texture");
            textureCutOff = serializedObject.FindProperty("textureCutOff");
            localPositionRandomMin = serializedObject.FindProperty("localPositionRandomMin");
            localPositionRandomMax = serializedObject.FindProperty("localPositionRandomMax");
            scale = serializedObject.FindProperty("scale");
            scaleStartRandomMin = serializedObject.FindProperty("scaleStartRandomMin");
            scaleStartRandomMax = serializedObject.FindProperty("scaleStartRandomMax");
            scaleUniform = serializedObject.FindProperty("scaleUniform");
            lookTarget = serializedObject.FindProperty("lookTarget");
            lookToCamera = serializedObject.FindProperty("lookToCamera");
            ground = serializedObject.FindProperty("ground");
            useLastAnimationState = serializedObject.FindProperty("useLastAnimationState");
            maxBatches = serializedObject.FindProperty("maxBatches");
            meshPoolSize = serializedObject.FindProperty("meshPoolSize");
            animationStates = serializedObject.FindProperty("animationStates");
            normalThreshold = serializedObject.FindProperty("normalThreshold");
            laserBandWidth = serializedObject.FindProperty("laserBandWidth");
            laserIntensity = serializedObject.FindProperty("laserIntensity");
            laserFlash = serializedObject.FindProperty("laserFlash");
            trailTint = serializedObject.FindProperty("trailTint");

            thisEffect = (TrailEffect)target;
            thisEffect.CheckEditorSettings();
        }

        public override void OnInspectorGUI() {
            EditorGUILayout.Separator();
            serializedObject.Update();

            if (!SystemInfo.supportsInstancing) {
                EditorGUILayout.HelpBox("Current platform does not support GPU instancing. Trail effects won't render correctly.", MessageType.Error);
            }

            EditorGUILayout.BeginHorizontal();
            TrailEffectProfile prevProfile = (TrailEffectProfile)profile.objectReferenceValue;
            EditorGUILayout.PropertyField(profile, new GUIContent("Profile", "Create or load stored presets."));
            if (profile.objectReferenceValue != null) {

                if (prevProfile != profile.objectReferenceValue) {
                    profileChanged = true;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(EditorGUIUtility.labelWidth));
                if (GUILayout.Button(new GUIContent("Create", "Creates a new profile which is a copy of the current settings."), GUILayout.Width(60))) {
                    CreateProfile();
                    profileChanged = false;
                    enableProfileApply = false;
                    GUIUtility.ExitGUI();
                    return;
                }
                if (GUILayout.Button(new GUIContent("Load", "Updates settings with the profile configuration."), GUILayout.Width(60))) {
                    profileChanged = true;
                }
                if (!enableProfileApply) {
                    GUI.enabled = false;
                }

                if (GUILayout.Button(new GUIContent("Save", "Updates profile configuration with changes in this inspector."), GUILayout.Width(60))) {
                    enableProfileApply = false;
                    profileChanged = false;
                    thisEffect.profile.Save(thisEffect);
                    EditorUtility.SetDirty(thisEffect.profile);
                    GUIUtility.ExitGUI();
                    return;
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(profileSync, new GUIContent("Sync With Profile", "If disabled, profile settings will only be loaded when clicking 'Load' which allows you to customize settings after loading a profile and keep those changes."));
                EditorGUILayout.BeginHorizontal();
            } else {
                if (GUILayout.Button(new GUIContent("Create", "Creates a new profile which is a copy of the current settings."), GUILayout.Width(60))) {
                    CreateProfile();
                    GUIUtility.ExitGUI();
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(targetProp, new GUIContent("Target", "The object to which trails are attached."));
            Transform t = (Transform)targetProp.objectReferenceValue;
            if (t != null) {
                if (t.GetComponentsInChildren<Renderer>().Length > 1) {
                    EditorGUILayout.HelpBox("More than 1 renderer found under target. Only the first one will be used. If you want to add trails to the others, add a TrailEffect component to each renderer.", MessageType.Warning);
                }
            }
            EditorGUILayout.PropertyField(executeInEditMode, new GUIContent("Execute In Edit Mode", "Render effect also when not in Play Mode."));
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);
            if (GUILayout.Button("Help", GUILayout.Width(60))) {
                EditorUtility.DisplayDialog("Quick help", "Move mouse over any property label to show a tooltip with additional info.\n\nIf you have any question, issue or suggestion, please contact us (check README file!).", "Ok");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(active, new GUIContent("Active", "Enable new trails. If this property is disabled, object won't render more trails when move but existing trail will be visible until it fades out."));
            EditorGUILayout.PropertyField(ignoreFrames, new GUIContent("Ignore First Frames", "Number of initial frames to ignore. Useful to ignore first frames while animation starts."));
            EditorGUILayout.PropertyField(continuous, new GUIContent("Continuous", "Leave a continuous trail."));
            if (!continuous.boolValue) {
                EditorGUILayout.PropertyField(checkWorldPosition, new GUIContent("World Position Change", "Adds a trail when the position of the object in world space changes."));
                if (checkWorldPosition.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(minDistance);
                    EditorGUILayout.PropertyField(smooth);
                    EditorGUILayout.PropertyField(worldPositionRelativeOption, new GUIContent("Relative To", "You can optionally specify a reference position or pivot determined by another gameobject. This can be useful if you want to track position changes with respect to a parent object."));
                    if (worldPositionRelativeOption.intValue == (int)PositionChangeRelative.OtherGameObject) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(worldPositionRelativeTransform, new GUIContent("GameObject"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(checkScreenPosition, new GUIContent("Screen Position Change", "Adds a trail when the screen position of the object changes."));
                if (checkScreenPosition.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(minPixelDistance);
                    EditorGUILayout.PropertyField(cam, new GUIContent("Camera", "A reference camera to compute pixel distance. If left unassigned the main camera will be picked automatically."));
                    EditorGUILayout.PropertyField(smooth);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(checkTime, new GUIContent("Time Interval", "Adds a trail every n seconds or fractions of seconds."));
                if (checkTime.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(timeInterval, new GUIContent("Interval", "Interval in seconds."));
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.PropertyField(checkCollisions, new GUIContent("Collisions", "Adds a trail when object collides with other geometry."));
            if (checkCollisions.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(collisionLayerMask, new GUIContent("Layer Mask"));
                EditorGUILayout.PropertyField(orientToSurface, new GUIContent("Orient To Surface"));
                EditorGUILayout.PropertyField(surfaceOffset, new GUIContent("Surface Offset"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(effect, new GUIContent("Trail Effect", "The type of trail to render."));
            EditorGUI.indentLevel++;
            switch (effect.intValue) {
                case (int)TrailStyle.Color:
                    break;
                case (int)TrailStyle.TextureStamp:
                    EditorGUILayout.PropertyField(texture);
                    EditorGUILayout.PropertyField(textureCutOff, new GUIContent("Cut Off", "Alpha threshold for texture transparency."));
                    EditorGUILayout.PropertyField(lookTarget, new GUIContent("Look Target", "Make the texture face specified target."));
                    if (lookTarget.objectReferenceValue == null) {
                        EditorGUILayout.PropertyField(lookToCamera, new GUIContent("Look To Camera"));
                    }
                    break;
                case (int)TrailStyle.Clone:
                    EditorGUILayout.PropertyField(textureCutOff, new GUIContent("Luminance Cut Off"));
                    break;
                case (int)TrailStyle.Outline:
                    EditorGUILayout.PropertyField(normalThreshold, new GUIContent("Normal Threshold"));
                    break;
                case (int)TrailStyle.Dash:
                    EditorGUILayout.PropertyField(laserBandWidth, new GUIContent("Separation"));
                    EditorGUILayout.PropertyField(laserIntensity, new GUIContent("Intensity"));
                    EditorGUILayout.PropertyField(laserFlash, new GUIContent("Flash"));
                    break;
                case (int)TrailStyle.SpaceDistortion:
                    EditorGUILayout.PropertyField(trailTint, new GUIContent("Additive Tint"));
#if UNITY_2019_3_OR_NEWER
                    try {
                        UnityEngine.Rendering.RenderPipelineAsset pipe = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
                        if (pipe != null) {
                            string pipeName = pipe.ToString();
                            if (pipeName.Contains("Universal") || pipeName.Contains("URP")) {
                                bool opaqueTexture = (bool)pipe.GetType().GetProperty("supportsCameraOpaqueTexture").GetValue(pipe, null);
                                if (!opaqueTexture) {
                                    EditorGUILayout.HelpBox("Space Distortion requires Opaque Texture option in the Universal Rendering Pipeline asset!", MessageType.Error);
                                    if (GUILayout.Button("Go to Universal Rendering Pipeline Asset")) {
                                        Selection.activeObject = pipe;
                                    }
                                }
                            }
                        }
                    } catch { }
#endif
                    break;
            }

            if (((TrailStyle)effect.intValue).supportsColor()) {
                EditorGUILayout.PropertyField(colorSequence, new GUIContent("Color Sequence", "The color used by each trail snapshot"));
                EditorGUI.indentLevel++;
                switch (colorSequence.intValue) {
                    case (int)ColorSequence.Cycle:
                        EditorGUILayout.PropertyField(colorStartPalette, new GUIContent("Palette"));
                        EditorGUILayout.PropertyField(colorCycleDuration, new GUIContent("Cycle Duration"));
                        break;
                    case (int)ColorSequence.PingPong:
                        EditorGUILayout.PropertyField(colorStartPalette, new GUIContent("Palette"));
                        EditorGUILayout.PropertyField(pingPongSpeed, new GUIContent("Speed"));
                        break;
                    case (int)ColorSequence.Random:
                    case (int)ColorSequence.FixedRandom:
                        EditorGUILayout.PropertyField(colorStartPalette, new GUIContent("Palette"));
                        break;
                    case (int)ColorSequence.Fixed:
                        EditorGUILayout.PropertyField(color);
                        break;
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.PropertyField(colorOverTime, new GUIContent("Color Over Time", "Tint color and alpha to be multiplied to the start color."));
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(duration);
            EditorGUILayout.PropertyField(maxStepsPerFrame, new GUIContent("Max Steps Per Frame", "If the object moves fast, the object is drawn several times along the trajectory depending on trigger options above like 'World Position Change', 'Screen Position Change', etc. This property controls the maximum number of trail steps or repetitions per frame."));
            EditorGUILayout.PropertyField(stepsBufferSize, new GUIContent("Steps Buffer Size", "Max number of active trail steps for this object. Should be greater than FPS multiplied by the Max Steps Per Frame. For example, if the 'Max Steps Per Frame' is 15 and the expected FPS of the game is 60, the buffer size should be at least 900 (15x60). Please note that increasing this value beyond necessary will degrade performace."));
            EditorGUILayout.PropertyField(drawBehind, new GUIContent("Draw Behind", "Forces the trail to be rendered behind the object."));
            EditorGUILayout.PropertyField(cullMode, new GUIContent("Cull Mode", "Cull mode for trail rendering."));
            EditorGUILayout.PropertyField(subMeshMask, new GUIContent("SubMesh Mask", "Specify which submeshes are included in the trail. This is a bitmask: -1 = all (default value), 1 = first submesh only, 2 = second submesh only, 3 means first and second submesh only, 4 = third submesh only, and so on."));

            EditorGUILayout.LabelField(new GUIContent("Position", "By default, trail is generated at the object position. In this section you can apply displacement for the source position or specify that the trail should be stamped on the ground."));
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(localPositionRandomMin, new GUIContent("Random Min"));
            EditorGUILayout.PropertyField(localPositionRandomMax, new GUIContent("Random Max"));
            EditorGUILayout.PropertyField(ground, new GUIContent("Ground", "Position the texture at the ground beneath the object"));
            if (ground.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(orientToSurface, new GUIContent("Orient To Surface"));
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField(new GUIContent("Scale", "Scale control for the trail effect."));
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(scaleUniform, new GUIContent("Uniform Scale"));
            if (scaleUniform.boolValue) {
                Vector3 baseScale = scale.vector3Value;
                baseScale.x = EditorGUILayout.FloatField(new GUIContent("Base Scale"), baseScale.x);
                baseScale.y = baseScale.z = baseScale.x;
                scale.vector3Value = baseScale;
                Vector3 min = scaleStartRandomMin.vector3Value;
                min.x = EditorGUILayout.FloatField(new GUIContent("Random Min"), min.x);
                scaleStartRandomMin.vector3Value = min;
                Vector3 max = scaleStartRandomMax.vector3Value;
                max.x = EditorGUILayout.FloatField(new GUIContent("Random Max"), max.x);
                scaleStartRandomMax.vector3Value = max;
            } else {
                EditorGUILayout.PropertyField(scale, new GUIContent("Base Scale"));
                EditorGUILayout.PropertyField(scaleStartRandomMin, new GUIContent("Random Min"));
                EditorGUILayout.PropertyField(scaleStartRandomMax, new GUIContent("Random Max"));
            }
            EditorGUILayout.PropertyField(scaleOverTime, new GUIContent("Scale Over Time"));
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Skinned Mesh Only");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(useLastAnimationState, new GUIContent("Use Last Animation", "Repeats the last animation for the entire trail. This option optimizes the number of batches."));
            if (!useLastAnimationState.boolValue) {
                EditorGUILayout.PropertyField(maxBatches, new GUIContent("Max Batches", "Can specify the maximum number of batches allowed when rendering animated skinned mesh trails."));
                EditorGUILayout.PropertyField(meshPoolSize, new GUIContent("Mesh Pool Size", "Maximum number of baked skinned meshes. Reducing this value will save memory but very long trails may reuse animation steps."));
            }
            EditorGUILayout.PropertyField(animationStates, new GUIContent("Animation States", "Add trails only during these animation states."));
            EditorGUI.indentLevel--;

            if (serializedObject.ApplyModifiedProperties() || profileChanged) {
                if (thisEffect.profile != null) {
                    if (profileChanged) {
                        thisEffect.profile.Load(thisEffect);
                        profileChanged = false;
                        enableProfileApply = false;
                    } else {
                        enableProfileApply = true;
                    }
                }
                foreach (TrailEffect effect in targets) {
                    effect.UpdateMaterialProperties();
                }
            }
        }

        void MarkDirty() {
            EditorUtility.SetDirty(thisEffect);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        #region Profile handling

        void CreateProfile() {

            TrailEffectProfile newProfile = CreateInstance<TrailEffectProfile>();
            newProfile.Save(thisEffect);

            AssetDatabase.CreateAsset(newProfile, "Assets/Trails FX Profile.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newProfile;

            thisEffect.profile = newProfile;
        }


        #endregion


    }

}