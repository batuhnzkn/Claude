using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Tavşan ve geyik: okçular avlayıp altın kazanır.</summary>
    public class Critter : Actor
    {
        public bool IsDeer;
        public float Hp = 1f;
        public int CoinValue = 1;

        float idleTimer;
        float targetX;
        float homeX;

        public bool Alive { get { return Hp > 0f; } }

        public static Critter Spawn(Transform parent, float x, bool deer)
        {
            var sr = Util.Spawn(parent, deer ? Art.Deer : Art.Rabbit, new Vector3(x, 0f, 0f),
                Layer.Villager - 2, deer ? "Deer" : "Rabbit");
            sr.color = new Color(0.75f, 0.72f, 0.68f);
            var c = sr.gameObject.AddComponent<Critter>();
            c.IsDeer = deer;
            c.Hp = deer ? 4f : 1.5f;
            c.CoinValue = deer ? 4 : 1;
            c.X = x;
            c.homeX = x;
            c.targetX = x;
            c.animStep = deer ? 0.3f : 0.16f;
            c.bobPixels = deer ? 1f : 2f;
            if (GameManager.I != null) GameManager.I.Critters.Add(c);
            return c;
        }

        protected override void UpdateSprite()
        {
            sr.sprite = IsDeer ? Art.Deer : Art.Rabbit;
        }

        void Update()
        {
            if (GM == null || Cfg == null) return;
            if (GM.State != GameState.Playing) { Apply(); return; }

            // gece olunca ormana çekilirler
            if (GM.Darkness > 0.5f) { Despawn(); return; }

            idleTimer -= Time.deltaTime;

            float threatX = 0f;
            bool threat = false;
            if (GM.King != null && Mathf.Abs(GM.King.X - X) < 4.5f) { threat = true; threatX = GM.King.X; }
            var g = GM.NearestGreed(X, 6f);
            if (g != null) { threat = true; threatX = g.X; }

            if (threat)
            {
                float away = (X - threatX) >= 0f ? 1f : -1f;
                Step(away, IsDeer ? 6.5f : 7.5f);
                idleTimer = Util.Rand(0.6f, 1.4f);
            }
            else if (idleTimer <= 0f)
            {
                targetX = homeX + Util.Rand(-9f, 9f);
                idleTimer = Util.Rand(1.2f, 3.5f);
            }
            else if (Mathf.Abs(targetX - X) > 0.3f)
            {
                StepTo(targetX, IsDeer ? 2.2f : 3f, 0.25f);
            }
            else
            {
                Idle();
            }

            ClampToWorld(3f);
            Apply();
        }

        void Idle() { animTime = 0f; }

        public void Hurt(float dmg)
        {
            Hp -= dmg;
            if (Hp > 0f)
            {
                Fx.Burst(Center, Pal.Fur, 3, 2f, 0.6f);
                return;
            }
            Fx.Burst(Center, Pal.Fur, 8, 2.6f);
            Sfx.Play("critter", 0.5f);
            for (int i = 0; i < CoinValue; i++)
                CoinPickup.Spawn(GM.WorldRoot, X + Util.Rand(-0.4f, 0.4f), 1, Util.Rand(2f, 4f));
            Despawn();
        }

        public void Despawn()
        {
            if (GM != null) GM.Critters.Remove(this);
            Destroy(gameObject);
        }
    }
}
