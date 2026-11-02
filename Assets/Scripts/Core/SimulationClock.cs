using System;

namespace Dewabhumi.Core
{
    public enum Season
    {
        Grishma = 0, // Pre-monsoon dry season
        Varsha = 1,  // Monsoon deluge
        Sharad = 2,  // Autumn harvest
        Hemanta = 3  // Cold winter
    }

    /// <summary>
    /// Pure C# deterministic simulation clock running at locked 10 Hz (0.1s / tick).
    /// Decoupled from rendering frame rate.
    /// Strictly complies with docs/design/SIMULATION_ARCHITECTURE.md.
    /// </summary>
    public class SimulationClock
    {
        public const int TicksPerSecond = 10;
        public const float SecondsPerTick = 1.0f / TicksPerSecond;

        public const int TicksPerHour = TicksPerSecond * 60;          // 600 ticks
        public const int HoursPerDay = 24;
        public const int TicksPerDay = TicksPerHour * HoursPerDay;    // 14,400 ticks
        public const int DaysPerSeason = 30;
        public const int DaysPerYear = DaysPerSeason * 4;             // 120 days

        public ulong TotalElapsedTicks { get; private set; }
        public int CurrentYear { get; private set; } = 1;
        public Season CurrentSeason { get; private set; } = Season.Grishma;
        public int DayOfSeason { get; private set; } = 1;
        public int HourOfDay { get; private set; } = 6; // Starts at dawn (6:00 AM)
        public int MinuteOfHour { get; private set; } = 0;

        public event Action<ulong> OnTick;
        public event Action<int> OnHourPassed;
        public event Action<int> OnDayPassed;
        public event Action<Season> OnSeasonChanged;
        public event Action<int> OnYearPassed;

        public float TimeScale { get; set; } = 1.0f; // 0 (paused), 1x, 2x, 4x
        private float _accumulator = 0f;

        /// <summary>
        /// Accumulates real-world frame delta time and executes discrete fixed ticks.
        /// </summary>
        public void Update(float realDeltaTime)
        {
            if (TimeScale <= 0f) return;

            _accumulator += realDeltaTime * TimeScale;
            while (_accumulator >= SecondsPerTick)
            {
                AdvanceSingleTick();
                _accumulator -= SecondsPerTick;
            }
        }

        /// <summary>
        /// Advances the simulation by exactly one deterministic tick.
        /// </summary>
        public void AdvanceSingleTick()
        {
            TotalElapsedTicks++;

            // Calculate calendar progression
            ulong dayTicks = TotalElapsedTicks % (ulong)TicksPerDay;
            HourOfDay = (int)(dayTicks / (ulong)TicksPerHour);
            MinuteOfHour = (int)((dayTicks % (ulong)TicksPerHour) / (ulong)TicksPerSecond);

            OnTick?.Invoke(TotalElapsedTicks);

            // Minute boundaries
            if (dayTicks % (ulong)TicksPerHour == 0)
            {
                OnHourPassed?.Invoke(HourOfDay);
            }

            if (dayTicks == 0)
            {
                DayOfSeason++;
                if (DayOfSeason > DaysPerSeason)
                {
                    DayOfSeason = 1;
                    int nextSeason = ((int)CurrentSeason + 1) % 4;
                    CurrentSeason = (Season)nextSeason;
                    OnSeasonChanged?.Invoke(CurrentSeason);

                    if (CurrentSeason == Season.Grishma)
                    {
                        CurrentYear++;
                        OnYearPassed?.Invoke(CurrentYear);
                    }
                }
                OnDayPassed?.Invoke(DayOfSeason);
            }
        }

        public void SetState(ulong elapsedTicks, int year, Season season, int dayOfSeason)
        {
            TotalElapsedTicks = elapsedTicks;
            CurrentYear = year;
            CurrentSeason = season;
            DayOfSeason = dayOfSeason;
            _accumulator = 0f;
        }
    }
}
