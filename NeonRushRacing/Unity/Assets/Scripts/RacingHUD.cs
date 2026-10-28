using UnityEngine;

namespace NeonRush
{
    /// <summary>
    /// High-performance arcade racing HUD drawn via OnGUI.
    /// Displays digital speedometer, RPM rev meter, gear indicator, nitro boost gauge,
    /// position pill (1st/4), lap counter, lap times, mini-map radar, and countdown banners.
    /// </summary>
    public class RacingHUD : MonoBehaviour
    {
        [Header("Car Reference")]
        [SerializeField] private ArcadeCarController playerCar;

        // Visual Textures (procedurally generated)
        private Texture2D bgBoxTex;
        private Texture2D solidWhiteTex;
        private GUIStyle posStyle;
        private GUIStyle speedStyle;
        private GUIStyle unitStyle;
        private GUIStyle gearStyle;
        private GUIStyle lapStyle;
        private GUIStyle timerStyle;
        private GUIStyle bannerStyle;
        private GUIStyle subBannerStyle;

        private void Awake()
        {
            // Procedural solid textures
            bgBoxTex = MakeTex(2, 2, new Color(0.04f, 0.06f, 0.12f, 0.78f));
            solidWhiteTex = MakeTex(2, 2, Color.white);
        }

        private void Start()
        {
            if (playerCar == null)
            {
                playerCar = FindFirstObjectByType<ArcadeCarController>();
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void InitStyles()
        {
            if (posStyle != null) return;

            posStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 38,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            posStyle.normal.textColor = new Color(0.0f, 1.0f, 0.95f); // Neon Cyan

            speedStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 52,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerRight
            };
            speedStyle.normal.textColor = Color.white;

            unitStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerLeft
            };
            unitStyle.normal.textColor = new Color(0.7f, 0.8f, 0.95f);

            gearStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            gearStyle.normal.textColor = new Color(1.0f, 0.8f, 0.1f); // Warm Gold

            lapStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            lapStyle.normal.textColor = Color.white;

            timerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            timerStyle.normal.textColor = new Color(0.9f, 0.9f, 1.0f);

            bannerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 80,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            bannerStyle.normal.textColor = new Color(1.0f, 0.95f, 0.2f); // Electric Yellow

            subBannerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            subBannerStyle.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            InitStyles();

            DrawPositionAndLap();
            DrawSpeedometer();
            DrawMiniMap();
            DrawCountdownAndBanners();
        }

        #region HUD Panels

        private void DrawPositionAndLap()
        {
            float sw = Screen.width;
            float sh = Screen.height;

            // Top-Left Panel
            Rect panelRect = new Rect(24, 20, 240, 95);
            GUI.DrawTexture(panelRect, bgBoxTex);

            RaceManager rm = RaceManager.Instance;
            int rank = rm != null ? rm.PlayerRank : 1;
            int total = rm != null ? rm.TotalRacers : 4;
            string rankSuffix = GetRankSuffix(rank);

            // Position (e.g. "1ST / 4")
            GUI.Label(new Rect(38, 22, 220, 48), $"{rank}{rankSuffix} <size=24><color=#88bbff>/ {total}</color></size>", posStyle);

            // Lap Counter (e.g. "LAP 1 / 3")
            string lapStr = rm != null ? rm.GetPlayerLapDisplay() : "1 / 3";
            GUI.Label(new Rect(38, 70, 220, 34), $"LAP  {lapStr}", lapStyle);

            // Top-Center Lap Time
            if (rm != null)
            {
                Rect timerRect = new Rect(sw * 0.5f - 110, 20, 220, 45);
                GUI.DrawTexture(timerRect, bgBoxTex);
                string timeStr = FormatTime(rm.CurrentLapTime);
                GUI.Label(timerRect, $"TIME: {timeStr}", timerStyle);
            }
        }

        private void DrawSpeedometer()
        {
            float sw = Screen.width;
            float sh = Screen.height;

            float speedKmh = playerCar != null ? playerCar.CurrentSpeedKmh : 0f;
            float speedRatio = playerCar != null ? playerCar.SpeedRatio : 0f;
            float boostNorm = playerCar != null ? playerCar.BoostNormalized : 1f;

            // Bottom-Right Speedometer Cluster
            Rect clusterRect = new Rect(sw - 300, sh - 150, 276, 126);
            GUI.DrawTexture(clusterRect, bgBoxTex);

            // Speed number + KM/H label
            int speedInt = Mathf.RoundToInt(speedKmh);
            GUI.Label(new Rect(sw - 300, sh - 160, 160, 65), $"{speedInt}", speedStyle);
            GUI.Label(new Rect(sw - 134, sh - 135, 100, 32), "KM/H", unitStyle);

            // Gear Indicator
            string gearStr = GetGearString(speedKmh);
            GUI.Label(new Rect(sw - 75, sh - 156, 45, 34), gearStr, gearStyle);

            // RPM / Rev Meter Bar
            float barX = sw - 286;
            float barY = sh - 76;
            float barW = 248;
            float barH = 12;

            // Bar background
            GUI.color = new Color(0.15f, 0.2f, 0.3f, 0.8f);
            GUI.DrawTexture(new Rect(barX, barY, barW, barH), solidWhiteTex);

            // Filled rev bar (Cyan to Red at redline)
            Color revColor = speedRatio > 0.85f ? Color.red : (speedRatio > 0.65f ? Color.yellow : Color.cyan);
            GUI.color = revColor;
            GUI.DrawTexture(new Rect(barX, barY, barW * speedRatio, barH), solidWhiteTex);

            // Boost Meter Bar
            float boostY = sh - 54;
            GUI.color = new Color(0.1f, 0.15f, 0.25f, 0.8f);
            GUI.DrawTexture(new Rect(barX, boostY, barW, barH), solidWhiteTex);

            // Filled boost bar
            GUI.color = new Color(0.0f, 0.9f, 1.0f); // Bright electric cyan
            GUI.DrawTexture(new Rect(barX, boostY, barW * boostNorm, barH), solidWhiteTex);

            // Boost Label
            GUI.color = Color.white;
            GUI.Label(new Rect(barX, boostY - 14, 100, 16), "<size=11>NITRO BOOST (SHIFT)</size>", unitStyle);
        }

