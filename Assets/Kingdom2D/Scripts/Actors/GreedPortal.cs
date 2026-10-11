using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Haritanın iki ucundaki greed delikleri: gece dalgalarını buradan gönderir.</summary>
    public class GreedPortal : MonoBehaviour
    {
        public int Side = -1;
        public float X;

        int remaining;
        float timer;
        float interval = 2f;
        int day = 1;
        bool blood;
        bool active;
        float anim;
        SpriteRenderer sr;

        public static GreedPortal Create(Transform parent, float x, int side)
        {
            var go = Util.Node(parent, "GreedPortal", new Vector3(x, 0f, 0f));
            var p = go.AddComponent<GreedPortal>();
            p.X = x;
            p.Side = side;
            p.sr = Util.Spawn(go.transform, Art.Portal(0), Vector3.zero, Layer.Portal, "portal");
            if (GameManager.I != null) GameManager.I.Portals.Add(p);
            return p;
        }

        void OnDestroy()
        {
            if (GameManager.I != null) GameManager.I.Portals.Remove(this);
        }

        public void BeginNight(int dayNumber, bool bloodMoon)
        {
            day = dayNumber;
            blood = bloodMoon;
            var cfg = GameManager.I.Config;

            float count = cfg.waveBase + (day - 1) * cfg.waveGrowth;
            if (blood) count *= cfg.bloodMoonMultiplier;
            remaining = Mathf.Max(1, Mathf.RoundToInt(count));

            float window = cfg.nightLength * 0.5f;
            interval = Mathf.Clamp(window / remaining, 0.35f, 4f);
            timer = Util.Rand(0.2f, 1.2f);
            active = true;
        }

        public void EndNight()
        {
            active = false;
            remaining = 0;
        }

        void Update()
        {
            anim += Time.deltaTime * 3.5f;
            if (sr != null) sr.sprite = Art.Portal(((int)anim) % 3);

            if (!active || GameManager.I == null || GameManager.I.State != GameState.Playing) return;
            if (remaining <= 0) return;

            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = interval * Util.Rand(0.7f, 1.3f);
            remaining--;

            float hpScale = 1f + (day - 1) * 0.07f;
            var type = PickType();
            Greed.Spawn(GameManager.I.ActorRoot, X - Side * Util.Rand(0f, 2f), Side, type, hpScale);
            Fx.Burst(new Vector2(X, 0.4f), Pal.Eye, 4, 2f, 0.7f);
        }

        GreedType PickType()
        {
            float r = Random.value;
            int d = blood ? day + 3 : day;

            if (d < 3) return GreedType.Floater;
            if (d < 6) return r < 0.6f ? GreedType.Floater : GreedType.Walker;
            if (d < 9)
            {
                if (r < 0.35f) return GreedType.Floater;
                if (r < 0.78f) return GreedType.Walker;
                if (r < 0.9f) return GreedType.Brute;
                return GreedType.Flyer;
            }
            if (r < 0.2f) return GreedType.Floater;
            if (r < 0.6f) return GreedType.Walker;
            if (r < 0.82f) return GreedType.Brute;
            return GreedType.Flyer;
        }
    }
}
