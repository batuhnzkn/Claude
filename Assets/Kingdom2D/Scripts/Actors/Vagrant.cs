using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Kamplardaki işsiz gezginler. Bir altın karşılığı krallığa katılırlar.</summary>
    public class Vagrant : Actor
    {
        public VagrantCamp Camp;
        float timer;
        float targetX;

        public static Vagrant Spawn(Transform parent, VagrantCamp camp, float x)
        {
            var sr = Util.Spawn(parent, Art.Vagrant, new Vector3(x, 0f, 0f), Layer.Villager - 1, "Vagrant");
            sr.color = new Color(0.8f, 0.8f, 0.85f);
            var v = sr.gameObject.AddComponent<Vagrant>();
            v.X = x;
            v.targetX = x;
            v.Camp = camp;
            v.animStep = 0.26f;
            return v;
        }

        protected override void UpdateSprite()
        {
            sr.sprite = Art.Vagrant;
        }

        void Update()
        {
            if (GM == null || GM.State != GameState.Playing) { Apply(); return; }

            timer -= Time.deltaTime;
            float home = Camp != null ? Camp.X : X;

            var g = GM.NearestGreed(X, 7f);
            if (g != null)
            {
                Step((X - g.X) >= 0f ? 1f : -1f, Cfg.villagerRunSpeed);
            }
            else if (timer <= 0f)
            {
                targetX = home + Util.Rand(-3.5f, 3.5f);
                timer = Util.Rand(1.5f, 4f);
            }
            else if (!StepTo(targetX, Cfg.villagerSpeed * 0.5f, 0.3f))
            {
                // yürüyor
            }
            else
            {
                animTime = 0f;
            }

            Apply();
        }

        /// <summary>Krallığa katılır: köylüye dönüşür.</summary>
        public void Recruit()
        {
            Villager.Create(GM.ActorRoot, X, VillagerRole.Peasant);
            Fx.Burst(Center, Pal.White, 8, 2.6f);
            Destroy(gameObject);
        }
    }
}
