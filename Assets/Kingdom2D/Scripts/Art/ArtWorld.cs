using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Art sınıfının yapılar, bitkiler ve manzara kısmı (geometrik üretim).</summary>
    public static partial class Art
    {
        // --------------------------------------------------------------- Duvarlar

        /// <summary>Duvar kademesinin dünya birimi cinsinden yüksekliği (okçu platformu).</summary>
        public static float WallHeight(int tier)
        {
            switch (tier)
            {
                case 1: return 14f / Pix.Ppu;
                case 2: return 22f / Pix.Ppu;
                case 3: return 30f / Pix.Ppu;
                default: return 0f;
            }
        }

        public static Sprite Wall(int tier, bool damaged)
        {
            return Get("wall" + tier + (damaged ? "d" : ""), () =>
            {
                var rng = new System.Random(1000 + tier);
                Pix p;
                if (tier <= 1)
                {
                    p = new Pix(30, 16);
                    for (int x = 1; x < 29; x += 4)
                    {
                        int hh = 11 + rng.Next(0, 4);
                        p.Rect(x, 0, 3, hh, Pal.Wood);
                        p.Tri(new Vector2(x, hh - 1), new Vector2(x + 2, hh - 1), new Vector2(x + 1, hh + 2), Pal.Wood);
                        p.Rect(x, 0, 1, hh, Pal.WoodDark);
                    }
                    p.Rect(0, 5, 30, 2, Pal.WoodDark);
                }
                else if (tier == 2)
                {
                    p = new Pix(34, 26);
                    p.Rect(2, 0, 30, 20, Pal.Wood);
                    for (int x = 2; x < 32; x += 4) p.Rect(x, 0, 1, 20, Pal.WoodDark);
                    p.Rect(0, 19, 34, 3, Pal.WoodDark);           // yürüyüş platformu
                    p.Rect(0, 22, 34, 1, Pal.Wood);
                    for (int x = 1; x < 33; x += 5) p.Rect(x, 22, 3, 4, Pal.Wood);
                    p.Rect(14, 2, 6, 10, Pal.WoodDark);           // kapı
                    p.Rect(2, 0, 30, 1, Pal.Dark);
                }
                else
                {
                    p = new Pix(40, 34);
                    p.Rect(1, 0, 38, 28, Pal.Stone);
                    for (int y = 0; y < 28; y += 4)
                    {
                        p.Rect(1, y, 38, 1, Pal.StoneDark);
                        int off = (y / 4) % 2 == 0 ? 0 : 3;
                        for (int x = 1 + off; x < 39; x += 6) p.Rect(x, y, 1, 4, Pal.StoneDark);
                    }
                    p.Rect(0, 27, 40, 3, Pal.StoneDark);          // platform
                    for (int x = 1; x < 39; x += 7) p.Rect(x, 30, 4, 4, Pal.Stone);
                    p.Tri(new Vector2(15, 0), new Vector2(24, 0), new Vector2(19.5f, 13), Pal.StoneDark);
                    p.Rect(15, 0, 10, 8, Pal.StoneDark);          // kemerli kapı
                }

                if (damaged)
                {
                    for (int i = 0; i < p.W * p.H / 14; i++)
                    {
                        int x = rng.Next(0, p.W), y = rng.Next(0, p.H);
                        if (p.Get(x, y).a > 0f && rng.Next(0, 100) < 55) p.Rect(x, y, 2, 2, Pal.Clear);
                    }
                }
                p.TopLight(new Color(1f, 1f, 1f, 0.12f));
                return p.ToSprite(0.5f, 0f);
            });
        }

        // ------------------------------------------------------------------- Üs

        public static float CampHeight(int tier)
        {
            switch (tier) { case 2: return 36f / Pix.Ppu; case 3: return 52f / Pix.Ppu; default: return 24f / Pix.Ppu; }
        }

        public static Sprite Camp(int tier)
        {
            return Get("camp" + tier, () =>
            {
                Pix p;
                if (tier <= 1)
                {
                    p = new Pix(30, 26);
                    // taş halka + odunlar
                    for (int x = 6; x < 24; x += 4) p.Ellipse(x, 2, 2.4f, 1.8f, Pal.StoneDark);
                    p.Line(8, 3, 20, 7, Pal.Wood, 2);
                    p.Line(8, 7, 20, 3, Pal.WoodDark, 2);
                    // bayrak direği
                    p.Rect(25, 0, 2, 24, Pal.WoodDark);
                    p.Tri(new Vector2(22, 24), new Vector2(27, 21), new Vector2(27, 16), Pal.Cape);
                    p.Rect(22, 16, 5, 8, Pal.Cape);
                }
                else if (tier == 2)
                {
                    p = new Pix(46, 36);
                    p.Rect(5, 0, 36, 20, Pal.Wood);
                    for (int x = 5; x < 41; x += 5) p.Rect(x, 0, 1, 20, Pal.WoodDark);
                    p.Tri(new Vector2(1, 19), new Vector2(45, 19), new Vector2(23, 31), Pal.WoodDark);
                    p.Rect(19, 0, 8, 12, Pal.Dark);               // kapı
                    p.Rect(10, 12, 5, 5, Pal.Dark);               // pencereler
                    p.Rect(31, 12, 5, 5, Pal.Dark);
                    p.Rect(22, 30, 2, 6, Pal.WoodDark);
                    p.Rect(24, 31, 6, 4, Pal.Cape);
                }
                else
                {
                    p = new Pix(66, 52);
                    p.Rect(4, 0, 58, 30, Pal.Stone);
                    for (int y = 0; y < 30; y += 5)
                    {
                        p.Rect(4, y, 58, 1, Pal.StoneDark);
                        int off = (y / 5) % 2 == 0 ? 0 : 4;
                        for (int x = 4 + off; x < 62; x += 8) p.Rect(x, y, 1, 5, Pal.StoneDark);
                    }
                    // iki kule
                    p.Rect(0, 0, 12, 44, Pal.Stone);
                    p.Rect(54, 0, 12, 44, Pal.Stone);
                    for (int i = 0; i < 2; i++)
                    {
                        int bx = i == 0 ? 0 : 54;
                        for (int x = bx; x < bx + 12; x += 4) p.Rect(x, 44, 3, 5, Pal.StoneDark);
                        p.Rect(bx, 42, 12, 2, Pal.StoneDark);
                        p.Rect(bx + 4, 24, 4, 7, Pal.Dark);
                    }
                    for (int x = 14; x < 52; x += 7) p.Rect(x, 30, 4, 4, Pal.StoneDark);
                    p.Rect(26, 0, 14, 20, Pal.Dark);              // ana kapı
                    p.Tri(new Vector2(26, 19), new Vector2(40, 19), new Vector2(33, 26), Pal.Dark);
                    p.Rect(32, 49, 2, 3, Pal.WoodDark);
                    p.Rect(34, 49, 7, 3, Pal.Cape);
                }
                p.TopLight(new Color(1f, 1f, 1f, 0.1f));
                return p.ToSprite(0.5f, 0f);
            });
        }

        public static Sprite Flame(int frame)
        {
            return Get("flame" + frame, () =>
            {
                var p = new Pix(10, 14);
                float w = frame == 0 ? 3.2f : (frame == 1 ? 2.6f : 3.6f);
                int h = frame == 0 ? 11 : (frame == 1 ? 13 : 9);
                p.Tri(new Vector2(5 - w, 0), new Vector2(5 + w, 0), new Vector2(5, h), Pal.Flame);
                p.Tri(new Vector2(5 - w * 0.5f, 0), new Vector2(5 + w * 0.5f, 0), new Vector2(5, h * 0.6f), Pal.FlameHot);
                return p.ToSprite(0.5f, 0f);
            });
        }

        // ---------------------------------------------------------------- Tarla

        public static Sprite Farm(int stage)
        {
            return Get("farm" + stage, () =>
            {
                var p = new Pix(38, 16);
                p.Rect(0, 0, 38, 4, Pal.Soil);
                for (int x = 1; x < 38; x += 6) p.Rect(x, 3, 3, 1, Pal.Dark2);
                // çit
                p.Rect(0, 0, 2, 9, Pal.WoodDark);
                p.Rect(36, 0, 2, 9, Pal.WoodDark);
                p.Rect(0, 7, 38, 1, Pal.Wood);
                if (stage >= 1)
                {
                    Color c = stage >= 2 ? Pal.Crop : Pal.Leaf;
                    int h = stage >= 2 ? 9 : 4;
                    for (int x = 3; x < 36; x += 4)
                    {
                        p.Rect(x, 3, 1, h, c);
                        if (stage >= 2) { p.Rect(x - 1, 3 + h - 2, 3, 2, c); }
                    }
                }
                return p.ToSprite(0.5f, 0f);
            });
        }

        // ------------------------------------------------------------- Tezgahlar

        public static Sprite Stand(StandKind kind)
        {
            return Get("stand" + (int)kind, () =>
            {
                var p = new Pix(16, 22);
                // tezgah
                p.Rect(2, 0, 2, 8, Pal.WoodDark);
                p.Rect(12, 0, 2, 8, Pal.WoodDark);
                p.Rect(1, 8, 14, 3, Pal.Wood);
                p.Rect(1, 8, 14, 1, Pal.WoodDark);
                // alet
                switch (kind)
                {
                    case StandKind.Bow:
                        p.Line(6, 12, 6, 20, Pal.Wood, 1);
                        p.Line(6, 20, 9, 18, Pal.Wood, 1);
                        p.Line(9, 18, 10, 16, Pal.Wood, 1);
                        p.Line(10, 16, 9, 14, Pal.Wood, 1);
                        p.Line(9, 14, 6, 12, Pal.Wood, 1);
                        p.Rect(5, 12, 1, 9, Pal.White);
                        break;
                    case StandKind.Hammer:
                        p.Rect(7, 11, 2, 9, Pal.Wood);
                        p.Rect(4, 18, 8, 4, Pal.MetalDark);
                        p.Rect(4, 20, 8, 1, Pal.Metal);
                        break;
                    case StandKind.Scythe:
                        p.Rect(8, 11, 2, 10, Pal.Wood);
                        p.Line(8, 20, 3, 18, Pal.Metal, 1);
                        p.Line(3, 18, 2, 15, Pal.Metal, 1);
                        p.Rect(7, 20, 2, 1, Pal.Metal);
                        break;
                    case StandKind.Sword:
                        p.Rect(7, 12, 2, 9, Pal.Metal);
                        p.Tri(new Vector2(7, 20), new Vector2(9, 20), new Vector2(8, 22), Pal.Metal);
                        p.Rect(5, 12, 6, 1, Pal.MetalDark);
                        p.Rect(7, 11, 2, 1, Pal.Wood);
                        break;
                }
                return p.ToSprite(0.5f, 0f);
            });
        }

        // ------------------------------------------------------------- Greed deliği

        public static Sprite Portal(int frame)
        {
            return Get("portal" + frame, () =>
            {
                var p = new Pix(32, 20);
                p.Ellipse(16, 4, 15f, 4.5f, Pal.Dark3);
                p.Ellipse(16, 5, 12f, 3.6f, Pal.GreedSkin);
                float glow = frame == 0 ? 2.2f : (frame == 1 ? 3.2f : 2.7f);
                p.Ellipse(16, 5, glow * 2.4f, glow, new Color(Pal.Eye.r, Pal.Eye.g, Pal.Eye.b, 0.55f));
                // dişler / kaya parçaları
                for (int i = 0; i < 7; i++)
                {
                    float x = 3 + i * 4.2f;
                    p.Tri(new Vector2(x, 6), new Vector2(x + 3, 6), new Vector2(x + 1.5f, 11 + (i % 3) * 3), Pal.Dark3);
                }
                return p.ToSprite(0.5f, 0f);
            });
        }

        public static Sprite Tent
        {
            get
            {
                return Get("tent", () =>
                {
                    var p = new Pix(28, 20);
                    p.Tri(new Vector2(1, 0), new Vector2(27, 0), new Vector2(14, 18), Pal.Dark2);
                    p.Tri(new Vector2(10, 0), new Vector2(18, 0), new Vector2(14, 11), Pal.Dark3);
                    p.Line(14, 17, 14, 20, Pal.WoodDark, 1);
                    p.Rect(0, 0, 28, 1, Pal.Dark3);
                    return p.ToSprite(0.5f, 0f);
                });
            }
        }

        // ---------------------------------------------------------------- Bitkiler

        public static Sprite Pine(int variant)
        {
            return Get("pine" + variant, () =>
            {
                var rng = new System.Random(77 + variant);
                int h = 34 + rng.Next(0, 10);
                var p = new Pix(24, h);
                p.Rect(11, 0, 3, h / 3, Pal.WoodDark);
                int layers = 4;
                for (int i = 0; i < layers; i++)
                {
                    float t = i / (float)(layers - 1);
                    float w = Mathf.Lerp(11f, 4f, t);
                    float y0 = Mathf.Lerp(h * 0.18f, h * 0.72f, t);
                    float y1 = y0 + h * 0.3f;
                    p.Tri(new Vector2(12 - w, y0), new Vector2(12 + w, y0), new Vector2(12, y1), i % 2 == 0 ? Pal.LeafDark : Pal.Leaf);
                }
                return p.ToSprite(0.5f, 0f);
            });
        }

        public static Sprite Oak(int variant)
        {
            return Get("oak" + variant, () =>
            {
                var rng = new System.Random(133 + variant);
                var p = new Pix(32, 32);
                p.Rect(14, 0, 4, 14, Pal.WoodDark);
                p.Line(16, 10, 10, 16, Pal.WoodDark, 2);
                p.Line(16, 10, 22, 16, Pal.WoodDark, 2);
                for (int i = 0; i < 7; i++)
                {
                    float cx = 16 + (rng.Next(0, 160) - 80) / 10f;
                    float cy = 20 + (rng.Next(0, 90) - 30) / 10f;
                    p.Disc(cx, cy, 6.5f - i * 0.3f, i % 2 == 0 ? Pal.LeafDark : Pal.Leaf);
                }
                return p.ToSprite(0.5f, 0f);
            });
        }

        public static Sprite Bush
        {
            get
            {
                return Get("bush", () =>
                {
                    var p = new Pix(18, 12);
                    p.Disc(6, 4, 5f, Pal.LeafDark);
                    p.Disc(12, 5, 5.5f, Pal.Leaf);
                    p.Rect(0, 0, 18, 1, Pal.LeafDark);
                    return p.ToSprite(0.5f, 0f);
                });
            }
        }

        public static Sprite Grass(int variant)
        {
            return Get("grass" + variant, () =>
            {
                var rng = new System.Random(9 + variant);
                var p = new Pix(9, 7);
                for (int i = 0; i < 5; i++)
                {
                    int x = 1 + i * 2;
                    int h = 3 + rng.Next(0, 4);
                    p.Line(x, 0, x + (i - 2), h, Pal.LeafDark, 1);
                }
                return p.ToSprite(0.5f, 0f);
            });
        }

        public static Sprite Rock(int variant)
        {
            return Get("rock" + variant, () =>
            {
                var p = new Pix(14, 9);
                p.Ellipse(7, 1, 6.5f, 5f, Pal.StoneDark);
                p.Ellipse(5, 2, 3.5f, 3f, Pal.Stone);
                p.Rect(0, 0, 14, 1, Pal.Dark3);
                return p.ToSprite(0.5f, 0f);
            });
        }

        // ---------------------------------------------------------------- Gökyüzü

        public static Sprite Sun
        {
            get
            {
                return Get("sun", () =>
                {
                    var p = new Pix(28, 28);
                    p.Disc(14, 14, 13f, new Color(1f, 0.85f, 0.4f, 0.18f));
                    p.Disc(14, 14, 10f, new Color(1f, 0.9f, 0.55f, 0.45f));
                    p.Disc(14, 14, 7.5f, new Color(1f, 0.96f, 0.78f, 1f));
                    return p.ToSprite(0.5f, 0.5f);
                });
            }
        }

        public static Sprite Moon
        {
            get
            {
                return Get("moon", () =>
                {
                    var p = new Pix(24, 24);
                    p.Disc(12, 12, 11f, new Color(1f, 1f, 1f, 0.14f));
                    p.Disc(12, 12, 8.5f, Pal.White);
                    p.Disc(9, 14, 2.2f, new Color(0.85f, 0.85f, 0.8f, 1f));
                    p.Disc(15, 9, 1.6f, new Color(0.85f, 0.85f, 0.8f, 1f));
                    p.Disc(14, 15, 1.2f, new Color(0.85f, 0.85f, 0.8f, 1f));
                    return p.ToSprite(0.5f, 0.5f);
                });
            }
        }

        public static Sprite Cloud(int variant)
        {
            return Get("cloud" + variant, () =>
            {
                var rng = new System.Random(300 + variant);
                var p = new Pix(56, 18);
                for (int i = 0; i < 6; i++)
                {
                    float cx = 8 + i * 8 + rng.Next(0, 5);
                    float cy = 5 + rng.Next(0, 5);
                    float r = 5 + rng.Next(0, 4);
                    p.Ellipse(cx, cy, r * 1.3f, r * 0.85f, new Color(1f, 1f, 1f, 0.9f));
                }
                p.Rect(0, 0, 56, 2, Pal.Clear);
                return p.ToSprite(0.5f, 0.5f);
            });
        }

        /// <summary>Kenarları birbirine uyan (döngüsel) tepe silueti: paralaks için yan yana dizilir.</summary>
        public static Sprite Hill(int w, int h, int seed, Color color, float amp = 0.45f)
        {
            return Get("hill" + w + "_" + h + "_" + seed, () =>
            {
                var rng = new System.Random(seed);
                var p = new Pix(w, h);
                float p1 = rng.Next(0, 628) / 100f, p2 = rng.Next(0, 628) / 100f, p3 = rng.Next(0, 628) / 100f;
                for (int x = 0; x < w; x++)
                {
                    float u = x / (float)w * Mathf.PI * 2f;
                    float n = Mathf.Sin(u + p1) * 0.55f + Mathf.Sin(u * 2f + p2) * 0.3f + Mathf.Sin(u * 3f + p3) * 0.15f;
                    int top = Mathf.RoundToInt(h * (0.5f + n * amp));
                    top = Mathf.Clamp(top, 2, h - 1);
                    p.Rect(x, 0, 1, top, color);
                }
                return p.ToSprite(0f, 0f);
            });
        }

        public static Sprite Stars(int w, int h, int count, int seed)
        {
            return Get("stars" + w + "_" + h + "_" + seed, () =>
            {
                var rng = new System.Random(seed);
                var p = new Pix(w, h);
                for (int i = 0; i < count; i++)
                {
                    int x = rng.Next(0, w), y = rng.Next(0, h);
                    float a = 0.35f + rng.Next(0, 65) / 100f;
                    p.Set(x, y, new Color(1f, 1f, 1f, a));
                    if (rng.Next(0, 100) < 12)
                    {
                        p.Set(x + 1, y, new Color(1f, 1f, 1f, a * 0.5f));
                        p.Set(x, y + 1, new Color(1f, 1f, 1f, a * 0.5f));
                    }
                }
                return p.ToSprite(0.5f, 0.5f);
            });
        }

        /// <summary>Zemin dokusu: üstte çim bandı, altta toprak.</summary>
        public static Sprite GroundStrip(int w, int h, int seed)
        {
            return Get("ground" + w + "_" + h + "_" + seed, () =>
            {
                var rng = new System.Random(seed);
                var p = new Pix(w, h);
                p.Rect(0, 0, w, h, Pal.GroundBody);
                p.Rect(0, 0, w, Mathf.Max(1, h / 3), Pal.GroundDeep);
                p.Rect(0, h - 3, w, 3, Pal.GroundTop);
                for (int x = 0; x < w; x++)
                {
                    if (rng.Next(0, 100) < 22) p.Set(x, h - 4, Pal.GroundTop);
                    if (rng.Next(0, 100) < 10) p.Rect(x, h - 7 - rng.Next(0, 6), 1, 2, Pal.Dark3);
                }
                return p.ToSprite(0f, 1f);
            });
        }
    }
}
