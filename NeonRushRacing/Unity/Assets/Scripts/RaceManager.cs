using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonRush
{
    public enum RacePhase
    {
        Countdown,
        Racing,
        Finished
    }

    public class RacerEntry : IComparable<RacerEntry>
    {
        public string Name;
        public Color ThemeColor;
        public bool IsPlayer;
        public Transform Transform;
        public int CurrentLap = 1;
        public int LastWaypointIndex;
        public float LapProgress;
        public float OverallScore;
        public int Rank = 1;

        public int CompareTo(RacerEntry other)
        {
            // Highest overall score is 1st place
            return other.OverallScore.CompareTo(this.OverallScore);
        }
    }

    /// <summary>
    /// Central manager for the Neon Rush Grand Prix.
    /// Manages starting countdown, 3-lap progression, real-time leaderboard rankings,
    /// and AI rival coordination.
    /// </summary>
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        [Header("Race Configuration")]
        [SerializeField] private int totalLaps = 3;
        [SerializeField] private float countdownDuration = 3.5f;

        [Header("Racers")]
        [SerializeField] private ArcadeCarController playerCar;
        [SerializeField] private List<AICarController> aiCars = new List<AICarController>();

        // State
        public RacePhase CurrentPhase { get; private set; } = RacePhase.Countdown;
        public int TotalLaps => totalLaps;
        public float CountdownTimer { get; private set; }
        public float CurrentLapTime { get; private set; }
        public float BestLapTime { get; private set; } = float.MaxValue;
        public float TotalRaceTime { get; private set; }
        public int PlayerRank { get; private set; } = 1;
        public int TotalRacers => racers.Count;
        public string CountdownDisplay { get; private set; } = "3";
        public List<RacerEntry> Leaderboard => racers;

        private List<RacerEntry> racers = new List<RacerEntry>();
        private RacerEntry playerEntry;
        private bool hasStartedRace;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            DiscoverRacers();
            SetupStartingGrid();
            CountdownTimer = countdownDuration;
            CurrentPhase = RacePhase.Countdown;
            LockControls(true);
        }

        private void DiscoverRacers()
        {
            racers.Clear();

            // Find player car
            if (playerCar == null)
            {
                playerCar = FindFirstObjectByType<ArcadeCarController>();
            }

            if (playerCar != null)
            {
                playerEntry = new RacerEntry
                {
                    Name = "PLAYER",
                    ThemeColor = new Color(1f, 0.15f, 0.25f), // Ruby Metallic
                    IsPlayer = true,
                    Transform = playerCar.transform,
                    CurrentLap = 1,
                    LastWaypointIndex = TrackWaypoints.GetClosestIndex(playerCar.transform.position)
                };
                racers.Add(playerEntry);
            }

            // Find AI cars
            AICarController[] foundAIs = FindObjectsByType<AICarController>(FindObjectsSortMode.None);
            aiCars.Clear();
            foreach (var ai in foundAIs)
            {
                aiCars.Add(ai);
                racers.Add(new RacerEntry
                {
                    Name = string.IsNullOrEmpty(ai.driverName) ? ai.name : ai.driverName,
                    ThemeColor = ai.carColor,
                    IsPlayer = false,
                    Transform = ai.transform,
                    CurrentLap = 1,
                    LastWaypointIndex = TrackWaypoints.GetClosestIndex(ai.transform.position)
                });
            }

            Debug.Log($"[RaceManager] Registered {racers.Count} racers ({aiCars.Count} AI rivals).");
        }

        private void SetupStartingGrid()
        {
            // Grid Slots:
            // Slot 1 (Player):   (-75.0, 0.35, -90.0)
            // Slot 2 (AI 1):     (-75.0, 0.35, -86.2)
            // Slot 3 (AI 2):     (-82.0, 0.35, -90.0)
            // Slot 4 (AI 3):     (-82.0, 0.35, -86.2)
            Vector3[] gridPositions = new Vector3[]
            {
                new Vector3(-75.0f, 0.35f, -90.0f),
                new Vector3(-75.0f, 0.35f, -86.2f),
                new Vector3(-82.0f, 0.35f, -90.0f),
                new Vector3(-82.0f, 0.35f, -86.2f)
            };
            Quaternion gridRot = Quaternion.Euler(0f, 90f, 0f);

            if (playerCar != null)
            {
                playerCar.transform.position = gridPositions[0];
                playerCar.transform.rotation = gridRot;
                Rigidbody prb = playerCar.GetComponent<Rigidbody>();
                if (prb != null)
                {
                    prb.linearVelocity = Vector3.zero;
                    prb.angularVelocity = Vector3.zero;
                }
            }

            for (int i = 0; i < aiCars.Count && i < 3; i++)
            {
                Vector3 pos = gridPositions[i + 1];
                aiCars[i].transform.position = pos;
                aiCars[i].transform.rotation = gridRot;
                Rigidbody arb = aiCars[i].GetComponent<Rigidbody>();
                if (arb != null)
                {
                    arb.linearVelocity = Vector3.zero;
                    arb.angularVelocity = Vector3.zero;
                }
            }
        }

        private void Update()
        {
            switch (CurrentPhase)
            {
                case RacePhase.Countdown:
                    UpdateCountdown();
                    break;
                case RacePhase.Racing:
                    UpdateRacing();
                    break;
                case RacePhase.Finished:
                    UpdateFinished();
                    break;
            }

            UpdateLeaderboard();
        }

        private void UpdateCountdown()
        {
            CountdownTimer -= Time.deltaTime;

            if (CountdownTimer > 2.5f) CountdownDisplay = "3";
            else if (CountdownTimer > 1.5f) CountdownDisplay = "2";
            else if (CountdownTimer > 0.5f) CountdownDisplay = "1";
            else if (CountdownTimer > -0.7f) CountdownDisplay = "GO!";
            else CountdownDisplay = "";

            if (CountdownTimer <= 0.5f && !hasStartedRace)
            {
                hasStartedRace = true;
                LockControls(false);
                CurrentPhase = RacePhase.Racing;
            }
        }

        private void UpdateRacing()
        {
            CurrentLapTime += Time.deltaTime;
            TotalRaceTime += Time.deltaTime;

            // Track waypoint progress and laps for each racer
            foreach (var r in racers)
            {
                if (r.Transform == null) continue;

                int closestIdx = TrackWaypoints.GetClosestIndex(r.Transform.position);

                // Lap completion check: wrapped from near end (> 150) to start (< 25)
                if (r.LastWaypointIndex > 150 && closestIdx < 25)
                {
                    if (r.IsPlayer)
                    {
                        if (CurrentLapTime < BestLapTime) BestLapTime = CurrentLapTime;
                        CurrentLapTime = 0f;
                    }

                    r.CurrentLap++;

                    if (r.IsPlayer && r.CurrentLap > totalLaps)
                    {
                        TriggerFinish();
                        return;
                    }
                }

                r.LastWaypointIndex = closestIdx;
                r.LapProgress = (float)closestIdx / TrackWaypoints.Count;
                r.OverallScore = (r.CurrentLap * 1000f) + closestIdx;
            }
        }

        private void UpdateFinished()
        {
            TotalRaceTime += Time.deltaTime;
        }

        private void UpdateLeaderboard()
        {
            racers.Sort();
            for (int i = 0; i < racers.Count; i++)
            {
                racers[i].Rank = i + 1;
                if (racers[i].IsPlayer)
                {
                    PlayerRank = i + 1;
                }
            }
        }

        private void TriggerFinish()
        {
            CurrentPhase = RacePhase.Finished;
            Debug.Log($"[RaceManager] Race Finished! Player finished in rank #{PlayerRank}!");
        }

        private void LockControls(bool locked)
        {
            if (playerCar != null)
            {
                playerCar.ControlsEnabled = !locked;
            }

            foreach (var ai in aiCars)
            {
                if (ai != null)
                {
                    ai.ControlsEnabled = !locked;
                }
            }
        }

        public string GetPlayerLapDisplay()
        {
            int lap = playerEntry != null ? Mathf.Min(playerEntry.CurrentLap, totalLaps) : 1;
            return $"{lap} / {totalLaps}";
        }
    }
}
