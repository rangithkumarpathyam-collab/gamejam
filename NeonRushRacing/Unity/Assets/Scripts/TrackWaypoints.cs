using UnityEngine;

namespace NeonRush
{
    /// <summary>
    /// Circuit waypoints extracted from the 180 continuous rings of NeonRushTrack.
    /// Provides spatial lookups for AI navigation, lap tracking, and leaderboard rankings.
    /// </summary>
    public static class TrackWaypoints
    {
        public static int Count => Points.Length;

        public static readonly Vector3[] Points = new Vector3[]
        {
            new Vector3(-77.37f, 0.35f, 90.26f), // Ring 0
            new Vector3(-71.86f, 0.35f, 90.70f), // Ring 1
            new Vector3(-65.90f, 0.35f, 90.96f), // Ring 2
            new Vector3(-59.55f, 0.35f, 91.08f), // Ring 3
            new Vector3(-52.90f, 0.35f, 91.09f), // Ring 4
            new Vector3(-46.00f, 0.35f, 91.00f), // Ring 5
            new Vector3(-38.93f, 0.35f, 90.85f), // Ring 6
            new Vector3(-31.75f, 0.35f, 90.66f), // Ring 7
            new Vector3(-24.54f, 0.35f, 90.45f), // Ring 8
            new Vector3(-17.37f, 0.35f, 90.26f), // Ring 9
            new Vector3(-10.30f, 0.35f, 90.11f), // Ring 10
            new Vector3(-3.40f, 0.35f, 90.02f), // Ring 11
            new Vector3(3.38f, 0.35f, 90.03f), // Ring 12
            new Vector3(10.22f, 0.35f, 90.15f), // Ring 13
            new Vector3(17.19f, 0.34f, 90.35f), // Ring 14
            new Vector3(24.24f, 0.33f, 90.60f), // Ring 15
            new Vector3(31.32f, 0.33f, 90.88f), // Ring 16
            new Vector3(38.36f, 0.32f, 91.13f), // Ring 17
            new Vector3(45.33f, 0.32f, 91.33f), // Ring 18
            new Vector3(52.17f, 0.31f, 91.45f), // Ring 19
            new Vector3(58.83f, 0.31f, 91.44f), // Ring 20
            new Vector3(65.26f, 0.32f, 91.28f), // Ring 21
            new Vector3(71.39f, 0.33f, 90.93f), // Ring 22
            new Vector3(77.19f, 0.34f, 90.35f), // Ring 23
            new Vector3(82.64f, 0.36f, 89.56f), // Ring 24
            new Vector3(87.79f, 0.39f, 88.65f), // Ring 25
            new Vector3(92.69f, 0.42f, 87.64f), // Ring 26
            new Vector3(97.35f, 0.46f, 86.52f), // Ring 27
            new Vector3(101.80f, 0.50f, 85.26f), // Ring 28
            new Vector3(106.05f, 0.55f, 83.86f), // Ring 29
            new Vector3(110.11f, 0.59f, 82.30f), // Ring 30
            new Vector3(114.01f, 0.64f, 80.55f), // Ring 31
            new Vector3(117.77f, 0.69f, 78.60f), // Ring 32
            new Vector3(121.39f, 0.74f, 76.44f), // Ring 33
            new Vector3(124.90f, 0.78f, 74.04f), // Ring 34
            new Vector3(128.31f, 0.83f, 71.39f), // Ring 35
            new Vector3(131.66f, 0.87f, 68.46f), // Ring 36
            new Vector3(134.97f, 0.91f, 65.22f), // Ring 37
            new Vector3(138.23f, 0.95f, 61.68f), // Ring 38
            new Vector3(141.40f, 1.00f, 57.88f), // Ring 39
            new Vector3(144.45f, 1.04f, 53.86f), // Ring 40
            new Vector3(147.35f, 1.08f, 49.65f), // Ring 41
            new Vector3(150.05f, 1.12f, 45.28f), // Ring 42
            new Vector3(152.53f, 1.16f, 40.79f), // Ring 43
            new Vector3(154.74f, 1.20f, 36.21f), // Ring 44
            new Vector3(156.66f, 1.25f, 31.58f), // Ring 45
            new Vector3(158.26f, 1.29f, 26.94f), // Ring 46
            new Vector3(159.48f, 1.33f, 22.31f), // Ring 47
            new Vector3(160.33f, 1.37f, 17.64f), // Ring 48
            new Vector3(160.83f, 1.41f, 12.82f), // Ring 49
            new Vector3(161.01f, 1.45f, 7.79f), // Ring 50
            new Vector3(160.88f, 1.50f, 2.62f), // Ring 51
            new Vector3(160.46f, 1.54f, -2.65f), // Ring 52
            new Vector3(159.78f, 1.58f, -7.96f), // Ring 53
            new Vector3(158.85f, 1.62f, -13.27f), // Ring 54
            new Vector3(157.68f, 1.66f, -18.51f), // Ring 55
            new Vector3(156.29f, 1.70f, -23.64f), // Ring 56
            new Vector3(154.71f, 1.75f, -28.61f), // Ring 57
            new Vector3(152.94f, 1.79f, -33.36f), // Ring 58
            new Vector3(151.01f, 1.83f, -37.83f), // Ring 59
            new Vector3(148.89f, 1.87f, -42.05f), // Ring 60
            new Vector3(146.54f, 1.92f, -46.08f), // Ring 61
            new Vector3(143.94f, 1.97f, -49.97f), // Ring 62
            new Vector3(141.10f, 2.02f, -53.72f), // Ring 63
            new Vector3(138.05f, 2.07f, -57.33f), // Ring 64
            new Vector3(134.80f, 2.12f, -60.80f), // Ring 65
            new Vector3(131.36f, 2.17f, -64.13f), // Ring 66
            new Vector3(127.76f, 2.21f, -67.33f), // Ring 67
            new Vector3(124.02f, 2.25f, -70.38f), // Ring 68
            new Vector3(120.14f, 2.29f, -73.30f), // Ring 69
            new Vector3(116.15f, 2.32f, -76.08f), // Ring 70
            new Vector3(112.06f, 2.34f, -78.72f), // Ring 71
            new Vector3(107.82f, 2.35f, -81.25f), // Ring 72
            new Vector3(103.28f, 2.36f, -83.72f), // Ring 73
            new Vector3(98.43f, 2.36f, -86.15f), // Ring 74
            new Vector3(93.34f, 2.35f, -88.49f), // Ring 75
            new Vector3(88.07f, 2.34f, -90.70f), // Ring 76
            new Vector3(82.70f, 2.33f, -92.76f), // Ring 77
            new Vector3(77.30f, 2.31f, -94.63f), // Ring 78
            new Vector3(71.93f, 2.28f, -96.28f), // Ring 79
            new Vector3(66.66f, 2.26f, -97.66f), // Ring 80
            new Vector3(61.57f, 2.23f, -98.75f), // Ring 81
            new Vector3(56.72f, 2.20f, -99.51f), // Ring 82
            new Vector3(52.18f, 2.17f, -99.90f), // Ring 83
            new Vector3(47.98f, 2.13f, -99.81f), // Ring 84
            new Vector3(44.07f, 2.09f, -99.10f), // Ring 85
            new Vector3(40.38f, 2.04f, -97.78f), // Ring 86
            new Vector3(36.89f, 1.99f, -96.00f), // Ring 87
            new Vector3(33.55f, 1.94f, -93.87f), // Ring 88
            new Vector3(30.33f, 1.88f, -91.52f), // Ring 89
            new Vector3(27.20f, 1.82f, -89.10f), // Ring 90
            new Vector3(24.12f, 1.76f, -86.72f), // Ring 91
            new Vector3(21.06f, 1.71f, -84.52f), // Ring 92
            new Vector3(17.98f, 1.66f, -82.62f), // Ring 93
            new Vector3(14.85f, 1.61f, -81.17f), // Ring 94
            new Vector3(11.63f, 1.57f, -80.28f), // Ring 95
            new Vector3(8.35f, 1.53f, -80.01f), // Ring 96
            new Vector3(5.07f, 1.50f, -80.25f), // Ring 97
            new Vector3(1.84f, 1.47f, -80.88f), // Ring 98
            new Vector3(-1.36f, 1.44f, -81.85f), // Ring 99
            new Vector3(-4.56f, 1.42f, -83.09f), // Ring 100
            new Vector3(-7.77f, 1.39f, -84.54f), // Ring 101
            new Vector3(-11.00f, 1.37f, -86.15f), // Ring 102
            new Vector3(-14.28f, 1.36f, -87.84f), // Ring 103
            new Vector3(-17.61f, 1.35f, -89.57f), // Ring 104
            new Vector3(-21.03f, 1.34f, -91.26f), // Ring 105
            new Vector3(-24.54f, 1.34f, -92.87f), // Ring 106
            new Vector3(-28.16f, 1.35f, -94.32f), // Ring 107
            new Vector3(-31.89f, 1.36f, -95.71f), // Ring 108
            new Vector3(-35.71f, 1.39f, -97.28f), // Ring 109
            new Vector3(-39.60f, 1.43f, -99.09f), // Ring 110
            new Vector3(-43.56f, 1.48f, -101.04f), // Ring 111
            new Vector3(-47.59f, 1.53f, -103.04f), // Ring 112
            new Vector3(-51.68f, 1.59f, -104.99f), // Ring 113
            new Vector3(-55.85f, 1.65f, -106.79f), // Ring 114
            new Vector3(-60.09f, 1.70f, -108.36f), // Ring 115
            new Vector3(-64.39f, 1.76f, -109.60f), // Ring 116
            new Vector3(-68.77f, 1.80f, -110.40f), // Ring 117
            new Vector3(-73.21f, 1.83f, -110.68f), // Ring 118
            new Vector3(-77.73f, 1.85f, -110.34f), // Ring 119
            new Vector3(-82.39f, 1.85f, -109.33f), // Ring 120
            new Vector3(-87.33f, 1.83f, -107.70f), // Ring 121
            new Vector3(-92.56f, 1.81f, -105.54f), // Ring 122
            new Vector3(-97.99f, 1.78f, -102.91f), // Ring 123
            new Vector3(-103.53f, 1.73f, -99.86f), // Ring 124
            new Vector3(-109.10f, 1.69f, -96.47f), // Ring 125
            new Vector3(-114.60f, 1.64f, -92.78f), // Ring 126
            new Vector3(-119.96f, 1.58f, -88.86f), // Ring 127
            new Vector3(-125.09f, 1.53f, -84.77f), // Ring 128
            new Vector3(-129.89f, 1.47f, -80.58f), // Ring 129
            new Vector3(-134.28f, 1.42f, -76.33f), // Ring 130
            new Vector3(-138.18f, 1.37f, -72.10f), // Ring 131
            new Vector3(-141.58f, 1.33f, -67.83f), // Ring 132
            new Vector3(-144.58f, 1.29f, -63.36f), // Ring 133
            new Vector3(-147.26f, 1.24f, -58.61f), // Ring 134
            new Vector3(-149.63f, 1.20f, -53.64f), // Ring 135
            new Vector3(-151.71f, 1.15f, -48.51f), // Ring 136
            new Vector3(-153.53f, 1.11f, -43.27f), // Ring 137
            new Vector3(-155.10f, 1.07f, -37.96f), // Ring 138
            new Vector3(-156.43f, 1.02f, -32.65f), // Ring 139
            new Vector3(-157.54f, 0.98f, -27.38f), // Ring 140
            new Vector3(-158.46f, 0.94f, -22.21f), // Ring 141
            new Vector3(-159.19f, 0.90f, -17.18f), // Ring 142
            new Vector3(-159.76f, 0.87f, -12.36f), // Ring 143
            new Vector3(-160.14f, 0.83f, -7.72f), // Ring 144
            new Vector3(-160.29f, 0.80f, -3.17f), // Ring 145
            new Vector3(-160.19f, 0.77f, 1.32f), // Ring 146
            new Vector3(-159.85f, 0.75f, 5.76f), // Ring 147
            new Vector3(-159.30f, 0.72f, 10.13f), // Ring 148
            new Vector3(-158.55f, 0.69f, 14.43f), // Ring 149
            new Vector3(-157.61f, 0.67f, 18.65f), // Ring 150
            new Vector3(-156.51f, 0.65f, 22.78f), // Ring 151
            new Vector3(-155.27f, 0.62f, 26.80f), // Ring 152
            new Vector3(-153.89f, 0.60f, 30.72f), // Ring 153
            new Vector3(-152.40f, 0.58f, 34.52f), // Ring 154
            new Vector3(-150.81f, 0.56f, 38.20f), // Ring 155
            new Vector3(-149.12f, 0.54f, 41.75f), // Ring 156
            new Vector3(-147.25f, 0.52f, 45.22f), // Ring 157
            new Vector3(-145.21f, 0.50f, 48.60f), // Ring 158
            new Vector3(-143.00f, 0.48f, 51.90f), // Ring 159
            new Vector3(-140.64f, 0.46f, 55.09f), // Ring 160
            new Vector3(-138.15f, 0.44f, 58.17f), // Ring 161
            new Vector3(-135.55f, 0.42f, 61.14f), // Ring 162
            new Vector3(-132.85f, 0.41f, 63.98f), // Ring 163
            new Vector3(-130.08f, 0.39f, 66.69f), // Ring 164
            new Vector3(-127.24f, 0.38f, 69.25f), // Ring 165
            new Vector3(-124.36f, 0.36f, 71.66f), // Ring 166
            new Vector3(-121.46f, 0.35f, 73.92f), // Ring 167
            new Vector3(-118.57f, 0.35f, 76.00f), // Ring 168
            new Vector3(-115.76f, 0.34f, 77.91f), // Ring 169
            new Vector3(-113.01f, 0.34f, 79.67f), // Ring 170
            new Vector3(-110.26f, 0.34f, 81.27f), // Ring 171
            new Vector3(-107.48f, 0.34f, 82.73f), // Ring 172
            new Vector3(-104.60f, 0.34f, 84.06f), // Ring 173
            new Vector3(-101.57f, 0.34f, 85.26f), // Ring 174
            new Vector3(-98.34f, 0.34f, 86.34f), // Ring 175
            new Vector3(-94.86f, 0.34f, 87.31f), // Ring 176
            new Vector3(-91.07f, 0.35f, 88.19f), // Ring 177
            new Vector3(-86.93f, 0.35f, 88.97f), // Ring 178
            new Vector3(-82.38f, 0.35f, 89.67f), // Ring 179
        };

        public static int GetClosestIndex(Vector3 position)
        {
            if (Points == null || Points.Length == 0) return 0;
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
            if (Points == null || Points.Length == 0) return Vector3.zero;
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
            if (Points == null || Points.Length == 0) return 0f;
            int idx = GetClosestIndex(position);
            return (float)idx / Points.Length;
        }
    }
}
