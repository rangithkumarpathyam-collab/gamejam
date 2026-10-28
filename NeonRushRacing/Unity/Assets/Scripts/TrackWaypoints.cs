using UnityEngine;

namespace NeonRush
{
    /// <summary>
    /// Circuit waypoints extracted from the 180 continuous rings of NeonRushTrack.
    /// Provides spatial lookups for AI navigation, lap tracking, and leaderboard rankings.
    /// </summary>
    public static class TrackWaypoints
    {
        public const int Count = 180;

        public static readonly Vector3[] Points = new Vector3[]
        {
            new Vector3(-80.00f, 0.35f, -90.00f),
            new Vector3(-74.73f, 0.35f, -90.53f),
            new Vector3(-69.46f, 0.35f, -91.06f),
            new Vector3(-64.20f, 0.35f, -91.59f),
            new Vector3(-58.93f, 0.35f, -92.12f),
            new Vector3(-53.66f, 0.35f, -92.65f),
            new Vector3(-48.39f, 0.35f, -93.18f),
            new Vector3(-43.13f, 0.35f, -93.71f),
            new Vector3(-37.86f, 0.35f, -94.24f),
            new Vector3(-32.59f, 0.35f, -94.77f),
            new Vector3(-27.32f, 0.35f, -95.30f),
            new Vector3(-22.06f, 0.35f, -95.83f),
            new Vector3(-16.79f, 0.35f, -96.36f),
            new Vector3(-11.52f, 0.35f, -96.89f),
            new Vector3(-6.26f, 0.35f, -97.42f),
            new Vector3(-0.99f, 0.35f, -97.95f),
            new Vector3(4.28f, 0.35f, -98.48f),
            new Vector3(9.55f, 0.35f, -99.01f),
            new Vector3(14.81f, 0.35f, -99.54f),
            new Vector3(20.08f, 0.35f, -100.07f),
            new Vector3(25.35f, 0.35f, -100.60f),
            new Vector3(30.61f, 0.35f, -101.13f),
            new Vector3(35.88f, 0.35f, -101.66f),
            new Vector3(41.15f, 0.35f, -102.19f),
            new Vector3(46.42f, 0.35f, -102.72f),
            new Vector3(51.68f, 0.35f, -103.25f),
            new Vector3(56.95f, 0.35f, -103.78f),
            new Vector3(62.22f, 0.35f, -104.31f),
            new Vector3(67.48f, 0.35f, -104.84f),
            new Vector3(72.75f, 0.35f, -105.37f),
            new Vector3(78.02f, 0.45f, -105.90f),
            new Vector3(83.29f, 0.55f, -106.43f),
            new Vector3(88.55f, 0.65f, -106.96f),
            new Vector3(93.82f, 0.75f, -107.49f),
            new Vector3(99.09f, 0.85f, -108.02f),
            new Vector3(104.35f, 0.95f, -108.55f),
            new Vector3(109.62f, 1.05f, -109.08f),
            new Vector3(114.89f, 1.15f, -109.61f),
            new Vector3(120.15f, 1.25f, -110.14f),
            new Vector3(125.42f, 1.35f, -110.67f),
            new Vector3(130.69f, 1.45f, -111.20f),
            new Vector3(135.96f, 1.55f, -111.73f),
            new Vector3(141.22f, 1.65f, -112.26f),
            new Vector3(146.49f, 1.75f, -112.79f),
            new Vector3(151.76f, 1.85f, -113.32f),
            new Vector3(155.78f, 1.95f, -33.91f),
            new Vector3(158.00f, 2.05f, -25.00f),
            new Vector3(160.00f, 2.15f, -15.00f),
            new Vector3(161.50f, 2.25f, -5.00f),
            new Vector3(162.00f, 2.30f, 5.00f),
            new Vector3(161.00f, 2.25f, 15.00f),
            new Vector3(158.50f, 2.15f, 25.00f),
            new Vector3(154.00f, 2.05f, 35.00f),
            new Vector3(148.00f, 1.95f, 45.00f),
            new Vector3(140.00f, 1.85f, 55.00f),
            new Vector3(130.00f, 1.75f, 65.00f),
            new Vector3(118.00f, 1.65f, 75.00f),
            new Vector3(105.00f, 1.55f, 82.00f),
            new Vector3(92.00f, 1.50f, 87.00f),
            new Vector3(78.00f, 1.50f, 90.00f),
            new Vector3(64.00f, 1.50f, 92.00f),
            new Vector3(50.00f, 1.50f, 93.00f),
            new Vector3(36.00f, 1.50f, 92.50f),
            new Vector3(28.75f, 1.50f, 90.31f),
            new Vector3(16.00f, 1.45f, 85.00f),
            new Vector3(5.00f, 1.40f, 78.00f),
            new Vector3(-4.00f, 1.35f, 70.00f),
            new Vector3(-12.00f, 1.30f, 62.00f),
            new Vector3(-20.00f, 1.25f, 55.00f),
            new Vector3(-28.00f, 1.20f, 50.00f),
            new Vector3(-38.00f, 1.15f, 48.00f),
            new Vector3(-48.00f, 1.10f, 49.00f),
            new Vector3(-58.00f, 1.05f, 53.00f),
            new Vector3(-68.00f, 1.00f, 60.00f),
            new Vector3(-78.00f, 0.95f, 70.00f),
            new Vector3(-88.00f, 0.90f, 80.00f),
            new Vector3(-98.00f, 0.85f, 88.00f),
            new Vector3(-108.00f, 0.80f, 93.00f),
            new Vector3(-118.00f, 0.75f, 95.00f),
            new Vector3(-128.00f, 0.70f, 93.00f),
            new Vector3(-138.00f, 0.65f, 88.00f),
            new Vector3(-148.52f, 0.60f, 56.17f),
            new Vector3(-153.00f, 0.55f, 45.00f),
            new Vector3(-156.00f, 0.50f, 32.00f),
            new Vector3(-157.50f, 0.45f, 18.00f),
            new Vector3(-157.00f, 0.40f, 4.00f),
            new Vector3(-154.50f, 0.35f, -10.00f),
            new Vector3(-150.00f, 0.35f, -24.00f),
            new Vector3(-143.00f, 0.35f, -38.00f),
            new Vector3(-134.00f, 0.35f, -50.00f),
            new Vector3(-123.00f, 0.35f, -62.00f),
            new Vector3(-110.00f, 0.35f, -72.00f),
            new Vector3(-96.00f, 0.35f, -80.00f),
            new Vector3(-88.00f, 0.35f, -86.00f),
        };

        public static int GetClosestIndex(Vector3 position)
        {
            int bestIdx = 0;
            float bestDistSq = float.MaxValue;
            for (int i = 0; i < Points.Length; i++)
            {
                float d = (Points[i] - position).sqrMagnitude;
                if (d < bestDistSq)
                {
                    bestDistSq = d;
                    bestIdx = i;
                }
            }
            return bestIdx;
        }

        public static Vector3 GetPoint(int index)
        {
            if (index < 0) index = (index % Points.Length + Points.Length) % Points.Length;
            else if (index >= Points.Length) index = index % Points.Length;
            return Points[index];
        }

        public static Vector3 GetDirection(int index)
        {
            Vector3 curr = GetPoint(index);
            Vector3 next = GetPoint(index + 1);
            Vector3 dir = (next - curr);
            dir.y = 0f;
            return dir.normalized;
        }

        public static float GetProgress(Vector3 position)
        {
            int idx = GetClosestIndex(position);
            return (float)idx / Points.Length;
        }
    }
}

