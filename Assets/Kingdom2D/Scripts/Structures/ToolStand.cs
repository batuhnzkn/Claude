using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Alet tezgahı. Ödeme yapılınca en yakın boş köylü gelip aleti alır
    /// ve rolü değişir (yay -> okçu, çekiç -> inşaatçı, tırpan -> çiftçi, kılıç -> şövalye).
    /// </summary>
    public class ToolStand : Structure
    {
        public StandKind Kind = StandKind.Bow;
        public int RequiredCampTier = 1;

        CoinSink sink;
        float hintCooldown;

        public static ToolStand Create(Transform parent, float x, StandKind kind, int requiredCampTier = 1)
        {
            var go = Util.Node(parent, "Stand_" + kind, new Vector3(x, GameManager.I.Config.groundY, 0f));
            var s = go.AddComponent<ToolStand>();
            s.X = x;
            s.Kind = kind;
            s.RequiredCampTier = requiredCampTier;
            s.Tier = 1;
            s.Hp = s.MaxHp = 1f;

            s.sr = Util.Spawn(go.transform, Art.Stand(kind), Vector3.zero, Layer.StructureBack, "stand");
            s.HalfWidth = s.sr.sprite.bounds.size.x * 0.5f;

            s.sink = go.AddComponent<CoinSink>();
            s.sink.Setup(GameManager.I.Config.CostStand(kind), 1.9f, s.OnPaid, true);
            return s;
        }

        public override bool NeedsWork { get { return false; } }

        void OnPaid()
        {
            var peasant = GM.NearestFreePeasant(X, 200f);
            if (peasant == null)
            {
                CoinPickup.Spawn(GM.WorldRoot, X, Cfg.CostStand(Kind), 3f);
                if (GM.Hud != null) GM.Hud.Flash("BOS KOYLU YOK - KAMPTAN ADAM TOPLA", Pal.Cape);
                return;
            }

            VillagerRole role;
            switch (Kind)
            {
                case StandKind.Hammer: role = VillagerRole.Builder; break;
                case StandKind.Scythe: role = VillagerRole.Farmer; break;
                case StandKind.Sword: role = VillagerRole.Knight; break;
                default: role = VillagerRole.Archer; break;
            }
            peasant.SendToStand(X, role);
            Fx.Burst(new Vector2(X, GroundY + 1.2f), Pal.Gold, 8, 2.6f);
        }

        protected override void UpdateVisual() { }

        protected override void Update()
        {
            base.Update();
            if (GM == null) return;

            bool unlocked = GM.Camp != null && GM.Camp.Tier >= RequiredCampTier;
            bool hasPeasant = GM.NearestFreePeasant(X, 200f) != null;

            if (sink != null) sink.Active = unlocked && hasPeasant;
            if (sr != null)
                sr.color = unlocked ? (hasPeasant ? Color.white : new Color(1f, 1f, 1f, 0.6f))
                                    : new Color(1f, 1f, 1f, 0.3f);

            // oyuncuya neden çalışmadığını söyle
            hintCooldown -= Time.deltaTime;
            if (unlocked && !hasPeasant && hintCooldown <= 0f && GM.King != null &&
                Mathf.Abs(GM.King.X - X) < 2.2f && GM.Hud != null)
            {
                hintCooldown = 6f;
                GM.Hud.Flash("ONCE SERSERI KAMPINDAN KOYLU TOPLA", Pal.Gold);
            }
        }
    }
}
