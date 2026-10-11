using UnityEngine;

namespace Kingdom2D
{
    public static class Util
    {
        /// <summary>Verilen sprite ile yeni bir görsel nesne oluşturur.</summary>
        public static SpriteRenderer Spawn(Transform parent, Sprite sprite, Vector3 pos, int order, string name = "sprite")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        public static GameObject Node(Transform parent, string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            return go;
        }

        public static float Rand(float a, float b) { return Random.Range(a, b); }

        public static int RandInt(int a, int b) { return Random.Range(a, b); }

        public static bool Chance(float p01) { return Random.value < p01; }

        public static float Sign0(float v) { return v > 0.001f ? 1f : (v < -0.001f ? -1f : 0f); }

        /// <summary>İki ucu arasında yumuşak 0..1 geçişi.</summary>
        public static float Ramp(float v, float a, float b)
        {
            if (Mathf.Abs(b - a) < 0.0001f) return v >= b ? 1f : 0f;
            return Mathf.Clamp01((v - a) / (b - a));
        }

        public static Color WithAlpha(Color c, float a) { return new Color(c.r, c.g, c.b, a); }

        /// <summary>Kareye yakın yumuşak darbe eğrisi (vuruş/titreme efektleri için).</summary>
        public static float Pulse(float t, float speed) { return (Mathf.Sin(t * speed) + 1f) * 0.5f; }
    }
}
