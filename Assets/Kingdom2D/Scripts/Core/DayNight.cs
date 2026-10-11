using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Gün/gece döngüsü: sayaç, evreler ve olaylar.</summary>
    public class DayNight : MonoBehaviour
    {
        GameConfig cfg;

        public int Day = 1;
        public float Clock;                 // döngü başından beri geçen saniye
        public DayPhase Phase = DayPhase.Dawn;
        public bool BloodMoon;
        public float Darkness;              // 0 = tam gündüz, 1 = tam gece

        public System.Action<int> OnNewDay;
        public System.Action OnNightStart;
        public System.Action OnNightEnd;

        bool nightActive;

        public bool IsNight { get { return Phase == DayPhase.Night; } }

        public void Init(GameConfig config, int startDay = 1)
        {
            cfg = config;
            Day = startDay;
            Clock = 0f;
            Phase = DayPhase.Dawn;
            BloodMoon = IsBloodMoonDay(Day);
            nightActive = false;
            Recompute();
        }

        bool IsBloodMoonDay(int d)
        {
            return cfg.bloodMoonEvery > 0 && d % cfg.bloodMoonEvery == 0;
        }

        void Update()
        {
            if (cfg == null) return;
            if (GameManager.I != null && GameManager.I.State != GameState.Playing) return;

            Clock += Time.deltaTime;
            if (Clock >= cfg.CycleLength)
            {
                Clock -= cfg.CycleLength;
                Day++;
                BloodMoon = IsBloodMoonDay(Day);
                if (OnNewDay != null) OnNewDay(Day);
                Sfx.Play("newday", 0.7f);
            }
            Recompute();

            if (Phase == DayPhase.Night && !nightActive)
            {
                nightActive = true;
                if (OnNightStart != null) OnNightStart();
            }
            else if (Phase != DayPhase.Night && nightActive)
            {
                nightActive = false;
                if (OnNightEnd != null) OnNightEnd();
            }
        }

        void Recompute()
        {
            float dawn = cfg.dawnLength;
            float day = dawn + cfg.dayLength;
            float dusk = day + cfg.duskLength;

            if (Clock < dawn)
            {
                Phase = DayPhase.Dawn;
                Darkness = 1f - Util.Ramp(Clock, 0f, dawn);
            }
            else if (Clock < day)
            {
                Phase = DayPhase.Day;
                Darkness = 0f;
            }
            else if (Clock < dusk)
            {
                Phase = DayPhase.Dusk;
                Darkness = Util.Ramp(Clock, day, dusk);
            }
            else
            {
                Phase = DayPhase.Night;
                Darkness = 1f;
            }
        }

        /// <summary>Gecenin ne kadarı geçti (0..1). Gündüzse 0.</summary>
        public float NightProgress
        {
            get
            {
                if (cfg == null || Phase != DayPhase.Night) return 0f;
                float nightStart = cfg.dawnLength + cfg.dayLength + cfg.duskLength;
                return Util.Ramp(Clock, nightStart, cfg.CycleLength);
            }
        }

        /// <summary>Gündüzün ne kadarı geçti (0..1): gece yaklaşıyor göstergesi.</summary>
        public float DayProgress
        {
            get
            {
                if (cfg == null) return 0f;
                float nightStart = cfg.dawnLength + cfg.dayLength + cfg.duskLength;
                if (Phase == DayPhase.Night) return 1f;
                return Util.Ramp(Clock, 0f, nightStart);
            }
        }

        /// <summary>Güneş/ay yayı için 0..1 (0 = doğu ufku, 1 = batı ufku).</summary>
        public float CelestialT
        {
            get
            {
                if (cfg == null) return 0.5f;
                float lightSpan = cfg.dawnLength + cfg.dayLength + cfg.duskLength;
                if (Phase == DayPhase.Night)
                    return Util.Ramp(Clock, lightSpan, cfg.CycleLength);
                return Util.Ramp(Clock, 0f, lightSpan);
            }
        }
    }
}
