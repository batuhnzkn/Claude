using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Kod ile piksel çizmek için küçük bir tuval. Oyundaki tüm grafikler
    /// (karakterler, yapılar, arka plan) çalışma anında bununla üretilir;
    /// projede hiçbir hazır görsel dosyası yoktur.
    /// </summary>
    public class Pix
    {
        public const float Ppu = 16f; // birim başına piksel

        public readonly int W;
        public readonly int H;
        readonly Color32[] px;

        public Pix(int w, int h)
        {
            W = Mathf.Max(1, w);
            H = Mathf.Max(1, h);
            px = new Color32[W * H];
            Color32 clear = new Color32(0, 0, 0, 0);
            for (int i = 0; i < px.Length; i++) px[i] = clear;
        }

        public void Set(int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= W || y >= H || c.a <= 0f) return;
            px[y * W + x] = c;
        }

        public Color Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return Pal.Clear;
            return px[y * W + x];
        }

        public void Fill(Color c)
        {
            for (int i = 0; i < px.Length; i++) px[i] = c;
        }

        public void Rect(int x, int y, int w, int h, Color c)
        {
            for (int j = y; j < y + h; j++)
                for (int i = x; i < x + w; i++)
                    Set(i, j, c);
        }

        public void Outline(int x, int y, int w, int h, Color c)
        {
            for (int i = x; i < x + w; i++) { Set(i, y, c); Set(i, y + h - 1, c); }
            for (int j = y; j < y + h; j++) { Set(x, j, c); Set(x + w - 1, j, c); }
        }

        public void Disc(float cx, float cy, float r, Color c)
        {
            int x0 = Mathf.FloorToInt(cx - r), x1 = Mathf.CeilToInt(cx + r);
            int y0 = Mathf.FloorToInt(cy - r), y1 = Mathf.CeilToInt(cy + r);
            float r2 = r * r;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    if (dx * dx + dy * dy <= r2) Set(x, y, c);
                }
        }

        public void Ellipse(float cx, float cy, float rx, float ry, Color c)
        {
            int x0 = Mathf.FloorToInt(cx - rx), x1 = Mathf.CeilToInt(cx + rx);
            int y0 = Mathf.FloorToInt(cy - ry), y1 = Mathf.CeilToInt(cy + ry);
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float dx = (x - cx) / Mathf.Max(0.001f, rx);
                    float dy = (y - cy) / Mathf.Max(0.001f, ry);
                    if (dx * dx + dy * dy <= 1f) Set(x, y, c);
                }
        }

        public void Line(int x0, int y0, int x1, int y1, Color c, int thickness = 1)
        {
            int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            int guard = 0;
            while (guard++ < 4096)
            {
                if (thickness <= 1) Set(x0, y0, c);
                else Rect(x0 - thickness / 2, y0 - thickness / 2, thickness, thickness, c);
                if (x0 == x1 && y0 == y1) break;
                int e2 = err * 2;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        public void Tri(Vector2 a, Vector2 b, Vector2 cc, Color col)
        {
            int minX = Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, cc.x)));
            int maxX = Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, cc.x)));
            int minY = Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, cc.y)));
            int maxY = Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, cc.y)));
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    float d1 = Edge(a, b, p), d2 = Edge(b, cc, p), d3 = Edge(cc, a, p);
                    bool neg = d1 <= 0 && d2 <= 0 && d3 <= 0;
                    bool pos = d1 >= 0 && d2 >= 0 && d3 >= 0;
                    if (neg || pos) Set(x, y, col);
                }
        }

        static float Edge(Vector2 a, Vector2 b, Vector2 p)
        {
            return (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
        }

        /// <summary>Karakter haritası basar. rows[0] en üst satırdır.</summary>
        public void Rows(string[] rows, int ox = 0, int oy = 0)
        {
            for (int r = 0; r < rows.Length; r++)
            {
                string row = rows[r];
                int y = oy + (rows.Length - 1 - r);
                for (int i = 0; i < row.Length; i++)
                {
                    char ch = row[i];
                    if (ch == '.' || ch == ' ') continue;
                    Set(ox + i, y, Pal.Of(ch));
                }
            }
        }

        /// <summary>Saydam olmayan tüm pikselleri tek renge boyar (arayüz ikonları için).</summary>
        public void Recolor(Color c)
        {
            for (int i = 0; i < px.Length; i++)
                if (px[i].a > 0) px[i] = c;
        }

        /// <summary>Üstten gelen ışık hissi için her sütunun en üst pikselini aydınlatır.</summary>
        public void TopLight(Color c, int depth = 1)
        {
            for (int x = 0; x < W; x++)
            {
                int painted = 0;
                for (int y = H - 1; y >= 0 && painted < depth; y--)
                {
                    if (Get(x, y).a > 0f) { Set(x, y, c); painted++; }
                }
            }
        }

        public Texture2D ToTexture()
        {
            var t = new Texture2D(W, H, TextureFormat.RGBA32, false);
            t.filterMode = FilterMode.Point;
            t.wrapMode = TextureWrapMode.Clamp;
            t.SetPixels32(px);
            t.Apply();
            return t;
        }

        /// <summary>pivotX/pivotY 0..1 aralığında. Karakterler için (0.5, 0) = ayak hizası.</summary>
        public Sprite ToSprite(float pivotX = 0.5f, float pivotY = 0f, float ppu = Ppu)
        {
            var tex = ToTexture();
            var sp = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(pivotX, pivotY), ppu, 0,
                SpriteMeshType.FullRect);
            sp.name = "pix_" + W + "x" + H;
            return sp;
        }
    }
}
