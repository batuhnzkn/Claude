using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Krallığın kalbi: kamp ateşi -> ahşap kale -> taş kale.
    /// Kademesi arttıkça kralın kese kapasitesi büyür.</summary>
    public class BaseCamp : Structure
    {
        public const int MaxTier = 3;

        CoinSink sink;
        SpriteRenderer fire;
        float fireAnim;

        public static BaseCamp Create(Transform parent, float x)
        {
            var go = Util.Node(parent, "BaseCamp", new Vector3(x, GameManager.I.Config.groundY, 0f));
            var c = go.AddComponent<BaseCamp>();
            c.X = x;
            c.Tier = 1;
            c.MaxHp = c.Hp = c.HpForTier(1);
            c.WorkOffset = 2.2f;
            c.BuildTime = 10f;

            c.sr = Util.Spawn(go.transform, Art.Camp(1), Vector3.zero, Layer.StructureBack, "camp");
            c.fire = Util.Spawn(go.transform, Art.Flame(0), new Vector3(-0.25f, 0.05f, 0f), Layer.StructureBack + 2, "campfire");
            c.HalfWidth = c.sr.sprite.bounds.size.x * 0.5f;

            c.sink = go.AddComponent<CoinSink>();
            c.SetupSink();
            c.Register();
            return c;
        }

        void SetupSink()
        {
            if (sink == null) return;
            int next = Tier + 1;
            if (next > MaxTier || UnderConstruction) { sink.Active = false; return; }
            sink.Setup(Cfg.CostCamp(next), Art.CampHeight(Tier) + 0.8f, OnPaid);
        }

        void OnPaid()
        {
            BeginBuild(Tier + 1);
            if (GM.CountRole(VillagerRole.Builder) == 0 && GM.Hud != null)
                GM.Hud.Flash("INSAATCI GEREK (CEKIC TEZGAHI)", Pal.Gold);
        }

        protected override void OnConstructionStarted()
        {
            if (sink != null) sink.Active = false;
        }

        protected override float HpForTier(int tier)
        {
            switch (tier) { case 2: return 320f; case 3: return 700f; default: return 150f; }
        }

        protected override void FinishBuild()
        {
            base.FinishBuild();
            SetupSink();
            if (GM.Hud != null) GM.Hud.Flash("US BUYUDU! KESE +" + Cfg.purseBonusPerTier, Pal.Gold);
        }

        protected override void Collapse()
        {
            Sfx.Play("break", 1f);
            Fx.Burst(new Vector2(X, GroundY + 1f), Pal.Stone, 20, 4f);
            if (GM != null && GM.Rig != null) GM.Rig.Shake(0.4f);

            if (Tier > 1)
            {
                Tier--;
                MaxHp = HpForTier(Tier);
                Hp = MaxHp * 0.5f;
                if (GM.Hud != null) GM.Hud.Flash("US YIKILIYOR!", Pal.Cape);
            }
            else
            {
                Hp = 1f;   // kamp ateşi sönmez ama kral savunmasız kalır
                if (GM.Hud != null) GM.Hud.Flash("US HARABEYE DONDU!", Pal.Cape);
            }
            SetupSink();
            UpdateVisual();
        }

        protected override void UpdateVisual()
        {
            if (sr == null) return;
            sr.sprite = Art.Camp(Tier);
            HalfWidth = sr.sprite.bounds.size.x * 0.5f;
            if (fire != null) fire.enabled = Tier == 1;
            float wear = Mathf.Clamp01(Hp / Mathf.Max(1f, MaxHp));
            sr.color = Color.Lerp(new Color(0.75f, 0.65f, 0.65f), Color.white, wear);
        }

        protected override void Update()
        {
            base.Update();
            fireAnim += Time.deltaTime * 10f;
            if (fire != null && fire.enabled) fire.sprite = Art.Flame(((int)fireAnim) % 3);
        }
    }
}
