using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Okçuların attığı ok. Yerçekimiyle düşer, isabet ederse hasar verir.</summary>
    public class Arrow : MonoBehaviour
    {
        const float Gravity = 11f;

        Vector2 pos;
        Vector2 vel;
        float damage;
        float life = 5f;
        bool stuck;
        float fade = 1f;
        SpriteRenderer sr;

        public static Arrow Fire(Transform parent, Vector2 from, Vector2 to, float speed, float damage)
        {
            var sr = Util.Spawn(parent, Art.Arrow, from, Layer.Arrow, "Arrow");
            var a = sr.gameObject.AddComponent<Arrow>();
            a.sr = sr;
            a.pos = from;
            a.damage = damage;

            Vector2 delta = to - from;
            float dist = Mathf.Max(0.3f, delta.magnitude);
            float t = dist / Mathf.Max(1f, speed);
            Vector2 aim = to + new Vector2(0f, 0.5f * Gravity * t * t);
            a.vel = (aim - from).normalized * speed;
            return a;
        }

        void Update()
        {
            if (GameManager.I == null) return;
            life -= Time.deltaTime;
            if (life <= 0f) { Destroy(gameObject); return; }

            if (stuck)
            {
                fade -= Time.deltaTime * 0.6f;
                if (sr != null) sr.color = new Color(1f, 1f, 1f, Mathf.Clamp01(fade));
                if (fade <= 0f) Destroy(gameObject);
                return;
            }

            vel.y -= Gravity * Time.deltaTime;
            pos += vel * Time.deltaTime;
            transform.position = pos;
            float ang = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, ang);

            var gm = GameManager.I;

            for (int i = gm.Greeds.Count - 1; i >= 0; i--)
            {
                var g = gm.Greeds[i];
                if (g == null || !g.Alive) continue;
                if (Hits(g.X, g.GroundLevel + g.Height, g.Rend)) { g.Hurt(damage); Impact(); return; }
            }

            for (int i = gm.Critters.Count - 1; i >= 0; i--)
            {
                var c = gm.Critters[i];
                if (c == null || !c.Alive) continue;
                if (Hits(c.X, c.GroundLevel, c.Rend)) { c.Hurt(damage); Impact(); return; }
            }

            float ground = gm.Config.groundY;
            if (pos.y <= ground + 0.05f)
            {
                pos.y = ground + 0.05f;
                transform.position = pos;
                stuck = true;
                Sfx.Play("arrowhit", 0.2f);
            }
        }

        bool Hits(float targetX, float footY, SpriteRenderer target)
        {
            float halfW = 0.3f;
            float h = 0.7f;
            if (target != null && target.sprite != null)
            {
                halfW = Mathf.Max(0.22f, target.sprite.bounds.size.x * 0.45f);
                h = target.sprite.bounds.size.y;
            }
            return Mathf.Abs(pos.x - targetX) <= halfW && pos.y >= footY - 0.1f && pos.y <= footY + h + 0.1f;
        }

        void Impact()
        {
            Sfx.Play("arrowhit", 0.35f);
            Destroy(gameObject);
        }
    }
}
