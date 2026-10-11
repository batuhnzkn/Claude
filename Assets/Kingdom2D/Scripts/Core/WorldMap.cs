using System.Collections.Generic;
using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Haritayı kurar: üs, tezgahlar, tarlalar, duvar yerleri, kamplar,
    /// greed delikleri, ağaçlar ve yaban hayatı.</summary>
    public class WorldMap : MonoBehaviour
    {
        GameConfig cfg;
        Transform worldRoot;
        Transform actorRoot;
        readonly List<float> occupied = new List<float>();

        public static readonly float[] WallOffsets = { 20f, 38f, 58f, 80f };
        public static readonly float[] CampOffsets = { 30f, 66f };

        public void Build(GameConfig config, Transform world, Transform actors)
        {
            cfg = config;
            worldRoot = world;
            actorRoot = actors;

            var gm = GameManager.I;

            // --- üs
            gm.Camp = BaseCamp.Create(worldRoot, 0f);
            Occupy(0f, 4f);

            // --- alet tezgahları
            ToolStand.Create(worldRoot, -6.5f, StandKind.Bow);
            ToolStand.Create(worldRoot, 6.5f, StandKind.Hammer);
            ToolStand.Create(worldRoot, -11.5f, StandKind.Scythe);
            ToolStand.Create(worldRoot, 11.5f, StandKind.Sword, 2);
            Occupy(-6.5f, 1.5f); Occupy(6.5f, 1.5f); Occupy(-11.5f, 1.5f); Occupy(11.5f, 1.5f);

            // --- tarlalar
            Farm.Create(worldRoot, -15.5f);
            Farm.Create(worldRoot, 15.5f);
            Occupy(-15.5f, 2.5f); Occupy(15.5f, 2.5f);

            // --- duvar mevzileri
            for (int s = -1; s <= 1; s += 2)
                for (int i = 0; i < WallOffsets.Length; i++)
                {
                    float x = s * WallOffsets[i];
                    Wall.Create(worldRoot, x, s);
                    Occupy(x, 3f);
                }

            // --- gezgin kampları
            for (int s = -1; s <= 1; s += 2)
                for (int i = 0; i < CampOffsets.Length; i++)
                {
                    float x = s * CampOffsets[i];
                    VagrantCamp.Create(actorRoot, x, 4);
                    Occupy(x, 4f);
                }

            // --- greed delikleri
            float portalX = cfg.worldHalfWidth - 25f;
            GreedPortal.Create(worldRoot, -portalX, -1);
            GreedPortal.Create(worldRoot, portalX, 1);
            Occupy(-portalX, 4f); Occupy(portalX, 4f);

            ScatterNature();
            ScatterTreasure();
            RespawnCritters();

            // --- başlangıç köylüleri
            Villager.Create(actorRoot, -2.5f, VillagerRole.Peasant);
            Villager.Create(actorRoot, 2.5f, VillagerRole.Peasant);
        }

        void Occupy(float x, float radius)
        {
            occupied.Add(x);
            occupied.Add(radius);
        }

        bool Free(float x, float pad)
        {
            for (int i = 0; i < occupied.Count; i += 2)
                if (Mathf.Abs(occupied[i] - x) < occupied[i + 1] + pad) return false;
            return true;
        }

        /// <summary>Haritaya dağılmış altın yığınları: keşfetmenin ödülü.</summary>
        void ScatterTreasure()
        {
            float[] spots = { 23f, 45f, 70f, 93f };
            for (int s = -1; s <= 1; s += 2)
                for (int i = 0; i < spots.Length; i++)
                {
                    float x = s * spots[i] + Util.Rand(-2f, 2f);
                    int n = 2 + i;
                    for (int k = 0; k < n; k++)
                        CoinPickup.Spawn(worldRoot, x + Util.Rand(-0.9f, 0.9f), 1, Util.Rand(0.4f, 1.6f));
                }
        }

        void ScatterNature()
        {
            float limit = cfg.worldHalfWidth - 4f;

            for (int i = 0; i < cfg.treeCount; i++)
            {
                float x = Util.Rand(-limit, limit);
                if (!Free(x, 1.2f)) continue;
                bool pine = Util.Chance(0.6f);
                bool front = Util.Chance(0.22f);
                var sr = Util.Spawn(worldRoot,
                    pine ? Art.Pine(Util.RandInt(0, 4)) : Art.Oak(Util.RandInt(0, 4)),
                    new Vector3(x, cfg.groundY, 0f),
                    front ? Layer.DecorFront : Layer.Decor, "tree");
                float scale = front ? Util.Rand(1.15f, 1.5f) : Util.Rand(0.8f, 1.1f);
                sr.transform.localScale = new Vector3(Util.Chance(0.5f) ? scale : -scale, scale, 1f);
                sr.color = front
                    ? new Color(0.55f, 0.6f, 0.6f)
                    : Color.Lerp(Color.white, new Color(0.8f, 0.88f, 0.9f), Util.Rand(0f, 0.4f));
                var sway = sr.gameObject.AddComponent<Sway>();
                sway.amount = front ? 0.9f : 1.6f;
            }

            for (int i = 0; i < 40; i++)
            {
                float x = Util.Rand(-limit, limit);
                if (!Free(x, 0.6f)) continue;
                Util.Spawn(worldRoot, Art.Bush, new Vector3(x, cfg.groundY, 0f), Layer.Decor, "bush");
            }

            for (int i = 0; i < 26; i++)
            {
                float x = Util.Rand(-limit, limit);
                Util.Spawn(worldRoot, Art.Rock(Util.RandInt(0, 3)), new Vector3(x, cfg.groundY, 0f),
                    Layer.GroundDecor, "rock");
            }

            for (int i = 0; i < 170; i++)
            {
                float x = Util.Rand(-limit, limit);
                var g = Util.Spawn(worldRoot, Art.Grass(Util.RandInt(0, 5)), new Vector3(x, cfg.groundY, 0f),
                    Util.Chance(0.3f) ? Layer.DecorFront : Layer.GroundDecor, "grass");
                g.color = new Color(0.9f, 0.95f, 0.9f, 0.85f);
            }
        }

        /// <summary>Her sabah ormanı yeniden canlandırır.</summary>
        public void RespawnCritters()
        {
            var gm = GameManager.I;
            if (gm == null) return;
            int missing = cfg.critterCount - gm.Critters.Count;
            for (int i = 0; i < missing; i++) SpawnOneCritter();
        }

        void SpawnOneCritter()
        {
            float side = Util.Chance(0.5f) ? -1f : 1f;
            // çoğu av üssün yakın çevresinde: okçular gündüz buradan geçinir
            float dist = Util.Chance(0.65f) ? Util.Rand(8f, 34f) : Util.Rand(34f, cfg.worldHalfWidth - 18f);
            Critter.Spawn(actorRoot, side * dist, Util.Chance(0.25f));
        }

        float critterTimer = 5f;

        void Update()
        {
            var gm = GameManager.I;
            if (gm == null || gm.State != GameState.Playing || cfg == null) return;
            if (gm.Darkness > 0.45f) return;

            critterTimer -= Time.deltaTime;
            if (critterTimer <= 0f)
            {
                critterTimer = 5f;
                if (gm.Critters.Count < cfg.critterCount) SpawnOneCritter();
            }
        }
    }

    /// <summary>Ağaçlara hafif rüzgar salınımı verir.</summary>
    public class Sway : MonoBehaviour
    {
        public float amount = 1.5f;
        float phase;

        void Start() { phase = Util.Rand(0f, 6.28f); }

        void Update()
        {
            float a = Mathf.Sin(Time.time * 0.8f + phase) * amount;
            transform.rotation = Quaternion.Euler(0f, 0f, a);
        }
    }
}
