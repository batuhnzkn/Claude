using System.Collections.Generic;
using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Oyunun merkezi: ayarlar, kayıtlar (köylü/greed/altın listeleri), gün olayları,
    /// sorgular ve oyun sonu / yeniden başlatma.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I;

        [Tooltip("Tüm denge değerleri. Oyun çalışırken de değiştirilebilir.")]
        public GameConfig Config = new GameConfig();

        public GameState State = GameState.Playing;

        [HideInInspector] public Transform WorldRoot;
        [HideInInspector] public Transform ActorRoot;
        [HideInInspector] public Transform FxRoot;

        public DayNight Clock;
        public CameraRig Rig;
        public Hud Hud;
        public Background Bg;
        public WorldMap Map;
        public PlayerKing King;
        public BaseCamp Camp;

        public readonly List<Villager> Villagers = new List<Villager>();
        public readonly List<Greed> Greeds = new List<Greed>();
        public readonly List<CoinPickup> Coins = new List<CoinPickup>();
        public readonly List<CoinSink> Sinks = new List<CoinSink>();
        public readonly List<Structure> Structures = new List<Structure>();
        public readonly List<Wall> Walls = new List<Wall>();
        public readonly List<Critter> Critters = new List<Critter>();
        public readonly List<GreedPortal> Portals = new List<GreedPortal>();
        public readonly List<Farm> Farms = new List<Farm>();
        public readonly List<VagrantCamp> VagrantCamps = new List<VagrantCamp>();

        public string GameOverReason { get; private set; }
        public int BestDay { get; private set; }

        const string BestKey = "kingdom2d_best_day";

        void Awake()
        {
            I = this;
            BestDay = PlayerPrefs.GetInt(BestKey, 0);
        }

        void OnDestroy()
        {
            if (I == this) I = null;
        }

        public int Day { get { return Clock != null ? Clock.Day : 1; } }
        public bool IsNight { get { return Clock != null && Clock.IsNight; } }
        public float Darkness { get { return Clock != null ? Clock.Darkness : 0f; } }
        public float BaseX { get { return Camp != null ? Camp.X : 0f; } }

        void Update()
        {
            if (State == GameState.GameOver && GameInput.RestartPressed)
            {
                GameBootstrap.RequestRestart();
                return;
            }
            if (GameInput.QuitPressed)
            {
#if UNITY_EDITOR
                Debug.Log("Kingdom 2D: cikis istendi (editorde oyun durdurulmaz).");
#else
                Application.Quit();
#endif
            }
        }

        // ------------------------------------------------------------- olaylar

        public void HookClock()
        {
            if (Clock == null) return;
            Clock.OnNewDay += OnNewDay;
            Clock.OnNightStart += OnNightStart;
            Clock.OnNightEnd += OnNightEnd;
        }

        void OnNewDay(int day)
        {
            if (Hud != null) Hud.Flash(day + ". GUN", Pal.White);
            for (int i = 0; i < VagrantCamps.Count; i++)
                if (VagrantCamps[i] != null) VagrantCamps[i].Replenish();
            if (Map != null) Map.RespawnCritters();
        }

        void OnNightStart()
        {
            bool blood = Clock.BloodMoon;
            for (int i = 0; i < Portals.Count; i++)
                if (Portals[i] != null) Portals[i].BeginNight(Day, blood);

            if (Hud != null) Hud.Flash(blood ? "KANLI AY!" : "GECE GELDI", blood ? Pal.Cape : Pal.White);
            Sfx.Play(blood ? "bloodmoon" : "horn", 0.9f);
        }

        void OnNightEnd()
        {
            for (int i = 0; i < Greeds.Count; i++)
                if (Greeds[i] != null) Greeds[i].StartFleeing();
            for (int i = 0; i < Portals.Count; i++)
                if (Portals[i] != null) Portals[i].EndNight();
            if (Hud != null) Hud.Flash("SAFAK SOKTU", Pal.Gold);
        }

        public void EndGame(string reason)
        {
            if (State == GameState.GameOver) return;
            State = GameState.GameOver;
            GameOverReason = reason;
            if (Day > BestDay)
            {
                BestDay = Day;
                PlayerPrefs.SetInt(BestKey, BestDay);
                PlayerPrefs.Save();
            }
            if (Hud != null) Hud.ShowGameOver(Day, reason, BestDay);
        }

        // ------------------------------------------------------------- sorgular

        public CoinSink NearestSink(float x, float range)
        {
            CoinSink best = null;
            float bd = range;
            for (int i = Sinks.Count - 1; i >= 0; i--)
            {
                var s = Sinks[i];
                if (s == null) { Sinks.RemoveAt(i); continue; }
                if (!s.Active || s.Full) continue;
                float d = Mathf.Abs(s.X - x);
                if (d <= bd) { bd = d; best = s; }
            }
            return best;
        }

        public Villager NearestVillager(float x, float maxDist, VillagerRole role, bool onlyGround = true)
        {
            Villager best = null;
            float bd = maxDist;
            for (int i = 0; i < Villagers.Count; i++)
            {
                var v = Villagers[i];
                if (v == null || v.Captured || v.Role != role) continue;
                if (onlyGround && v.Height > 0.3f) continue;
                float d = Mathf.Abs(v.X - x);
                if (d <= bd) { bd = d; best = v; }
            }
            return best;
        }

        /// <summary>Alet almaya gitmeyen en yakın boş köylü.</summary>
        public Villager NearestFreePeasant(float x, float maxDist)
        {
            Villager best = null;
            float bd = maxDist;
            for (int i = 0; i < Villagers.Count; i++)
            {
                var v = Villagers[i];
                if (v == null || v.Captured || v.Role != VillagerRole.Peasant || v.HasPendingTool) continue;
                float d = Mathf.Abs(v.X - x);
                if (d <= bd) { bd = d; best = v; }
            }
            return best;
        }

        public Villager NearestAnyVillager(float x, float maxDist, bool groundOnly = true)
        {
            Villager best = null;
            float bd = maxDist;
            for (int i = 0; i < Villagers.Count; i++)
            {
                var v = Villagers[i];
                if (v == null || v.Captured) continue;
                if (groundOnly && v.Height > 0.3f) continue;
                float d = Mathf.Abs(v.X - x);
                if (d <= bd) { bd = d; best = v; }
            }
            return best;
        }

        public Greed NearestGreed(float x, float maxDist, bool includeFlyers = true)
        {
            Greed best = null;
            float bd = maxDist;
            for (int i = 0; i < Greeds.Count; i++)
            {
                var g = Greeds[i];
                if (g == null || !g.Alive) continue;
                if (!includeFlyers && g.Type == GreedType.Flyer) continue;
                float d = Mathf.Abs(g.X - x);
                if (d <= bd) { bd = d; best = g; }
            }
            return best;
        }

        public Critter NearestCritter(float x, float maxDist)
        {
            Critter best = null;
            float bd = maxDist;
            for (int i = 0; i < Critters.Count; i++)
            {
                var c = Critters[i];
                if (c == null || !c.Alive) continue;
                float d = Mathf.Abs(c.X - x);
                if (d <= bd) { bd = d; best = c; }
            }
            return best;
        }

        public CoinPickup NearestCoin(float x, float maxDist)
        {
            CoinPickup best = null;
            float bd = maxDist;
            for (int i = Coins.Count - 1; i >= 0; i--)
            {
                var c = Coins[i];
                if (c == null || c.Claimed) { Coins.RemoveAt(i); continue; }
                // kralın hemen yanındaki altınları köylüler almaz
                if (King != null && Mathf.Abs(c.X - King.X) < 2f) continue;
                float d = Mathf.Abs(c.X - x);
                if (d <= bd) { bd = d; best = c; }
            }
            return best;
        }

        /// <summary>Belirli taraftaki en dıştaki ayakta duvar.</summary>
        public Wall OuterWall(int side)
        {
            Wall best = null;
            float bx = -1f;
            for (int i = 0; i < Walls.Count; i++)
            {
                var w = Walls[i];
                if (w == null || !w.Standing || w.Side != side) continue;
                float ax = Mathf.Abs(w.X);
                if (ax > bx) { bx = ax; best = w; }
            }
            return best;
        }

        /// <summary>fromX noktasından dir yönünde ilerleyen bir düşmanın önündeki ilk duvar.</summary>
        public Wall FirstWallAhead(float fromX, int dir, int side)
        {
            Wall best = null;
            float bd = float.MaxValue;
            for (int i = 0; i < Walls.Count; i++)
            {
                var w = Walls[i];
                if (w == null || !w.Standing || w.Side != side) continue;
                float delta = (w.X - fromX) * dir;
                if (delta < -0.4f) continue;              // geride kaldı
                if (delta < bd) { bd = delta; best = w; }
            }
            return best;
        }

        public Structure FindWork(float fromX, bool nightSafe)
        {
            Structure best = null;
            float bd = float.MaxValue;
            for (int i = 0; i < Structures.Count; i++)
            {
                var s = Structures[i];
                if (s == null || !s.NeedsWork) continue;
                if (nightSafe)
                {
                    var g = NearestGreed(s.X, 10f);
                    if (g != null) continue;
                }
                float d = Mathf.Abs(s.X - fromX);
                if (d < bd) { bd = d; best = s; }
            }
            return best;
        }

        public Farm FindFarmWithSlot(float fromX)
        {
            Farm best = null;
            float bd = float.MaxValue;
            for (int i = 0; i < Farms.Count; i++)
            {
                var f = Farms[i];
                if (f == null || !f.HasFreeSlot) continue;
                float d = Mathf.Abs(f.X - fromX);
                if (d < bd) { bd = d; best = f; }
            }
            return best;
        }

        public int CountRole(VillagerRole role)
        {
            int n = 0;
            for (int i = 0; i < Villagers.Count; i++)
                if (Villagers[i] != null && Villagers[i].Role == role) n++;
            return n;
        }

        /// <summary>Gece saklanılacak nokta: en içteki duvarın arkası / üs.</summary>
        public float ShelterX(float fromX)
        {
            float side = fromX >= BaseX ? 1f : -1f;
            return BaseX + side * Util.Rand(1.5f, 5f);
        }
    }
}
