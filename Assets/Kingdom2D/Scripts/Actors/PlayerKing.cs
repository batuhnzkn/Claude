using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Oyuncu: at üstündeki hükümdar. Altın taşır, atar ve tacını korur.</summary>
    public class PlayerKing : Actor
    {
        public int Coins;

        float dropTimer;
        float hurtFlash;
        float robCooldown;

        public int Capacity
        {
            get
            {
                int tierBonus = GM != null && GM.Camp != null ? (GM.Camp.Tier - 1) * Cfg.purseBonusPerTier : 0;
                return Cfg.purseBase + tierBonus;
            }
        }

        public static PlayerKing Create(Transform parent, float x, int coins)
        {
            var sr = Util.Spawn(parent, Art.King(0), new Vector3(x, 0f, 0f), Layer.Player, "King");
            var k = sr.gameObject.AddComponent<PlayerKing>();
            k.X = x;
            k.Coins = coins;
            k.animStep = 0.26f;
            k.bobPixels = 1f;
            return k;
        }

        protected override void UpdateSprite()
        {
            sr.sprite = Art.King(frame);
            Color c = Color.white;
            if (hurtFlash > 0f) c = Color.Lerp(Color.white, new Color(1f, 0.4f, 0.4f), hurtFlash);
            sr.color = c;
        }

        void Update()
        {
            if (GM == null || Cfg == null) return;

            hurtFlash = Mathf.MoveTowards(hurtFlash, 0f, Time.deltaTime * 2.5f);
            robCooldown = Mathf.MoveTowards(robCooldown, 0f, Time.deltaTime);
            dropTimer = Mathf.MoveTowards(dropTimer, 0f, Time.deltaTime);

            if (GM.State == GameState.Playing)
            {
                Step(GameInput.Horizontal, Cfg.kingSpeed);
                ClampToWorld(1.5f);

                if (GameInput.PayHeld) TryPay();
                CollectCoins();
            }
            else
            {
                animTime = 0f;
            }

            Apply();
        }

        void TryPay()
        {
            if (Coins <= 0 || dropTimer > 0f) return;

            var sink = GM.NearestSink(X, Cfg.payRange);
            if (sink != null)
            {
                Coins--;
                dropTimer = Cfg.coinDropInterval;
                sink.Pay(1);
                Sfx.Play("drop", 0.5f);
                return;
            }

            // yakında ödeme noktası yok: altını yere bırak (köylüler üsse taşır)
            Coins--;
            dropTimer = Cfg.coinDropInterval * 1.5f;
            CoinPickup.Spawn(GM.WorldRoot, X + facing * 0.3f, 1, 2.5f);
            Sfx.Play("drop", 0.4f);
        }

        void CollectCoins()
        {
            if (Coins >= Capacity) return;
            var list = GM.Coins;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var c = list[i];
                if (c == null) { list.RemoveAt(i); continue; }
                if (Mathf.Abs(c.X - X) > Cfg.pickupRange) continue;
                int space = Capacity - Coins;
                if (space <= 0) break;
                int take = Mathf.Min(space, c.Value);
                if (c.Take())
                {
                    Coins += take;
                    Sfx.Play("coin", 0.45f);
                }
            }
        }

        public int Add(int amount)
        {
            int space = Mathf.Max(0, Capacity - Coins);
            int taken = Mathf.Min(space, amount);
            Coins += taken;
            return amount - taken;
        }

        /// <summary>Greed krala ulaştı: önce altınları, altın yoksa tacı alır.</summary>
        public void Robbed(Greed thief)
        {
            if (GM.State != GameState.Playing || robCooldown > 0f) return;
            robCooldown = 0.6f;
            hurtFlash = 1f;

            if (Coins > 0)
            {
                int stolen = Mathf.Min(Coins, Cfg.greedCoinSteal);
                Coins -= stolen;
                if (thief != null) thief.TakeLoot(stolen);
                Sfx.Play("steal", 0.9f);
                GM.Rig.Shake(0.22f);
                GM.Hud.Flash("ALTINLARIN CALINDI!", Pal.Cape);
            }
            else
            {
                Sfx.Play("gameover", 1f);
                GM.Rig.Shake(0.45f);
                GM.EndGame("TAC CALINDI");
            }
        }
    }
}
