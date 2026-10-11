using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Yerdeki altın. Kral üstünden geçince toplar; köylüler üsse taşır.</summary>
    public class CoinPickup : MonoBehaviour
    {
        public int Value = 1;
        public bool Claimed;

        float y, vy;
        bool landed;
        float spin;
        float groundY;

        public float X { get { return transform.position.x; } }

        public static CoinPickup Spawn(Transform parent, float x, int value, float launchVy = 4f)
        {
            var sr = Util.Spawn(parent, Art.Coin, new Vector3(x, 0.3f, 0f), Layer.Coin, "coin");
            var c = sr.gameObject.AddComponent<CoinPickup>();
            c.Value = Mathf.Max(1, value);
            c.vy = launchVy;
            c.groundY = GameManager.I != null ? GameManager.I.Config.groundY : 0f;
            c.y = c.groundY + 0.35f;
            c.spin = Util.Rand(0f, 6f);
            if (GameManager.I != null) GameManager.I.Coins.Add(c);
            return c;
        }

        void Update()
        {
            if (!landed)
            {
                vy -= 16f * Time.deltaTime;
                y += vy * Time.deltaTime;
                float floor = groundY + 0.12f;
                if (y <= floor)
                {
                    y = floor;
                    if (vy < -1.5f) { vy = -vy * 0.35f; }
                    else { landed = true; vy = 0f; }
                }
                transform.position = new Vector3(transform.position.x, y, 0f);
            }

            spin += Time.deltaTime * 3.2f;
            float s = 0.35f + Mathf.Abs(Mathf.Cos(spin)) * 0.65f;
            transform.localScale = new Vector3(s, 1f, 1f);
            float bob = landed ? Mathf.Sin(spin * 0.7f) * 0.03f : 0f;
            transform.position = new Vector3(transform.position.x, y + bob, 0f);
        }

        /// <summary>Altını alır; başkası daha önce aldıysa false.</summary>
        public bool Take()
        {
            if (Claimed) return false;
            Claimed = true;
            if (GameManager.I != null) GameManager.I.Coins.Remove(this);
            Destroy(gameObject);
            return true;
        }
    }
}
