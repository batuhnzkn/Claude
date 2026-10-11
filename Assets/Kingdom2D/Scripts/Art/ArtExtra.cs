using UnityEngine;

namespace Kingdom2D
{
    public static partial class Art
    {
        /// <summary>Arka plan orman şeridi (paralaks için yan yana dizilir).</summary>
        public static Sprite ForestStrip(int w, int h, int seed, Color color)
        {
            return Get("forest" + w + "_" + h + "_" + seed, () =>
            {
                var rng = new System.Random(seed);
                var p = new Pix(w, h);
                for (int i = 0; i < w / 5 + 2; i++)
                {
                    float cx = rng.Next(0, w);
                    int th = h / 2 + rng.Next(0, h / 2);
                    if (rng.Next(0, 100) < 65)
                    {
                        // çam
                        for (int l = 0; l < 3; l++)
                        {
                            float t = l / 2f;
                            float ww = Mathf.Lerp(th * 0.32f, th * 0.14f, t);
                            float y0 = Mathf.Lerp(0f, th * 0.55f, t);
                            p.Tri(new Vector2(cx - ww, y0), new Vector2(cx + ww, y0), new Vector2(cx, y0 + th * 0.5f), color);
                        }
                    }
                    else
                    {
                        p.Rect((int)cx, 0, 2, th / 2, color);
                        p.Disc(cx + 1, th * 0.55f, th * 0.27f, color);
                    }
                }
                p.Rect(0, 0, w, 2, color);
                return p.ToSprite(0f, 0f);
            });
        }

        /// <summary>Dikey degrade gökyüzü dokusu; renkleri her kare güncellenebilir.</summary>
        public static Sprite MakeGradientSprite(int h, out Texture2D tex)
        {
            tex = new Texture2D(1, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            var sp = Sprite.Create(tex, new Rect(0, 0, 1, h), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect);
            sp.name = "gradient";
            return sp;
        }
    }
}
