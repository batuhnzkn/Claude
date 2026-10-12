using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Savunma duvarı. Üç kademe: kazık, ahşap palisad, taş sur.
    /// Üstünde okçu mevzileri vardır; greed'ler önce buraya saldırır.
    /// </summary>
    public class Wall : Structure
    {
        public int Side = -1;
        public const int MaxTier = 3;

        CoinSink sink;
        NightGlow glow;
        Villager[] posts = new Villager[3];
        SpriteRenderer ghost;     // henüz yapılmamışken gösterilen soluk taslak

        public bool Standing { get { return Tier > 0 && Hp > 0f; } }

        public float PostHeight { get { return Art.WallHeight(Tier); } }

        public int PostCount { get { return Tier <= 0 ? 0 : (Tier == 1 ? 1 : (Tier == 2 ? 2 : 3)); } }

        public static Wall Create(Transform parent, float x, int side)
        {
            var go = Util.Node(parent, "Wall", new Vector3(x, GameManager.I.Config.groundY, 0f));
            var w = go.AddComponent<Wall>();
            w.X = x;
            w.Side = side;
            w.Tier = 0;
            w.Hp = w.MaxHp = 1f;
            w.WorkOffset = 1.6f;
            w.HalfWidth = 1f;
            w.BuildTime = 7f;

            w.ghost = Util.Spawn(go.transform, Art.Wall(1, false), Vector3.zero, Layer.StructureBack - 5, "ghost");
            w.ghost.color = new Color(1f, 1f, 1f, 0.12f);
            w.sr = Util.Spawn(go.transform, Art.Wall(1, false), Vector3.zero, Layer.StructureBack, "wall");
            w.sr.enabled = false;

            w.glow = NightGlow.Create(go.transform, new Vector3(0f, 1.1f, 0f), 5.5f, 0.42f, new Color(1f, 0.7f, 0.42f));
            w.glow.On = false;

            w.sink = go.AddComponent<CoinSink>();
            w.SetupSink();
            w.Register();
            if (GameManager.I != null) GameManager.I.Walls.Add(w);
            return w;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (GameManager.I != null) GameManager.I.Walls.Remove(this);
        }

        void SetupSink()
        {
            if (sink == null) return;
            int nextTier = Tier + 1;
            if (nextTier > MaxTier || UnderConstruction)
            {
                sink.Active = false;
                return;
            }
            sink.Setup(Cfg.CostWall(nextTier), Mathf.Max(1.3f, Art.WallHeight(Tier) + 1.1f), OnPaid);
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
            if (ghost != null)
            {
                ghost.sprite = Art.Wall(pendingTier, false);
                ghost.enabled = true;
            }
        }

        protected override float HpForTier(int tier)
        {
            switch (tier) { case 1: return 110f; case 2: return 280f; case 3: return 600f; default: return 1f; }
        }

        protected override void FinishBuild()
        {
            base.FinishBuild();
            SetupSink();
            if (GM.Hud != null && Tier == 1) GM.Hud.Flash("DUVAR YUKSELDI", Pal.White);
        }

        protected override void OnTierChanged()
        {
            // kademe değişince mevziler yeniden düzenlenir
            for (int i = 0; i < posts.Length; i++)
            {
                if (posts[i] == null) continue;
                if (i >= PostCount) { posts[i].LeavePost(); }
                else posts[i].Height = PostHeight;
            }
        }

        protected override void Collapse()
        {
            Sfx.Play("break", 0.8f);
            Fx.Burst(new Vector2(X, GroundY + 0.8f), Pal.Wood, 16, 3.5f);
            if (GM != null && GM.Rig != null) GM.Rig.Shake(0.25f);

            for (int i = 0; i < posts.Length; i++)
                if (posts[i] != null) posts[i].LeavePost();

            Tier = Mathf.Max(0, Tier - 1);
            if (Tier > 0)
            {
                MaxHp = HpForTier(Tier);
                Hp = MaxHp * 0.55f;
            }
            else
            {
                Hp = 0f;
                MaxHp = 1f;
            }
            UpdateVisual();
            SetupSink();
            if (GM != null && GM.Hud != null) GM.Hud.Flash("DUVAR YIKILDI!", Pal.Cape);
        }

        protected override void UpdateVisual()
        {
            if (sr == null) return;
            if (Tier <= 0)
            {
                sr.enabled = false;
                if (ghost != null)
                {
                    ghost.enabled = true;
                    ghost.sprite = Art.Wall(UnderConstruction ? pendingTier : 1, false);
                    ghost.color = new Color(1f, 1f, 1f, UnderConstruction ? 0.12f + 0.4f * BuildProgress : 0.12f);
                }
                HalfWidth = 1f;
                return;
            }

            sr.enabled = true;
            bool damaged = Hp < MaxHp * 0.5f;
            sr.sprite = Art.Wall(Tier, damaged);
            HalfWidth = sr.sprite.bounds.size.x * 0.5f;

            if (ghost != null)
            {
                ghost.enabled = UnderConstruction;
                if (UnderConstruction)
                {
                    ghost.sprite = Art.Wall(pendingTier, false);
                    ghost.color = new Color(1f, 1f, 1f, 0.12f + 0.45f * BuildProgress);
                }
            }
        }

        protected override void Update()
        {
            base.Update();
            if (UnderConstruction) UpdateVisual();
            if (glow != null)
            {
                glow.On = Standing;
                glow.transform.localPosition = new Vector3(0f, Mathf.Max(0.8f, PostHeight + 0.3f), 0f);
            }
        }

        // ------------------------------------------------------------ mevziler

        public bool HasFreePost
        {
            get
            {
                for (int i = 0; i < PostCount; i++)
                    if (posts[i] == null) return true;
                return false;
            }
        }

        public int FirstFreePost()
        {
            for (int i = 0; i < PostCount; i++)
                if (posts[i] == null) return i;
            return -1;
        }

        public void ClaimPost(int index, Villager v)
        {
            if (index < 0 || index >= posts.Length) return;
            posts[index] = v;
            v.Post = this;
            v.PostIndex = index;
        }

        public void ReleasePost(int index, Villager v)
        {
            if (index < 0 || index >= posts.Length) return;
            if (posts[index] == v) posts[index] = null;
        }

        public float PostX(int index)
        {
            int n = Mathf.Max(1, PostCount);
            float span = Mathf.Max(0.6f, HalfWidth * 1.5f);
            if (n == 1) return X;
            float t = index / (float)(n - 1);
            return X - span * 0.5f + span * t;
        }
    }
}
