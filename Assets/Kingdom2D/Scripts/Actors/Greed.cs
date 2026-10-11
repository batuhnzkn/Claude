using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Greed: geceleri deliklerden çıkan açgözlü yaratıklar. Duvarları kırar,
    /// altın ve köylü çalar, kralın tacına göz diker. Gündüz güneşte yanar.
    /// </summary>
    public class Greed : Actor
    {
        public GreedType Type = GreedType.Floater;
        public float Hp = 2f;
        public float MaxHp = 2f;
        public float Damage = 1f;
        public float Speed = 2f;
        public int Side = -1;                 // -1: soldaki delik, +1: sağdaki
        public int Loot;
        public Villager Captive;
        public bool Fleeing;

        float attackTimer;
        float hurtFlash;
        float bobPhase;
        bool removed;

        public bool Alive { get { return Hp > 0f; } }
        public int AdvanceDir { get { return -Side; } }

        public static void Stats(GreedType t, out float hp, out float dmg, out float speedMul)
        {
            switch (t)
            {
                case GreedType.Walker: hp = 5f; dmg = 2f; speedMul = 1f; break;
                case GreedType.Brute: hp = 17f; dmg = 6f; speedMul = 0.72f; break;
                case GreedType.Flyer: hp = 4f; dmg = 2f; speedMul = 1.45f; break;
                default: hp = 2f; dmg = 1f; speedMul = 1.18f; break;
            }
        }

        public static Greed Spawn(Transform parent, float x, int side, GreedType type, float hpScale)
        {
            var sr = Util.Spawn(parent, Art.Greed(type, 0), new Vector3(x, 0f, 0f), Layer.Greed, "Greed");
            var g = sr.gameObject.AddComponent<Greed>();
            float hp, dmg, spd;
            Stats(type, out hp, out dmg, out spd);
            g.Type = type;
            g.MaxHp = g.Hp = hp * hpScale;
            g.Damage = dmg;
            g.Speed = GameManager.I.Config.greedBaseSpeed * spd;
            g.Side = side;
            g.X = x;
            g.animStep = 0.25f;
            g.bobPhase = Util.Rand(0f, 6.28f);
            g.SetFacing(-side);
            if (type == GreedType.Flyer) g.Height = Util.Rand(1.8f, 3.2f);
            GameManager.I.Greeds.Add(g);
            return g;
        }

        protected override void UpdateSprite()
        {
            sr.sprite = Art.Greed(Type, frame);
            sr.color = hurtFlash > 0f
                ? Color.Lerp(Color.white, new Color(1f, 0.55f, 0.4f), hurtFlash)
                : Color.white;
        }

        void Update()
        {
            if (GM == null || Cfg == null || removed) return;
            if (GM.State != GameState.Playing) { Apply(); return; }

            hurtFlash = Mathf.MoveTowards(hurtFlash, 0f, Time.deltaTime * 4f);
            attackTimer = Mathf.MoveTowards(attackTimer, 0f, Time.deltaTime);

            if (Type == GreedType.Flyer)
            {
                bobPhase += Time.deltaTime * 2.6f;
                Height = Mathf.Max(0.6f, Height + Mathf.Sin(bobPhase) * 0.004f);
                animTime += Time.deltaTime * 2f;   // kanat çırpma
            }

            // gün ışığında yanarlar
            if (GM.Darkness < 0.3f)
            {
                Hp -= Time.deltaTime * 2.5f;
                if (Util.Chance(Time.deltaTime * 6f)) Fx.Burst(Center, Pal.Flame, 1, 1.2f, 0.6f);
                if (!Fleeing) StartFleeing();
                if (Hp <= 0f) { Die(); return; }
            }

            if (Fleeing) FleeTick();
            else AdvanceTick();
            if (removed) return;

            Apply();
        }

        // ---------------------------------------------------------- davranış

        void AdvanceTick()
        {
            int dir = AdvanceDir;
            bool flyer = Type == GreedType.Flyer;

            // önündeki duvar
            Wall wall = flyer ? null : GM.FirstWallAhead(X, dir, Side);
            float stopX = float.NaN;
            if (wall != null)
            {
                stopX = wall.X - dir * (wall.HalfWidth + 0.45f);
                if ((X - stopX) * dir >= -0.05f)
                {
                    AttackWall(wall);
                    return;
                }
            }

            // önündeki savunmacı
            var defender = GM.NearestAnyVillager(X, 1.1f, !flyer);
            if (defender != null)
            {
                if (defender.Role == VillagerRole.Knight || Type == GreedType.Brute)
                {
                    if (attackTimer <= 0f)
                    {
                        defender.Hurt(Damage);
                        attackTimer = Cfg.greedAttackInterval;
                        Sfx.Play("greedhit", 0.45f);
                    }
                    SetFacing(defender.X - X);
                    animTime = 0f;
                    return;
                }
                if (Captive == null)
                {
                    Captive = defender;
                    defender.Capture(this);
                    StartFleeing();
                    return;
                }
            }

            // yerdeki altın
            var coin = GM.Coins.Count > 0 ? NearbyCoin(0.8f) : null;
            if (coin != null && coin.Take())
            {
                Loot += coin.Value;
                Sfx.Play("steal", 0.5f);
                Fx.Burst(Center, Pal.Gold, 4, 2f, 0.7f);
                StartFleeing();
                return;
            }

            // kral
            if (GM.King != null && Mathf.Abs(GM.King.X - X) < 0.85f && Height < 1.2f)
            {
                GM.King.Robbed(this);
                return;
            }

            // üs (duvarlar düştüyse)
            if (GM.Camp != null && !flyer && Mathf.Abs(GM.Camp.X - X) < GM.Camp.HalfWidth + 0.5f)
            {
                if (attackTimer <= 0f)
                {
                    GM.Camp.Damage(Damage);
                    attackTimer = Cfg.greedAttackInterval;
                    Sfx.Play("greedhit", 0.5f);
                    GM.Rig.Shake(0.1f);
                }
                animTime = 0f;
                return;
            }

            Step(dir, Speed);
            if (!float.IsNaN(stopX) && (X - stopX) * dir > 0f) X = stopX;
        }

        void AttackWall(Wall wall)
        {
            SetFacing(wall.X - X);
            animTime = 0f;
            if (attackTimer > 0f) return;
            attackTimer = Cfg.greedAttackInterval;
            wall.Damage(Damage);
            Sfx.Play("greedhit", 0.4f);
            Fx.Burst(new Vector2(wall.X - Mathf.Sign(wall.X - X) * wall.HalfWidth, GroundLevel + 0.6f),
                Pal.Wood, 3, 2f, 0.7f);
            GM.Rig.Shake(0.06f);
        }

        CoinPickup NearbyCoin(float range)
        {
            for (int i = GM.Coins.Count - 1; i >= 0; i--)
            {
                var c = GM.Coins[i];
                if (c == null || c.Claimed) { GM.Coins.RemoveAt(i); continue; }
                if (Mathf.Abs(c.X - X) <= range) return c;
            }
            return null;
        }

        void FleeTick()
        {
            float portalX = Side * (Cfg.worldHalfWidth - 25f);
            Step(Side, Speed * 1.25f);
            if (Mathf.Abs(X - portalX) < 1.2f) Escape();
        }

        public void StartFleeing()
        {
            if (Fleeing) return;
            Fleeing = true;
            SetFacing(Side);
        }

        void Escape()
        {
            if (Captive != null)
            {
                var c = Captive;
                Captive = null;
                c.Captor = null;
                if (GM.Hud != null) GM.Hud.Flash("BIR KOYLU KACIRILDI", Pal.Cape);
                c.Remove();
            }
            removed = true;
            if (GM != null) GM.Greeds.Remove(this);
            Destroy(gameObject);
        }

        // ------------------------------------------------------------- hasar

        public void Hurt(float dmg)
        {
            if (!Alive) return;
            Hp -= dmg;
            hurtFlash = 1f;
            Fx.Burst(Center, Pal.GreedSkin2, 3, 2f, 0.7f);
            if (Hp <= 0f) Die();
        }

        public void TakeLoot(int coins)
        {
            Loot += coins;
            StartFleeing();
        }

        void Die()
        {
            Fx.Burst(Center, Pal.GreedSkin2, 10, 3.2f);
            Fx.Burst(Center, Pal.Eye, 4, 2.4f, 0.8f);
            Sfx.Play("greeddie", 0.5f);

            for (int i = 0; i < Loot; i++)
                CoinPickup.Spawn(GM.WorldRoot, X + Util.Rand(-0.4f, 0.4f), 1, Util.Rand(2.5f, 4.5f));

            if (Captive != null)
            {
                Captive.Release();
                Captive = null;
            }
            removed = true;
            if (GM != null) GM.Greeds.Remove(this);
            Destroy(gameObject);
        }
    }
}