        private void DrawMiniMap()
        {
            float sw = Screen.width;
            Rect mapRect = new Rect(sw - 165, 20, 145, 145);
            GUI.DrawTexture(mapRect, bgBoxTex);

            // Radar Map bounds (NeonRushTrack is approx X: [-170, 170], Z: [-100, 120])
            float minX = -175f, maxX = 175f;
            float minZ = -105f, maxZ = 125f;

            Vector2 WorldToMap(Vector3 worldPos)
            {
                float nx = (worldPos.x - minX) / (maxX - minX);
                float ny = 1f - (worldPos.z - minZ) / (maxZ - minZ); // Invert Z for screen Y
                return new Vector2(mapRect.x + nx * (mapRect.width - 16) + 8, mapRect.y + ny * (mapRect.height - 16) + 8);
            }

            // Draw track waypoints as dot loop
            if (TrackWaypoints.Points != null && TrackWaypoints.Points.Length > 0)
            {
                GUI.color = new Color(0.35f, 0.45f, 0.65f, 0.6f);
                for (int i = 0; i < TrackWaypoints.Points.Length; i += 3)
                {
                    Vector2 pt = WorldToMap(TrackWaypoints.Points[i]);
                    GUI.DrawTexture(new Rect(pt.x - 1, pt.y - 1, 3, 3), solidWhiteTex);
                }
            }

            // Draw Rival AI blips
            RaceManager rm = RaceManager.Instance;
            if (rm != null)
            {
                foreach (var r in rm.Leaderboard)
                {
                    if (r.Transform == null || r.IsPlayer) continue;
                    Vector2 aiPt = WorldToMap(r.Transform.position);
                    GUI.color = r.ThemeColor;
                    GUI.DrawTexture(new Rect(aiPt.x - 3, aiPt.y - 3, 6, 6), solidWhiteTex);
                }
            }

            // Draw Player blip (Bright Ruby / Magenta dot with outline)
            if (playerCar != null)
            {
                Vector2 playerPt = WorldToMap(playerCar.transform.position);
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(playerPt.x - 5, playerPt.y - 5, 10, 10), solidWhiteTex);
                GUI.color = new Color(1f, 0.15f, 0.3f);
                GUI.DrawTexture(new Rect(playerPt.x - 4, playerPt.y - 4, 8, 8), solidWhiteTex);
            }

            GUI.color = Color.white;
        }

        private void DrawCountdownAndBanners()
        {
            float sw = Screen.width;
            float sh = Screen.height;

            RaceManager rm = RaceManager.Instance;
            if (rm == null) return;

            // Countdown banner (3, 2, 1, GO!)
            if (rm.CurrentPhase == RacePhase.Countdown && !string.IsNullOrEmpty(rm.CountdownDisplay))
            {
                Rect centerRect = new Rect(sw * 0.5f - 200, sh * 0.35f, 400, 100);
                // Shadow
                GUI.color = new Color(0f, 0f, 0f, 0.7f);
                GUI.Label(new Rect(centerRect.x + 4, centerRect.y + 4, centerRect.width, centerRect.height), rm.CountdownDisplay, bannerStyle);
                // Glow text
                GUI.color = rm.CountdownDisplay == "GO!" ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.9f, 0.1f);
                GUI.Label(centerRect, rm.CountdownDisplay, bannerStyle);
                GUI.color = Color.white;
            }

            // Race Finished Banner
            if (rm.CurrentPhase == RacePhase.Finished)
            {
                Rect finishRect = new Rect(sw * 0.5f - 300, sh * 0.30f, 600, 180);
                GUI.DrawTexture(finishRect, bgBoxTex);

                GUI.color = new Color(1f, 0.85f, 0.1f);
                GUI.Label(new Rect(finishRect.x, finishRect.y + 15, finishRect.width, 70), "RACE FINISHED!", bannerStyle);

                int finalRank = rm.PlayerRank;
                string rankSuffix = GetRankSuffix(finalRank);
                string placeColor = finalRank == 1 ? "#FFD700" : (finalRank == 2 ? "#C0C0C0" : "#CD7F32");
                GUI.color = Color.white;
                GUI.Label(new Rect(finishRect.x, finishRect.y + 90, finishRect.width, 50),
                    $"<color={placeColor}>{finalRank}{rankSuffix} PLACE</color>  -  TOTAL TIME: {FormatTime(rm.TotalRaceTime)}", subBannerStyle);
            }
        }

        #endregion

        #region Helpers

        private string GetRankSuffix(int rank)
        {
            switch (rank)
            {
                case 1: return "ST";
                case 2: return "ND";
                case 3: return "RD";
                default: return "TH";
            }
        }

        private string GetGearString(float speedKmh)
        {
            if (playerCar != null && playerCar.IsReversing) return "R";
            if (speedKmh < 2f) return "N";
            if (speedKmh < 45f) return "1";
            if (speedKmh < 85f) return "2";
            if (speedKmh < 125f) return "3";
            if (speedKmh < 165f) return "4";
            if (speedKmh < 205f) return "5";
            return "6";
        }

        private string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            float remSeconds = seconds % 60f;
            return $"{minutes:00}:{remSeconds:00.00}";
        }

        #endregion
    }
}

