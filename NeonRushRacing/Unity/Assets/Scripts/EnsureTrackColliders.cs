using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonRush
{
    /// <summary>
    /// Guarantees that track, road colliders, barriers, and mountain terrain
    /// always have valid, active MeshColliders bound to their runtime meshes,
    /// and ensures the car is safely grounded on the track at startup.
    /// </summary>
    [DefaultExecutionOrder(-2000)]
    public class EnsureTrackColliders : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitOnLoad()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                FixCollidersAndCar();
            };
        }

        private void Awake()
        {
            FixCollidersAndCar();
        }

        public static void FixCollidersAndCar()
        {
            // 1. Process all track objects in the active scene
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded && !Application.isPlaying) return;

            GameObject[] roots = scene.GetRootGameObjects();
            Transform trackRoadTransform = null;

            foreach (var root in roots)
            {
                Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (var t in transforms)
                {
                    string nameLower = t.name.ToLower();

                    // Dedicated road collider object
                    if (nameLower == "track_road_collider")
                    {
                        trackRoadTransform = t;
                        MeshFilter mf = t.GetComponent<MeshFilter>();
                        if (mf != null && mf.sharedMesh != null)
                        {
                            MeshCollider mc = t.GetComponent<MeshCollider>();
                            if (mc == null) mc = t.gameObject.AddComponent<MeshCollider>();
                            mc.sharedMesh = mf.sharedMesh;
                            mc.convex = false;
                            Debug.Log($"[NeonRush] Successfully bound road MeshCollider to {t.name} ({mf.sharedMesh.vertexCount} verts)");
                        }
                    }
                    // Visual road: remove MeshCollider if Track_Road_Collider exists to prevent seam conflicts
                    else if (nameLower == "track_road")
                    {
                        MeshFilter mf = t.GetComponent<MeshFilter>();
                        // If there is no Track_Road_Collider, use Track_Road as collider fallback
                        if (trackRoadTransform == null && mf != null && mf.sharedMesh != null)
                        {
                            MeshCollider mc = t.GetComponent<MeshCollider>();
                            if (mc == null) mc = t.gameObject.AddComponent<MeshCollider>();
                            mc.sharedMesh = mf.sharedMesh;
                            mc.convex = false;
                        }
                    }
                    // Barriers: Inner & Outer
                    else if (nameLower.Contains("track_barriers"))
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
                    // Mountain Ranges: ensures safe floor catching so car never falls into the void
                    else if (nameLower.Contains("mountain_ranges"))
                    {
                        MeshFilter mf = t.GetComponent<MeshFilter>();
                        if (mf != null && mf.sharedMesh != null)
                        {
                            MeshCollider mc = t.GetComponent<MeshCollider>();
                            if (mc == null) mc = t.gameObject.AddComponent<MeshCollider>();
                            mc.sharedMesh = mf.sharedMesh;
                            mc.convex = false;
                            Debug.Log($"[NeonRush] Bound valley floor MeshCollider to {t.name}");
                        }
                    }
                }
            }

            // 2. Configure PlayerCar
            GameObject car = GameObject.Find("PlayerCar");
            if (car != null)
            {
                Rigidbody rb = car.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                }

                // If car is starting in the air or falling, snap to starting line
                if (car.transform.position.y < 0.05f || car.transform.position.y > 5.0f || car.transform.position.z < -200f)
                {
                    car.transform.position = new Vector3(-75f, 0.35f, -90f);
                    car.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

                    if (rb != null)
                    {
#if UNITY_6000_0_OR_NEWER
                        rb.linearVelocity = Vector3.zero;
#else
                        rb.velocity = Vector3.zero;
#endif
                        rb.angularVelocity = Vector3.zero;
                    }
                }

                // Auto-hook real wheel transforms if any
                ArcadeCarController controller = car.GetComponent<ArcadeCarController>();
                if (controller != null)
                {
                    // Clean up bad wheel assignments
                    controller.SanitizeWheels();
                }
            }
        }
    }
}
