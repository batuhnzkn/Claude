using System.Collections.Generic;
using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// 3x5 piksel font. Harici font dosyasına / UI paketine ihtiyaç duymadan
    /// yazıyı sprite olarak üretir (HUD de dünya uzayında sprite ile çizilir).
    /// Türkçe karakterler ASCII karşılığına indirgenir.
    /// </summary>
    public static class PixelFont
    {
        const int GW = 3, GH = 5, Gap = 1;

        static readonly Dictionary<char, string> G = new Dictionary<char, string>
        {
            { 'A', "010101111101101" }, { 'B', "110101110101110" }, { 'C', "011100100100011" },
            { 'D', "110101101101110" }, { 'E', "111100110100111" }, { 'F', "111100110100100" },
            { 'G', "011100101101011" }, { 'H', "101101111101101" }, { 'I', "111010010010111" },
            { 'J', "001001001101011" }, { 'K', "101101110101101" }, { 'L', "100100100100111" },
            { 'M', "101111111101101" }, { 'N', "110101101101101" }, { 'O', "010101101101010" },
            { 'P', "110101110100100" }, { 'Q', "010101101011001" }, { 'R', "110101110101101" },
            { 'S', "011100010001110" }, { 'T', "111010010010010" }, { 'U', "101101101101111" },
            { 'V', "101101101101010" }, { 'W', "101101111111101" }, { 'X', "101101010101101" },
            { 'Y', "101101010010010" }, { 'Z', "111001010100111" },
            { '0', "111101101101111" }, { '1', "010110010010111" }, { '2', "111001111100111" },
            { '3', "111001111001111" }, { '4', "101101111001001" }, { '5', "111100111001111" },
            { '6', "111100111101111" }, { '7', "111001010010010" }, { '8', "111101111101111" },
            { '9', "111101111001111" },
            { ' ', "000000000000000" }, { '-', "000000111000000" }, { '.', "000000000000010" },
            { ':', "000010000010000" }, { '!', "010010010000010" }, { '?', "110001010000010" },
            { '/', "001001010100100" }, { '+', "000010111010000" }, { '=', "000111000111000" },
            { ',', "000000000010100" }, { '\'', "010010000000000" }, { '(', "001010010010001" },
            { ')', "100010010010100" }, { '<', "001010100010001" }, { '>', "100010001010100" },
            { '*', "000101010101000" }, { '%', "101001010100101" }, { 'x', "000101010010101" },
        };

        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        const int CacheLimit = 400;

        /// <summary>Önbellek şiştiğinde (saatler süren oturumlar) dokuları serbest bırakır.</summary>
        static void TrimCache()
        {
            if (cache.Count <= CacheLimit) return;
            foreach (var kv in cache)
            {
                var sp = kv.Value;
                if (sp == null) continue;
                var tex = sp.texture;
                Object.Destroy(sp);
                if (tex != null) Object.Destroy(tex);
            }
            cache.Clear();
        }

        public static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new System.Text.StringBuilder(s.Length);
            foreach (char raw in s)
            {
                char c = raw;
                switch (c)
                {
                    case 'Ç': case 'ç': c = 'C'; break;
                    case 'Ğ': case 'ğ': c = 'G'; break;
                    case 'İ': case 'ı': c = 'I'; break;
                    case 'Ö': case 'ö': c = 'O'; break;
                    case 'Ş': case 'ş': c = 'S'; break;
                    case 'Ü': case 'ü': c = 'U'; break;
                }
                if (c != 'x') c = char.ToUpperInvariant(c);
                sb.Append(G.ContainsKey(c) ? c : ' ');
            }
            return sb.ToString();
        }

        public static int Width(string text, int scale)
        {
            string s = Normalize(text);
            if (s.Length == 0) return 0;
            return (s.Length * (GW + Gap) - Gap) * scale;
        }

        public static int Height(int scale) { return GH * scale; }

        /// <summary>Yazıyı sprite'a çevirir (sol-orta pivot). Sonuç önbelleğe alınır.</summary>
        public static Sprite Make(string text, Color color, int scale = 1, float pivotX = 0f, float pivotY = 0.5f)
        {
            string s = Normalize(text);
            if (s.Length == 0) s = " ";
            string key = s + "|" + color.r + "," + color.g + "," + color.b + "|" + scale + "|" + pivotX + "," + pivotY;
            Sprite cached;
            if (cache.TryGetValue(key, out cached) && cached != null) return cached;

            int w = s.Length * (GW + Gap) - Gap;
            var pix = new Pix(w * scale, GH * scale);
            for (int i = 0; i < s.Length; i++)
            {
                string bits;
                if (!G.TryGetValue(s[i], out bits)) continue;
                int ox = i * (GW + Gap) * scale;
                for (int r = 0; r < GH; r++)
                    for (int cx = 0; cx < GW; cx++)
                    {
                        if (bits[r * GW + cx] != '1') continue;
                        int y = (GH - 1 - r) * scale;
                        pix.Rect(ox + cx * scale, y, scale, scale, color);
                    }
            }
            var sp = pix.ToSprite(pivotX, pivotY);
            TrimCache();
            cache[key] = sp;
            return sp;
        }
    }
}
