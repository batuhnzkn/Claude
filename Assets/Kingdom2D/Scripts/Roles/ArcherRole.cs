using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Okçu: gündüz avlanır (altın getirir), gece duvar üstündeki mevzisinden
    /// yaklaşan greed'leri vurur.
    /// </summary>
    public class ArcherRole : RoleBehavior
    {
        int side = 1;
        bool onPost;

        public override void Enter(Villager villager)
        {
            base.Enter(villager);
            side = SideOf(villager.X);
            if (side == 0) side = 1;
        }

        public override void Exit()
        {
            onPost = false;
        }

        public override void Tick()
        {
            bool night = GM.Darkness > 0.3f;
            if (night) Defend();
            else Hunt();
        }

        // ------------------------------------------------------------- savunma

        void Defend()
        {
            EnsurePost();

            float postX;
            float postHeight = 0f;
            if (v.Post != null && v.Post.Standing)
            {
                postX = v.Post.PostX(v.PostIndex);
                postHeight = v.Post.PostHeight;
            }
            else
            {
                var outer = GM.OuterWall(side);
                postX = outer != null ? outer.X - side * 1.6f : GM.BaseX + side * 7f;
            }

            bool arrived = v.GoTo(postX, v.RunSpeed, 0.2f);
            if (arrived)
            {
                v.Idle();
                if (postHeight > 0f && !onPost)
                {
                    onPost = true;
                    v.TargetHeight = postHeight;
                }
            }
            else if (onPost)
            {
                onPost = false;
                v.TargetHeight = 0f;
            }

            var target = GM.NearestGreed(v.X, Cfg.archerRange);
            if (target == null) return;

            v.Face(target.X - v.X);
            if (arrived && v.AttackTimer <= 0f)
            {
                Arrow.Fire(GM.ActorRoot, v.Center, target.Center, Cfg.arrowSpeed, Cfg.arrowDamage);
                v.AttackTimer = Cfg.archerFireInterval;
                Sfx.Play("bow", 0.35f);
            }
        }

        void EnsurePost()
        {
            if (v.Post != null && v.Post.Standing) return;
            if (v.Post != null) v.LeavePost();

            // kendi tarafındaki en dış duvardan içe doğru boş mevzi ara
            Wall best = null;
            int bestIndex = -1;
            float bestX = -1f;
            for (int i = 0; i < GM.Walls.Count; i++)
            {
                var w = GM.Walls[i];
                if (w == null || !w.Standing || w.Side != side) continue;
                if (!w.HasFreePost) continue;
                float ax = Mathf.Abs(w.X);
                if (ax > bestX) { bestX = ax; best = w; bestIndex = w.FirstFreePost(); }
            }
            if (best == null)
            {
                // kendi tarafında yer yok: diğer tarafa bak
                for (int i = 0; i < GM.Walls.Count; i++)
                {
                    var w = GM.Walls[i];
                    if (w == null || !w.Standing || !w.HasFreePost) continue;
                    float ax = Mathf.Abs(w.X);
                    if (ax > bestX) { bestX = ax; best = w; bestIndex = w.FirstFreePost(); side = w.Side; }
                }
            }
            if (best != null && bestIndex >= 0) best.ClaimPost(bestIndex, v);
        }

        // ---------------------------------------------------------------- av

        void Hunt()
        {
            if (onPost || v.TargetHeight > 0f)
            {
                onPost = false;
                v.TargetHeight = 0f;
            }

            if (DeliverCoinsIfCarrying()) return;
            if (CollectLooseCoin(14f)) return;

            var prey = GM.NearestCritter(v.X, 26f);
            if (prey != null)
            {
                float d = Mathf.Abs(prey.X - v.X);
                if (d > Cfg.archerRange * 0.8f)
                {
                    v.GoTo(prey.X, v.WalkSpeed, 0.3f);
                }
                else
                {
                    v.Idle();
                    v.Face(prey.X - v.X);
                    if (v.AttackTimer <= 0f)
                    {
                        Arrow.Fire(GM.ActorRoot, v.Center, prey.Center, Cfg.arrowSpeed, Cfg.arrowDamage);
                        v.AttackTimer = Cfg.archerFireInterval * 0.8f;
                        Sfx.Play("bow", 0.3f);
                    }
                }
                return;
            }

            // av yoksa kendi tarafında devriye
            var outer = GM.OuterWall(side);
            float center = outer != null ? outer.X + side * 4f : GM.BaseX + side * 16f;
            Wander(center, 12f, v.WalkSpeed);
        }
    }
}
