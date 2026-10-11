using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Bir köylü rolünün davranışı. Her rol bunu kendine göre uygular.</summary>
    public abstract class RoleBehavior
    {
        protected Villager v;

        protected GameManager GM { get { return GameManager.I; } }
        protected GameConfig Cfg { get { return GameManager.I.Config; } }

        public virtual void Enter(Villager villager) { v = villager; }
        public abstract void Tick();
        public virtual void Exit() { }

        /// <summary>Akşam karanlığı bastı mı (sivillerin eve dönme vakti).</summary>
        protected bool Dangerous { get { return GM.Darkness > 0.4f; } }

        protected int SideOf(float x) { return x >= GM.BaseX ? 1 : -1; }

        /// <summary>Yakında greed varsa ters yöne kaçar.</summary>
        protected bool FleeIfThreatened(float range)
        {
            var g = GM.NearestGreed(v.X, range);
            if (g == null) return false;
            float away = (v.X - g.X) >= 0f ? 1f : -1f;
            float target = Mathf.Clamp(v.X + away * 7f, -Cfg.worldHalfWidth + 2f, Cfg.worldHalfWidth - 2f);
            v.GoTo(target, v.RunSpeed, 0.1f);
            return true;
        }

        /// <summary>Merkez etrafında amaçsız dolaşma.</summary>
        protected void Wander(float centerX, float radius, float speed)
        {
            if (v.Timer <= 0f)
            {
                v.WanderX = centerX + Util.Rand(-radius, radius);
                v.Timer = Util.Rand(1.4f, 4.5f);
            }
            if (v.GoTo(v.WanderX, speed * 0.62f, 0.3f)) v.Idle();
        }

        /// <summary>Altın taşıyorsa üsse götürüp bırakır.</summary>
        protected bool DeliverCoinsIfCarrying()
        {
            if (v.CarriedCoins <= 0) return false;
            float side = v.X >= GM.BaseX ? 1f : -1f;
            if (v.GoTo(GM.BaseX + side * 1.2f, v.WalkSpeed, 0.4f)) v.DropCarriedAtBase();
            return true;
        }

        /// <summary>Yerdeki altını alıp taşımaya başlar.</summary>
        protected bool CollectLooseCoin(float searchRange)
        {
            var coin = GM.NearestCoin(v.X, searchRange);
            if (coin == null) return false;
            if (v.GoTo(coin.X, v.WalkSpeed, 0.35f))
            {
                if (coin.Take()) v.AddCarried(coin.Value);
            }
            return true;
        }
    }
}
