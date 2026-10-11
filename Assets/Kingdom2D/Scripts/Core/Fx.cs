using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Küçük parçacık ve yükselen yazı efektleri.</summary>
    public class Fx : MonoBehaviour
    {
        Vector2 vel;
        float life, maxLife, gravity, spin;
        SpriteRenderer sr;

        public static void Burst(Vector2 pos, Color color, int count, float power, float size = 1f)
        {
            if (GameManager.I == null) return;
            for (int i = 0; i < count; i++)
            {
                var sr = Util.Spawn(GameManager.I.FxRoot, Art.Dot, pos, Layer.Fx, "fx");
                sr.color = color;
                sr.transform.localScale = Vector3.one * Util.Rand(0.6f, 1.6f) * size;
                var f = sr.gameObject.AddComponent<Fx>();
                f.sr = sr;
                f.vel = new Vector2(Util.Rand(-power, power), Util.Rand(power * 0.3f, power * 1.3f));
                f.maxLife = f.life = Util.Rand(0.3f, 0.7f);
                f.gravity = 9f;
                f.spin = Util.Rand(-4f, 4f);
            }
        }

        public static void FloatText(Vector2 pos, string text, Color color, float scale = 1f)
        {
            if (GameManager.I == null) return;
            var sr = Util.Spawn(GameManager.I.FxRoot, PixelFont.Make(text, color, 1, 0.5f, 0.5f), pos, Layer.Fx + 5, "fxtext");
            sr.transform.localScale = Vector3.one * scale;
            var f = sr.gameObject.AddComponent<Fx>();
            f.sr = sr;
            f.vel = new Vector2(0f, 1.3f);
            f.maxLife = f.life = 0.9f;
            f.gravity = 0f;
        }

        void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0f) { Destroy(gameObject); return; }
            vel.y -= gravity * Time.deltaTime;
            transform.position += (Vector3)(vel * Time.deltaTime);
            transform.Rotate(0f, 0f, spin);
            if (sr != null)
            {
                Color c = sr.color;
                sr.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(life / Mathf.Max(0.01f, maxLife)));
            }
        }
    }
}
