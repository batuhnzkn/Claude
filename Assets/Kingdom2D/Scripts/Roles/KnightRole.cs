using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Şövalye: duvarın önünde durur, greed'lerle göğüs göğüse çarpışır.</summary>
    public class KnightRole : RoleBehavior
    {
        int side = 1;

        public override void Enter(Villager villager)
        {
            base.Enter(villager);
            side = SideOf(villager.X);
            if (side == 0) side = 1;
        }

        public override void Tick()
        {
            var g = GM.NearestGreed(v.X, 14f, false);
            if (g != null)
            {
                float d = Mathf.Abs(g.X - v.X);
                if (d > Cfg.knightRange * 0.9f)
                {
                    v.GoTo(g.X, v.WalkSpeed * 1.15f, Cfg.knightRange * 0.7f);
                }
                else
                {
                    v.Idle();
                    v.Face(g.X - v.X);
                    if (v.AttackTimer <= 0f)
                    {
                        g.Hurt(Cfg.knightDamage);
                        v.AttackTimer = Cfg.knightAttackInterval;
                        Sfx.Play("greedhit", 0.5f);
                        Fx.Burst(g.Center, Pal.Metal, 4, 2.5f, 0.7f);
                    }
                }
                return;
            }

            var outer = GM.OuterWall(side);
            float post = outer != null ? outer.X + side * 2.4f : GM.BaseX + side * 11f;
            if (v.GoTo(post, v.WalkSpeed, 0.4f))
            {
                v.Idle();
                v.Face(side);
            }
        }
    }
}
