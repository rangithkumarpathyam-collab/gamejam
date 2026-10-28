using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonRush
{
    /// <summary>
    /// Bulletproof runtime stabilizer.
    /// Runs before and during scene initialization to:
    /// 1. Bind MeshColliders to Track_Road_Collider, Track_Barriers, and Mountain valley floor.
    /// 2. Clean up stray child cameras and duplicate scattered cars.
    /// 3. Position Player Car at the starting line (-75, 0.35, -90) with ArcadeCarController and CarAudio.
    /// 4. Spawn 3 rival AI cars (Cyan Viper, Golden Phoenix, Neon Emerald) on grid slots.
    /// 5. Ensure Main Camera follows the player with third-person chase camera.
    /// 6. Ensure RaceManager and RacingHUD are active.
    /// 7. Calibrate lighting so the screen is crisp, moody twilight with neon glow (never blown-out white).
    /// </summary>
    [DefaultExecutionOrder(-2000)]
    public class EnsureTrackColliders : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitBeforeSceneLoad()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                StabilizeScene();
            };
        }

        private void Awake()
        {
            StabilizeScene();
        }

        public static void StabilizeScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded && !Application.isPlaying) return;

            // 1. Calibrate Lighting to prevent white blowout
            CalibrateLighting();

            // 2. Setup Track Colliders
            SetupColliders();

            // 3. Setup / Stabilize Player Car
            GameObject playerCar = StabilizePlayerCar();

            // 4. Setup Main Camera Chase
            StabilizeCamera(playerCar);

            // 5. Spawn / Stabilize AI Rival Cars
            StabilizeAICars();

            // 6. Setup Race Manager and HUD
            StabilizeManagers(playerCar);
        }

        private static void CalibrateLighting()
        {
            // Balance Directional Light
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    l.intensity = 1.05f;
                    l.color = new Color(0.75f, 0.82f, 1.0f); // Cool twilight
                    l.shadows = LightShadows.Soft;
                }
            }

            // Ambient lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.18f, 0.22f, 0.35f);
            RenderSettings.ambientEquatorColor = new Color(0.12f, 0.15f, 0.22f);
            RenderSettings.ambientGroundColor = new Color(0.05f, 0.06f, 0.09f);
        }

        private static void SetupColliders()
        {
            MeshFilter[] allMeshFilters = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
            Transform roadColliderT = null;

            foreach (var mf in allMeshFilters)
            {
                if (mf.sharedMesh == null) continue;
                string n = mf.gameObject.name.ToLower();

                if (n == "track_road_collider")
                {
                    roadColliderT = mf.transform;
                    MeshCollider mc = mf.GetComponent<MeshCollider>();
                    if (mc == null) mc = mf.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    mc.convex = false;
                }
                else if (n.Contains("track_barriers"))
                {
                    MeshCollider mc = mf.GetComponent<MeshCollider>();
                    if (mc == null) mc = mf.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    mc.convex = false;
                }
                else if (n.Contains("mountain_ranges"))
                {
                    MeshCollider mc = mf.GetComponent<MeshCollider>();
                    if (mc == null) mc = mf.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    mc.convex = false;
                }
            }

            // If dedicated road collider exists, ensure visual road doesn't have a duplicate collider
            if (roadColliderT != null)
            {
                foreach (var mf in allMeshFilters)
                {
                    if (mf.gameObject.name.ToLower() == "track_road")
                    {
                        MeshCollider mc = mf.GetComponent<MeshCollider>();
                        if (mc != null) Object.Destroy(mc);
                    }
                }
            }
        }

        private static GameObject StabilizePlayerCar()
        {
            // Find existing player car or candidates
            ArcadeCarController controller = Object.FindFirstObjectByType<ArcadeCarController>();
            GameObject car = controller != null ? controller.gameObject : null;

            if (car == null)
            {
                // Look for RacingCar objects in scene
                GameObject[] allRoots = SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var r in allRoots)
                {
                    string n = r.name.ToLower();
                    if (n.Contains("racingcar") || n.Contains("playercar"))
                    {
                        car = r;
                        break;
                    }
                }
            }

            // Remove any other duplicate cars that are not this player car
            Transform[] allTransforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (var t in allTransforms)
            {
                if (t == null) continue;
                GameObject go = t.gameObject;
                if (go != car && t.parent == null)
                {
                    string n = go.name.ToLower();
                    if ((n.Contains("racingcar (") || n.Contains("racingcar(1)") || n.Contains("racingcar(2)")) && !n.Contains("cyan") && !n.Contains("gold") && !n.Contains("green"))
                    {
                        Debug.Log($"[NeonRush] Cleaning up stray duplicate car: {go.name}");
                        Object.Destroy(go);
                    }
                }
            }

            if (car == null)
            {
                // Load Player Car FBX
                GameObject carModel = Resources.Load<GameObject>("RacingCar");
                if (carModel == null)
                {
#if UNITY_EDITOR
                    carModel = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/RacingCar.fbx");
#endif
                }
                if (carModel != null)
                {
                    car = Object.Instantiate(carModel);
                    car.name = "PlayerCar";
                }
                else
                {
                    car = new GameObject("PlayerCar");
                }
            }
            else
            {
                car.name = "PlayerCar";
            }

            // Remove any stray cameras nested inside the car hierarchy
            Camera[] childCams = car.GetComponentsInChildren<Camera>(true);
            foreach (var cc in childCams)
            {
                Object.Destroy(cc.gameObject);
            }

            // Setup Rigidbody
            Rigidbody rb = car.GetComponent<Rigidbody>();
            if (rb == null) rb = car.AddComponent<Rigidbody>();
            rb.mass = 1200f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 2.0f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Setup CapsuleCollider
            CapsuleCollider cap = car.GetComponent<CapsuleCollider>();
            if (cap == null) cap = car.AddComponent<CapsuleCollider>();
            cap.center = new Vector3(0f, 0.55f, 0f);
            cap.radius = 0.55f;
            cap.height = 4.2f;
            cap.direction = 2; // Z forward

            // Setup ArcadeCarController
            controller = car.GetComponent<ArcadeCarController>();
            if (controller == null) controller = car.AddComponent<ArcadeCarController>();
            controller.SanitizeWheels();

            // Setup CarAudio
            CarAudio audio = car.GetComponent<CarAudio>();
            if (audio == null) audio = car.AddComponent<CarAudio>();

            // Snap to Grid Slot 1
            Vector3 startPos = new Vector3(-75.0f, 0.35f, -90.0f);
            Quaternion startRot = Quaternion.Euler(0f, 90f, 0f);
            car.transform.position = startPos;
            car.transform.rotation = startRot;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            return car;
        }

        private static void StabilizeCamera(GameObject playerCar)
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = GameObject.Find("Main Camera");
                if (camObj != null) mainCam = camObj.GetComponent<Camera>();
            }

            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                mainCam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }

            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.04f, 0.05f, 0.09f);
            mainCam.nearClipPlane = 0.3f;
            mainCam.farClipPlane = 1500f;
            mainCam.fieldOfView = 60f;

            ArcadeCameraFollow follow = mainCam.GetComponent<ArcadeCameraFollow>();
            if (follow == null) follow = mainCam.gameObject.AddComponent<ArcadeCameraFollow>();

            if (playerCar != null)
            {
                follow.target = playerCar.transform;
                // Position camera behind player immediately
                Vector3 desiredPos = playerCar.transform.position - playerCar.transform.forward * 7.0f + Vector3.up * 2.8f;
                mainCam.transform.position = desiredPos;
                mainCam.transform.LookAt(playerCar.transform.position + Vector3.up * 1.2f);
            }
        }

        private static void StabilizeAICars()
        {
            AICarController[] existingAIs = Object.FindObjectsByType<AICarController>(FindObjectsSortMode.None);
            if (existingAIs.Length >= 3) return; // Already present

            // Spawn 3 AI Rival Cars
            string[] aiNames = new string[] { "Cyan Viper", "Golden Phoenix", "Neon Emerald" };
            Color[] aiColors = new Color[] { new Color(0f, 0.9f, 1f), new Color(1f, 0.85f, 0.1f), new Color(0.1f, 1f, 0.4f) };
            string[] modelPaths = new string[] { "Assets/Models/RacingCar_Cyan.fbx", "Assets/Models/RacingCar_Gold.fbx", "Assets/Models/RacingCar_Green.fbx" };
            Vector3[] aiSlotPositions = new Vector3[]
            {
                new Vector3(-75.0f, 0.35f, -86.2f),
                new Vector3(-82.0f, 0.35f, -90.0f),
                new Vector3(-82.0f, 0.35f, -86.2f)
            };
            Quaternion gridRot = Quaternion.Euler(0f, 90f, 0f);

            for (int i = 0; i < 3; i++)
            {
                GameObject aiObj = null;
#if UNITY_EDITOR
                GameObject model = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(modelPaths[i]);
                if (model != null)
                {
                    aiObj = Object.Instantiate(model);
                }
#endif
                if (aiObj == null)
                {
                    aiObj = new GameObject($"RivalCar_{i + 1}");
                }

                aiObj.name = $"Rival_{aiNames[i].Replace(" ", "")}";
                aiObj.transform.position = aiSlotPositions[i];
                aiObj.transform.rotation = gridRot;

                // Strip child cameras
                Camera[] cc = aiObj.GetComponentsInChildren<Camera>(true);
                foreach (var c in cc) Object.Destroy(c.gameObject);

                // Add Rigidbody
                Rigidbody rb = aiObj.GetComponent<Rigidbody>();
                if (rb == null) rb = aiObj.AddComponent<Rigidbody>();
                rb.mass = 1200f;
                rb.linearDamping = 0.05f;
                rb.angularDamping = 2.0f;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Add CapsuleCollider
                CapsuleCollider cap = aiObj.GetComponent<CapsuleCollider>();
                if (cap == null) cap = aiObj.AddComponent<CapsuleCollider>();
                cap.center = new Vector3(0f, 0.55f, 0f);
                cap.radius = 0.55f;
                cap.height = 4.2f;
                cap.direction = 2;

                // Add AICarController
                AICarController ai = aiObj.GetComponent<AICarController>();
                if (ai == null) ai = aiObj.AddComponent<AICarController>();
                ai.driverName = aiNames[i];
                ai.carColor = aiColors[i];

                Debug.Log($"[NeonRush] Spawned AI rival: {aiNames[i]} at grid slot {i + 2}");
            }
        }

        private static void StabilizeManagers(GameObject playerCar)
        {
            // RaceManager
            RaceManager rm = Object.FindFirstObjectByType<RaceManager>();
            if (rm == null)
            {
                GameObject rmObj = new GameObject("RaceManager");
                rm = rmObj.AddComponent<RaceManager>();
            }

            // RacingHUD
            RacingHUD hud = Object.FindFirstObjectByType<RacingHUD>();
            if (hud == null)
            {
                GameObject hudObj = new GameObject("RacingHUD");
                hud = hudObj.AddComponent<RacingHUD>();
            }
        }
    }
}
