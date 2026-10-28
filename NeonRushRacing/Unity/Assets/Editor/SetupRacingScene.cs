using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonRush.Editor
{
    [InitializeOnLoad]
    public static class SetupRacingScene
    {
        private const string ScenePath = "Assets/Scenes/RacingScene.unity";
        private const string TrackModelPath = "Assets/Models/NeonRushTrack.fbx";
        private const string CarModelPath = "Assets/Models/RacingCar.fbx";
        private const string PhysicMatPath = "Assets/Materials/CarFrictionless.physicMaterial";

        static SetupRacingScene()
        {
            EditorApplication.delayCall += ExecuteSetupOnce;
        }

        private static void ExecuteSetupOnce()
        {
            if (!File.Exists(Path.Combine(Application.dataPath, "..", ScenePath)))
            {
                BuildScene();
            }
        }

        [MenuItem("Neon Rush/Setup Racing Scene", priority = 10)]
        public static void BuildScene()
        {
            Debug.Log("[NeonRush] Building Complete Neon Rush Racing Scene...");

            string scenesDir = Path.Combine(Application.dataPath, "Scenes");
            if (!Directory.Exists(scenesDir))
            {
                Directory.CreateDirectory(scenesDir);
            }

            // Create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Instantiate Track
            GameObject trackPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TrackModelPath);
            GameObject trackInstance = null;
            if (trackPrefab != null)
            {
                trackInstance = (GameObject)PrefabUtility.InstantiatePrefab(trackPrefab);
                trackInstance.name = "NeonRushTrack";
                trackInstance.transform.position = Vector3.zero;
                trackInstance.transform.rotation = Quaternion.identity;
                trackInstance.transform.localScale = Vector3.one;

                // Configure colliders: Track_Road_Collider gets MeshCollider, visual Track_Road has NO collider
                SetupTrackColliders(trackInstance);
                Debug.Log("[NeonRush] Added colliders to track and barriers.");
            }
            else
            {
                Debug.LogWarning($"[NeonRush] Track model not found at {TrackModelPath}");
            }

            // 2. Add Directional Light and Environment Lighting
            GameObject lightObj = new GameObject("Directional Light");
            Light dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.65f, 0.75f, 0.95f); // Cool twilight blue
            dirLight.intensity = 1.0f;
            dirLight.shadows = LightShadows.Soft;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Environment lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.15f, 0.20f, 0.32f);
            RenderSettings.ambientEquatorColor = new Color(0.10f, 0.12f, 0.20f);
            RenderSettings.ambientGroundColor = new Color(0.04f, 0.05f, 0.08f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.04f, 0.05f, 0.10f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.002f;

            // 3. Load or create frictionless physic material
            PhysicsMaterial frictionMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(PhysicMatPath);
            if (frictionMat == null)
            {
                frictionMat = new PhysicsMaterial("CarFrictionless")
                {
                    dynamicFriction = 0f,
                    staticFriction = 0f,
                    frictionCombine = PhysicsMaterialCombine.Minimum,
                    bounciness = 0f,
                    bounceCombine = PhysicsMaterialCombine.Minimum
                };
                AssetDatabase.CreateAsset(frictionMat, PhysicMatPath);
            }

            // 4. Instantiate Car at Starting Line
            Vector3 carStartPos = new Vector3(-75.0f, 0.5f, -90.0f);
            Quaternion carStartRot = Quaternion.Euler(0f, 90f, 0f);

            GameObject carPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CarModelPath);
            GameObject carInstance = null;
            if (carPrefab != null)
            {
                carInstance = (GameObject)PrefabUtility.InstantiatePrefab(carPrefab);
                carInstance.name = "PlayerCar";
                carInstance.transform.position = carStartPos;
                carInstance.transform.rotation = carStartRot;
                carInstance.transform.localScale = Vector3.one;

                // Clean up any stray child cameras inside car prefab
                Camera[] childCams = carInstance.GetComponentsInChildren<Camera>(true);
                foreach (Camera c in childCams)
                {
                    GameObject.DestroyImmediate(c.gameObject);
                }

                // Setup Rigidbody
                Rigidbody rb = carInstance.GetComponent<Rigidbody>();
                if (rb == null) rb = carInstance.AddComponent<Rigidbody>();
                rb.mass = 1200f;
                rb.linearDamping = 0.05f;
                rb.angularDamping = 2.5f;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Setup CapsuleCollider (horizontal along Z-axis)
                CapsuleCollider cap = carInstance.GetComponent<CapsuleCollider>();
                if (cap == null) cap = carInstance.AddComponent<CapsuleCollider>();
                cap.center = new Vector3(0f, 0.55f, 0f);
                cap.radius = 0.55f;
                cap.height = 4.2f;
                cap.direction = 2; // Z-axis forward
                cap.sharedMaterial = frictionMat;

                // Setup ArcadeCarController
                ArcadeCarController controller = carInstance.GetComponent<ArcadeCarController>();
                if (controller == null) controller = carInstance.AddComponent<ArcadeCarController>();

                Debug.Log($"[NeonRush] PlayerCar configured at start line: {carStartPos}");
            }
            else
            {
                Debug.LogWarning($"[NeonRush] Car model not found at {CarModelPath}");
            }

            // 5. Setup Main Camera with ArcadeCameraFollow
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();

            camObj.transform.position = new Vector3(-82.0f, 3.2f, -90.0f);
            camObj.transform.rotation = Quaternion.Euler(12f, 90f, 0f);
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1500f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.09f);

            if (carInstance != null)
            {
                ArcadeCameraFollow follow = camObj.AddComponent<ArcadeCameraFollow>();
                // Set target via reflection or serialized property
                SerializedObject so = new SerializedObject(follow);
                SerializedProperty targetProp = so.FindProperty("target");
                if (targetProp != null)
                {
                    targetProp.objectReferenceValue = carInstance.transform;
                    so.ApplyModifiedProperties();
                }
            }

            // 6. Save Scene and Assets
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[NeonRush] Successfully created and saved scene to {ScenePath}");
        }

        private static void SetupTrackColliders(GameObject trackRoot)
        {
            Transform[] allChildren = trackRoot.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allChildren)
            {
                string n = t.name.ToLower();

                // Clean up any stray cameras or lights inside track FBX
                Camera c = t.GetComponent<Camera>();
                if (c != null) GameObject.DestroyImmediate(c.gameObject);
                Light l = t.GetComponent<Light>();
                if (l != null) GameObject.DestroyImmediate(l.gameObject);

                // Dedicated Road Collider gets MeshCollider
                if (n == "track_road_collider")
                {
                    MeshFilter mf = t.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null)
                    {
                        MeshCollider mc = t.GetComponent<MeshCollider>();
                        if (mc == null) mc = t.gameObject.AddComponent<MeshCollider>();
                        mc.sharedMesh = mf.sharedMesh;
                        mc.convex = false;
                    }
                }
                // Visual Track_Road should NOT have collider (prevents double collider snagging)
                else if (n == "track_road")
                {
                    MeshCollider mc = t.GetComponent<MeshCollider>();
                    if (mc != null) GameObject.DestroyImmediate(mc);
                }
                // Barriers get MeshCollider
                else if (n.Contains("track_barriers"))
                {
                    MeshFilter mf = t.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null)
                    {
                        MeshCollider mc = t.GetComponent<MeshCollider>();
                        if (mc == null) mc = t.gameObject.AddComponent<MeshCollider>();
                        mc.sharedMesh = mf.sharedMesh;
                        mc.convex = false;
                    }
                }
            }
        }
    }
}
