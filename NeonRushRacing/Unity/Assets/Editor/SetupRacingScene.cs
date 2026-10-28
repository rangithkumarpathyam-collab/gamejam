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
        private const string PlayerCarModelPath = "Assets/Models/RacingCar.fbx";
        private const string CyanCarModelPath = "Assets/Models/RacingCar_Cyan.fbx";
        private const string GoldCarModelPath = "Assets/Models/RacingCar_Gold.fbx";
        private const string GreenCarModelPath = "Assets/Models/RacingCar_Green.fbx";
        private const string PhysicMatPath = "Assets/Materials/CarFrictionless.physicMaterial";

        [MenuItem("Neon Rush/Setup Racing Scene", false, 10)]
        public static void BuildScene()
        {
            Debug.Log("[NeonRush] Building Complete Neon Rush Grand Prix Racing Scene...");

            string scenesDir = Path.Combine(Application.dataPath, "Scenes");
            if (!Directory.Exists(scenesDir)) Directory.CreateDirectory(scenesDir);

            // Create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Frictionless Physics Material
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

            // 2. Instantiate Track
            GameObject trackPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TrackModelPath);
            if (trackPrefab != null)
            {
                GameObject track = (GameObject)PrefabUtility.InstantiatePrefab(trackPrefab);
                track.name = "NeonRushTrack";
                track.transform.position = Vector3.zero;
                track.transform.rotation = Quaternion.identity;

                SetupTrackColliders(track);
                Debug.Log("[NeonRush] Configured track colliders on road and barriers.");
            }

            // 3. Directional Light & Environment
            GameObject lightObj = new GameObject("Directional Light");
            Light dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.75f, 0.82f, 1.0f);
            dirLight.intensity = 1.05f;
            dirLight.shadows = LightShadows.Soft;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.18f, 0.22f, 0.35f);
            RenderSettings.ambientEquatorColor = new Color(0.12f, 0.15f, 0.22f);
            RenderSettings.ambientGroundColor = new Color(0.05f, 0.06f, 0.09f);

            // 4. Starting Grid Coordinates
            Vector3[] gridPositions = new Vector3[]
            {
                new Vector3(-75.0f, 0.35f, -90.0f), // Slot 1: Player
                new Vector3(-75.0f, 0.35f, -86.2f), // Slot 2: AI Cyan Viper
                new Vector3(-82.0f, 0.35f, -90.0f), // Slot 3: AI Golden Phoenix
                new Vector3(-82.0f, 0.35f, -86.2f)  // Slot 4: AI Neon Emerald
            };
            Quaternion gridRot = Quaternion.Euler(0f, 90f, 0f);

            // 5. Instantiate Player Car
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCarModelPath);
            GameObject playerCar = null;
            if (playerPrefab != null)
            {
                playerCar = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerCar.name = "PlayerCar";
                playerCar.transform.position = gridPositions[0];
                playerCar.transform.rotation = gridRot;

                StripChildCameras(playerCar);
                ConfigureCarPhysics(playerCar, frictionMat);

                ArcadeCarController controller = playerCar.AddComponent<ArcadeCarController>();
                controller.SanitizeWheels();
                playerCar.AddComponent<CarAudio>();
                Debug.Log("[NeonRush] Configured Player Car (Carmine Red Metallic) in Slot 1.");
            }

            // 6. Instantiate 3 AI Rivals
            string[] aiNames = new string[] { "Rival_CyanViper", "Rival_GoldenPhoenix", "Rival_NeonEmerald" };
            string[] aiDriverLabels = new string[] { "Cyan Viper", "Golden Phoenix", "Neon Emerald" };
            Color[] aiColors = new Color[] { new Color(0f, 0.9f, 1f), new Color(1f, 0.85f, 0.1f), new Color(0.1f, 1f, 0.4f) };
            string[] aiPaths = new string[] { CyanCarModelPath, GoldCarModelPath, GreenCarModelPath };

            for (int i = 0; i < 3; i++)
            {
                GameObject aiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(aiPaths[i]);
                if (aiPrefab != null)
                {
                    GameObject aiCar = (GameObject)PrefabUtility.InstantiatePrefab(aiPrefab);
                    aiCar.name = aiNames[i];
                    aiCar.transform.position = gridPositions[i + 1];
                    aiCar.transform.rotation = gridRot;

                    StripChildCameras(aiCar);
                    ConfigureCarPhysics(aiCar, frictionMat);

                    AICarController aiController = aiCar.AddComponent<AICarController>();
                    aiController.driverName = aiDriverLabels[i];
                    aiController.carColor = aiColors[i];

                    Debug.Log($"[NeonRush] Configured AI rival: {aiDriverLabels[i]} in Slot {i + 2}.");
                }
            }

            // 7. Setup Main Camera Chase
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

            ArcadeCameraFollow follow = camObj.AddComponent<ArcadeCameraFollow>();
            if (playerCar != null) follow.target = playerCar.transform;

            // 8. Setup RaceManager and RacingHUD
            GameObject rmObj = new GameObject("RaceManager");
            rmObj.AddComponent<RaceManager>();

            GameObject hudObj = new GameObject("RacingHUD");
            hudObj.AddComponent<RacingHUD>();

            // 9. Save Scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[NeonRush] Scene saved successfully to {ScenePath}!");
        }

        private static void ConfigureCarPhysics(GameObject car, PhysicsMaterial frictionMat)
        {
            Rigidbody rb = car.GetComponent<Rigidbody>();
            if (rb == null) rb = car.AddComponent<Rigidbody>();
            rb.mass = 1200f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 2.0f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            CapsuleCollider cap = car.GetComponent<CapsuleCollider>();
            if (cap == null) cap = car.AddComponent<CapsuleCollider>();
            cap.center = new Vector3(0f, 0.55f, 0f);
            cap.radius = 0.55f;
            cap.height = 4.2f;
            cap.direction = 2; // Z axis
            cap.sharedMaterial = frictionMat;
        }

        private static void StripChildCameras(GameObject root)
        {
            Camera[] cams = root.GetComponentsInChildren<Camera>(true);
            foreach (var c in cams) GameObject.DestroyImmediate(c.gameObject);
        }

        private static void SetupTrackColliders(GameObject trackRoot)
        {
            Transform[] allChildren = trackRoot.GetComponentsInChildren<Transform>(true);
            Transform roadColliderT = null;

            foreach (Transform t in allChildren)
            {
                string n = t.name.ToLower();
                Camera c = t.GetComponent<Camera>();
                if (c != null) GameObject.DestroyImmediate(c.gameObject);

                if (n == "track_road_collider")
                {
                    roadColliderT = t;
                    MeshFilter mf = t.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null)
                    {
                        MeshCollider mc = t.GetComponent<MeshCollider>();
                        if (mc == null) mc = t.gameObject.AddComponent<MeshCollider>();
                        mc.sharedMesh = mf.sharedMesh;
                        mc.convex = false;
                    }
                }
                else if (n.Contains("track_barriers") || n.Contains("mountain_ranges"))
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

            if (roadColliderT != null)
            {
                foreach (Transform t in allChildren)
                {
                    if (t.name.ToLower() == "track_road")
                    {
                        MeshCollider mc = t.GetComponent<MeshCollider>();
                        if (mc != null) GameObject.DestroyImmediate(mc);
                    }
                }
            }
        }
    }
}
