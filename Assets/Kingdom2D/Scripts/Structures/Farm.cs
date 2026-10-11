using System.Collections.Generic;
using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Tarla: çiftçiler burada çalışır, hasat altına dönüşür.</summary>
    public class Farm : Structure
    {
        public int Slots = 2;

        readonly List<Villager> workers = new List<Villager>();
        CoinSink sink;
        SpriteRenderer ghost;
        float growth;

        public bool Active { get { return Tier > 0 && !UnderConstruction; } }
        public bool HasFreeSlot { get { return Active && workers.Count < Slots; } }

        public static Farm Create(Transform parent, float x)
        {
            var go = Util.Node(parent, "Farm", new Vector3(x, GameManager.I.Config.groundY, 0f));
            var f = go.AddComponent<Farm>();
            f.X = x;
            f.Tier = 0;
            f.Hp = f.MaxHp = 1f;
            f.WorkOffset = 1.2f;
            f.BuildTime = 5f;

            f.ghost = Util.Spawn(go.transform, Art.Farm(0), Vector3.zero, Layer.StructureBack - 5, "farm_ghost");
            f.ghost.color = new Color(1f, 1f, 1f, 0.12f);
            f.sr = Util.Spawn(go.transform, Art.Farm(0), Vector3.zero, Layer.StructureBack, "farm");
            f.sr.enabled = false;
            f.HalfWidth = f.sr.sprite.bounds.size.x * 0.5f;

            f.sink = go.AddComponent<CoinSink>();
            f.sink.Setup(GameManager.I.Config.costFarmPlot, 1.6f, f.OnPaid);
            f.Register();
            if (GameManager.I != null) GameManager.I.Farms.Add(f);
            return f;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (GameManager.I != null) GameManager.I.Farms.Remove(this);
        }

        void OnPaid()
        {
            BeginBuild(1);
            if (GM.CountRole(VillagerRole.Builder) == 0 && GM.Hud != null)
                GM.Hud.Flash("INSAATCI GEREK (CEKIC TEZGAHI)", Pal.Gold);
        }

        protected override void OnConstructionStarted()
        {
            if (sink != null) sink.Active = false;
        }

        protected override float HpForTier(int tier) { return 20f; }

        protected override void FinishBuild()
        {
            base.FinishBuild();
            if (GM.Hud != null) GM.Hud.Flash("TARLA HAZIR - CIFTCI GEREK", Pal.Crop);
        }

        protected override void Collapse()
        {
            Fx.Burst(new Vector2(X, GroundY + 0.4f), Pal.Soil, 12, 3f);
            Tier = 0;
            Hp = 0f;
            growth = 0f;
            for (int i = workers.Count - 1; i >= 0; i--)
                if (workers[i] != null) workers[i].LeaveFarm();
            workers.Clear();
            if (sink != null) sink.Setup(Cfg.costFarmPlot, 1.6f, OnPaid);
            UpdateVisual();
        }

        protected override void UpdateVisual()
        {
            if (sr == null) return;
            if (!Active)
            {
                sr.enabled = false;
                if (ghost != null)
                {
                    ghost.enabled = true;
                    ghost.color = new Color(1f, 1f, 1f, UnderConstruction ? 0.12f + 0.4f * BuildProgress : 0.12f);
                }
                return;
            }
            sr.enabled = true;
            if (ghost != null) ghost.enabled = false;
            int stage = growth < 0.35f ? 0 : (growth < 0.75f ? 1 : 2);
            sr.sprite = Art.Farm(stage);
        }

        // ------------------------------------------------------------ çalışma

        public void AssignWorker(Villager v)
        {
            if (!HasFreeSlot || workers.Contains(v)) return;
            workers.Add(v);
            v.WorkFarm = this;
        }

        public void ReleaseWorker(Villager v)
        {
            workers.Remove(v);
        }

        public float WorkSpot(Villager v)
        {
            int index = workers.IndexOf(v);
            if (index < 0) index = 0;
            int n = Mathf.Max(1, workers.Count);
            float span = HalfWidth * 1.6f;
            if (n == 1) return X;
            return X - span * 0.5f + span * (index / (float)(n - 1));
        }

        public void Work(float dt, Villager worker)
        {
            if (!Active) return;
            growth += dt / Mathf.Max(1f, Cfg.farmCycle);
            if (Util.Chance(dt * 1.5f)) Fx.Burst(new Vector2(X + Util.Rand(-1f, 1f), GroundY + 0.5f), Pal.Crop, 1, 1.2f, 0.5f);

            if (growth >= 1f)
            {
                growth = 0f;
                worker.AddCarried(Cfg.farmYield);
                Fx.FloatText(new Vector2(worker.X, GroundY + 1.2f), "+" + Cfg.farmYield, Pal.Gold);
                Sfx.Play("coin", 0.3f);
            }
            UpdateVisual();
        }

        protected override void Update()
        {
            base.Update();
            for (int i = workers.Count - 1; i >= 0; i--)
                if (workers[i] == null) workers.RemoveAt(i);
            if (UnderConstruction) UpdateVisual();
        }
    }
}
