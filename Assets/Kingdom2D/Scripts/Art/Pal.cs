using System.Collections.Generic;
using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Oyunun tüm renk paleti. Tek yerden değiştirilebilir.</summary>
    public static class Pal
    {
        public static Color Hex(string hex)
        {
            int i = hex.Length == 7 ? 1 : 0;
            float r = System.Convert.ToInt32(hex.Substring(i + 0, 2), 16) / 255f;
            float g = System.Convert.ToInt32(hex.Substring(i + 2, 2), 16) / 255f;
            float b = System.Convert.ToInt32(hex.Substring(i + 4, 2), 16) / 255f;
            return new Color(r, g, b, 1f);
        }

        public static readonly Color Clear = new Color(0, 0, 0, 0);

        // Siluetler
        public static readonly Color Dark = Hex("#14121C");
        public static readonly Color Dark2 = Hex("#241F2E");
        public static readonly Color Dark3 = Hex("#0A0910");

        // Kral
        public static readonly Color Cape = Hex("#9E2B3A");
        public static readonly Color CapeDark = Hex("#6E1C28");
        public static readonly Color Gold = Hex("#F0C64B");
        public static readonly Color GoldDark = Hex("#B8882B");

        // Genel
        public static readonly Color White = Hex("#EDE7DA");
        public static readonly Color Metal = Hex("#9AA3AD");
        public static readonly Color MetalDark = Hex("#5E6670");

        // Greed (düşman)
        public static readonly Color GreedSkin = Hex("#2A1F33");
        public static readonly Color GreedSkin2 = Hex("#3D2C4A");
        public static readonly Color Eye = Hex("#FF8A3D");
        public static readonly Color EyeHot = Hex("#FFD089");

        // Doğa
        public static readonly Color Leaf = Hex("#2F4A33");
        public static readonly Color LeafDark = Hex("#1E3324");
        public static readonly Color Wood = Hex("#4A3426");
        public static readonly Color WoodDark = Hex("#2E2018");
        public static readonly Color Stone = Hex("#5A5A66");
        public static readonly Color StoneDark = Hex("#3C3C46");
        public static readonly Color Soil = Hex("#3A2E26");
        public static readonly Color Crop = Hex("#C9A74E");
        public static readonly Color Fur = Hex("#7A6652");

        // Ateş
        public static readonly Color Flame = Hex("#F08A2E");
        public static readonly Color FlameHot = Hex("#FFD36B");

        // Zemin
        public static readonly Color GroundTop = Hex("#2C4433");
        public static readonly Color GroundBody = Hex("#1A2820");
        public static readonly Color GroundDeep = Hex("#101A14");

        // Gökyüzü (üst / alt)
        public static readonly Color SkyDayTop = Hex("#5AA8DE");
        public static readonly Color SkyDayBot = Hex("#C8E6F0");
        public static readonly Color SkyDuskTop = Hex("#36406E");
        public static readonly Color SkyDuskBot = Hex("#E9875A");
        public static readonly Color SkyNightTop = Hex("#070A1A");
        public static readonly Color SkyNightBot = Hex("#1C2748");
        public static readonly Color SkyBloodTop = Hex("#1A0810");
        public static readonly Color SkyBloodBot = Hex("#5E1522");

        // Tepeler (uzaktan yakına)
        public static readonly Color HillFar = Hex("#6E86A8");
        public static readonly Color HillMid = Hex("#4A5C7A");
        public static readonly Color HillNear = Hex("#2E3A52");
        public static readonly Color ForestBack = Hex("#1D2636");

        /// <summary>Piksel haritalarında kullanılan karakter -> renk eşlemesi.</summary>
        public static readonly Dictionary<char, Color> Map = new Dictionary<char, Color>
        {
            { '.', Clear },
            { '#', Dark },
            { 'o', Dark2 },
            { 'x', Dark3 },
            { 'r', Cape },
            { 'R', CapeDark },
            { 'y', Gold },
            { 'Y', GoldDark },
            { 'w', White },
            { 'm', Metal },
            { 'M', MetalDark },
            { 'p', GreedSkin },
            { 'P', GreedSkin2 },
            { 'e', Eye },
            { 'E', EyeHot },
            { 'g', Leaf },
            { 'G', LeafDark },
            { 'b', Wood },
            { 'B', WoodDark },
            { 's', Stone },
            { 'S', StoneDark },
            { 't', Soil },
            { 'c', Crop },
            { 'f', Flame },
            { 'F', FlameHot },
            { 'u', Fur },
        };

        public static Color Of(char c)
        {
            Color col;
            return Map.TryGetValue(c, out col) ? col : Clear;
        }
    }
}
