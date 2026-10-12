using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Gece karanlığında ateşlerin ve yapıların çevresini aydınlatan yumuşak hale.
    /// Karartma perdesinin üstüne çizilir, böylece gece okunabilir kalır.
    /// </summary>
    public class NightGlow : MonoBehaviour
    {
        SpriteRenderer sr;
        float strength = 0.5f;
        Color tint = new Color(1f, 0.72f, 0.38f);
        float flicker;
        public bool On = true;

        public static NightGlow Create(Transform parent, Vector3 localPos, float size, float strength, Color tint,
            bool flickering = true)
        {
            var sr = Util.Spawn(parent, Art.Glow(64), localPos, Layer.Night + 1, "glow");
            sr.transform.localScale = Vector3.one * size;
            var g = sr.gameObject.AddComponent<NightGlow>();
            g.sr = sr;
            g.strength = strength;
            g.tint = tint;
            g.flicker = flickering ? Util.Rand(0f, 6.28f) : -1f;
            return g;
        }

        void Update()
        {
            if (GameManager.I == null || sr == null) return;
            float dark = GameManager.I.Darkness;
            float f = 1f;
            if (flicker >= 0f)
            {
                flicker += Time.deltaTime * 4.5f;
                f = 0.86f + Mathf.Sin(flicker) * 0.08f + Mathf.Sin(flicker * 2.7f) * 0.06f;
            }
            float a = dark * strength * f * (On ? 1f : 0f);
            sr.enabled = a > 0.01f;
            sr.color = new Color(tint.r, tint.g, tint.b, a);
        }
    }
}
