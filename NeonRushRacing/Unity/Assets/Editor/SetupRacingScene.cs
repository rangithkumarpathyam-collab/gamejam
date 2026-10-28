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

        static SetupRacingScene()
        {
            EditorApplication.delayCall += ExecuteSetupOnce;
        }

        private static void ExecuteSetupOnce()
        {
            // Only auto-run if the scene doesn't exist yet, or run via menu item anytime
            if (!File.Exists(Path.Combine(Application.dataPath, "..", ScenePath)))
            {
                BuildScene();
            }
        }

        [MenuItem("Neon Rush/Setup Racing Scene", priority = 10)]
        public static void BuildScene()
        {
            Debug.Log("[NeonRush] Setting up RacingScene...");

            // 1. Create or Open Scenes directory
            string scenesDir = Path.Combine(Application.dataPath, "Scenes");
            if (!Directory.Exists(scenesDir))
            {
                Directory.CreateDirectory(scenesDir);
            }

            // 2. Create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. Instantiate Track
            GameObject trackPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TrackModelPath);
            GameObject trackInstance = null;
            if (trackPrefab != null)
            {
                trackInstance = (GameObject)PrefabUtility.InstantiatePrefab(trackPrefab);
                trackInstance.name = "NeonRushTrack";
                trackInstance.transform.position = Vector3.zero;
                trackInstance.transform.rotation = Quaternion.identity;
                trackInstance.transform.localScale = Vector3.one;

                // 4. Add Colliders to Road and Barriers
                AddColliders(trackInstance);
                Debug.Log("[NeonRush] Added colliders to track and barriers.");
            }
            else
            {
                Debug.LogWarning($"[NeonRush] Track model not found at {TrackModelPath}");
            }

            // 5. Add Directional Light and Environment Lighting
            GameObject lightObj = new GameObject("Directional Light");
            Light dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.55f, 0.70f, 0.95f); // Cool twilight blue
            dirLight.intensity = 0.85f;
            dirLight.shadows = LightShadows.Soft;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Environment lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.12f, 0.16f, 0.28f);
            RenderSettings.ambientEquatorColor = new Color(0.08f, 0.10f, 0.18f);
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.03f, 0.05f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.03f, 0.04f, 0.08f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0025f;

            // 6. Instantiate Car at Starting Line
            // In Unity coordinates, start line is at X = -75, Y = 0.05, Z = -90, heading +X (Euler 0, 90, 0)
            Vector3 carStartPos = new Vector3(-75.0f, 0.05f, -90.0f);
            Quaternion carStartRot = Quaternion.Euler(0f, 90f, 0f);

            GameObject carPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CarModelPath);
            GameObject carInstance = null;
            if (carPrefab != null)
            {
                carInstance = (GameObject)PrefabUtility.InstantiatePrefab(carPrefab);
                carInstance.name = "RacingCar";
                carInstance.transform.position = carStartPos;
                carInstance.transform.rotation = carStartRot;
                carInstance.transform.localScale = Vector3.one;
                Debug.Log($"[NeonRush] RacingCar placed at start line: {carStartPos}");
            }
            else
            {
                Debug.LogWarning($"[NeonRush] Car model not found at {CarModelPath}");
            }

            // 7. Add Camera positioned behind the car
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();

            // Position 7m behind, 2.8m above car, angled down 12 degrees
            camObj.transform.position = new Vector3(-82.0f, 2.85f, -90.0f);
            camObj.transform.rotation = Quaternion.Euler(12f, 90f, 0f);
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.03f, 0.06f);

            // 8. Save the Scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[NeonRush] Successfully created and saved scene to {ScenePath}");
        }

        private static void AddColliders(GameObject trackRoot)
        {
            Transform[] allChildren = trackRoot.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allChildren)
            {
                string n = t.name.ToLower();
                // Check for Road and Road Collider
                if (n.Contains("road_collider") || n.Contains("track_road"))
                {
                    MeshFilter mf = t.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null && t.GetComponent<Collider>() == null)
                    {
                        MeshCollider mc = t.gameObject.AddComponent<MeshCollider>();
                        mc.sharedMesh = mf.sharedMesh;
                        mc.convex = false;
                        Debug.Log($"[NeonRush] Added MeshCollider to road: {t.name}");
                    }
                }
                // Check for Barriers
                else if (n.Contains("barrier"))
                {
                    MeshFilter mf = t.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null && t.GetComponent<Collider>() == null)
                    {
                        MeshCollider mc = t.gameObject.AddComponent<MeshCollider>();
                        mc.sharedMesh = mf.sharedMesh;
                        mc.convex = false;
                        Debug.Log($"[NeonRush] Added MeshCollider to barrier: {t.name}");
                    }
                }
            }
        }
    }
}
