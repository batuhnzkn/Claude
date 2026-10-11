using System.Collections.Generic;
using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Oyunun sprite kütüphanesi. Tüm görseller ilk istendiğinde üretilip
    /// önbelleğe alınır. Karakterler piksel haritalarıyla, yapılar ve
    /// manzara ArtWorld.cs içinde geometrik olarak çizilir.
    /// </summary>
    public static partial class Art
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        static Sprite Get(string key, System.Func<Sprite> build)
        {
            Sprite s;
            if (cache.TryGetValue(key, out s) && s != null) return s;
            s = build();
            cache[key] = s;
            return s;
        }

        // ------------------------------------------------------------------ Kral

        static readonly string[] KingBase =
        {
            "........y.y.........",
            "........yyy.........",
            "........###.........",
            "......rr###....###..",
            ".....rrr###...#####.",
            ".....rrr###...#####.",
            ".##.rrr#####.#####..",
            "..############......",
            ".#############......",
            "..############......",
            "..###########.......",
        };

        static readonly string[] KingLegsA =
        {
            "...##.....##........",
            "...##.....##........",
            "...##.....##........",
            "..####....###.......",
        };

        static readonly string[] KingLegsB =
        {
            "...##.....##........",
            "..##.......##.......",
            ".##.........##......",
            "###.........###.....",
        };

        public static Sprite King(int frame)
        {
            return Get("king" + frame, () =>
            {
                var p = new Pix(20, 15);
                p.Rows(KingBase, 0, 4);
                p.Rows(frame == 0 ? KingLegsA : KingLegsB, 0, 0);
                return p.ToSprite(0.5f, 0f);
            });
        }

        // -------------------------------------------------------------- Köylüler

        static readonly string[] VillagerTorso =
        {
            "..##....",
            ".####...",
            ".####...",
            "..##....",
            ".o###...",
            ".####...",
            ".####...",
            "..##....",
        };

        static readonly string[] LegsA = { ".#..#...", ".#..#...", "##..##.." };
        static readonly string[] LegsB = { "..##....", "..##....", ".####..." };

        static readonly string[] ToolBow =
        {
            "........",
            "........",
            ".....#..",
            "......#.",
            "......#.",
            "......#.",
            "......#.",
            ".....#..",
            "........",
            "........",
            "........",
        };

        static readonly string[] ToolHammer =
        {
            "........",
            ".....MMM",
            "......#.",
            "......#.",
            "......#.",
            "......#.",
            "........",
            "........",
            "........",
            "........",
            "........",
        };

        static readonly string[] ToolScythe =
        {
            "....mmm.",
            "...m...m",
            "......#.",
            "......#.",
            "......#.",
            "......#.",
            "......#.",
            "........",
            "........",
            "........",
            "........",
        };

        static readonly string[] ToolSword =
        {
            "........",
            "......m.",
            "......m.",
            "......m.",
            "M.....m.",
            "M...mmmm",
            "M.....#.",
            "M.......",
            "........",
            "........",
            "........",
        };

        public static Sprite Villager(VillagerRole role, int frame)
        {
            return Get("vil" + (int)role + frame, () =>
            {
                var p = new Pix(8, 11);
                p.Rows(VillagerTorso, 0, 3);
                p.Rows(frame == 0 ? LegsA : LegsB, 0, 0);
                string[] tool = null;
                switch (role)
                {
                    case VillagerRole.Archer: tool = ToolBow; break;
                    case VillagerRole.Builder: tool = ToolHammer; break;
                    case VillagerRole.Farmer: tool = ToolScythe; break;
                    case VillagerRole.Knight: tool = ToolSword; break;
                }
                if (tool != null) p.Rows(tool, 0, 0);
                return p.ToSprite(0.5f, 0f);
            });
        }

        static readonly string[] VagrantMap =
        {
            "..oo....",
            ".oooo...",
            ".oooo...",
            "..oo....",
            ".ooo....",
            ".oooo...",
            ".ooo....",
            "..oo....",
            ".o..o...",
            ".o..o...",
            "oo..oo..",
        };

        public static Sprite Vagrant { get { return Get("vagrant", () => { var p = new Pix(8, 11); p.Rows(VagrantMap); return p.ToSprite(0.5f, 0f); }); } }

        // ------------------------------------------------------------------ Greed

        static readonly string[] GreedFloater =
        {
            "..ppp..",
            ".ppppp.",
            "ppeppep",
            "ppppppp",
            ".ppppp.",
            "..ppp..",
            "..p.p..",
            ".p...p.",
        };

        static readonly string[] GreedWalkerBody =
        {
            "...ppp...",
            "..peppe..",
            "..ppppp..",
            "...ppp...",
            ".pppppp..",
            "p.ppppp.p",
            "p.ppppp.p",
            "p..ppp..p",
        };

        static readonly string[] GreedWalkerLegsA = { "...p.p...", "...p.p...", "..pp.pp.." };
        static readonly string[] GreedWalkerLegsB = { "...ppp...", "..pp.pp..", ".pp...pp." };

        static readonly string[] GreedBruteBody =
        {
            "..p........p..",
            "..pp......pp..",
            "...pppppppp...",
            "..peppppppep..",
            "...pppppppp...",
            "..pppppppppp..",
            ".pppppppppppp.",
            "pppppppppppppp",
            "pppppppppppppp",
            ".pppppppppppp.",
            ".pppppppppppp.",
            "..pppppppppp..",
        };

        static readonly string[] GreedBruteLegsA = { "..ppp....ppp..", "..ppp....ppp..", "..ppp....ppp..", ".pppp....pppp." };
        static readonly string[] GreedBruteLegsB = { "..ppp....ppp..", ".ppp......ppp.", ".ppp......ppp.", "pppp......pppp" };

        static readonly string[] GreedFlyerA =
        {
            "p..........p",
            "pp...pp...pp",
            "ppp.pppp.ppp",
            ".pppeppeppp.",
            "..pppppppp..",
            "...pppppp...",
            "....p..p....",
            "...p....p...",
        };

        static readonly string[] GreedFlyerB =
        {
            "............",
            ".....pp.....",
            "ppp.pppp.ppp",
            "ppppeppepppp",
            "..pppppppp..",
            "...pppppp...",
            "....p..p....",
            "....p..p....",
        };

        public static Sprite Greed(GreedType type, int frame)
        {
            return Get("greed" + (int)type + frame, () =>
            {
                Pix p;
                switch (type)
                {
                    case GreedType.Floater:
                        p = new Pix(7, 8);
                        p.Rows(GreedFloater);
                        break;
                    case GreedType.Brute:
                        p = new Pix(14, 16);
                        p.Rows(GreedBruteBody, 0, 4);
                        p.Rows(frame == 0 ? GreedBruteLegsA : GreedBruteLegsB, 0, 0);
                        break;
                    case GreedType.Flyer:
                        p = new Pix(12, 8);
                        p.Rows(frame == 0 ? GreedFlyerA : GreedFlyerB);
                        break;
                    default:
                        p = new Pix(9, 11);
                        p.Rows(GreedWalkerBody, 0, 3);
                        p.Rows(frame == 0 ? GreedWalkerLegsA : GreedWalkerLegsB, 0, 0);
                        break;
                }
                return p.ToSprite(0.5f, 0f);
            });
        }

        // ------------------------------------------------------------- Eşya / ufak

        static readonly string[] CoinMap =
        {
            ".yyy.",
            "ywyyY",
            "yyyYY",
            "yyYYY",
            ".YYY.",
        };

        public static Sprite Coin { get { return Get("coin", () => { var p = new Pix(5, 5); p.Rows(CoinMap); return p.ToSprite(0.5f, 0.5f); }); } }

        static readonly string[] ArrowMap = { "o....m.", "ooooomm", "o....m." };

        public static Sprite Arrow { get { return Get("arrow", () => { var p = new Pix(7, 3); p.Rows(ArrowMap); return p.ToSprite(0.9f, 0.5f); }); } }

        static readonly string[] CrownMap =
        {
            "y.y.y.y",
            "y.y.y.y",
            "yyyyyyy",
            "yYYYYYy",
            "yyyyyyy",
        };

        public static Sprite Crown { get { return Get("crown", () => { var p = new Pix(7, 5); p.Rows(CrownMap); return p.ToSprite(0.5f, 0.5f); }); } }

        static readonly string[] RabbitMap =
        {
            "...#.#..",
            "...#.#..",
            "..#####.",
            ".######.",
            "########",
            ".#.##.#.",
            ".#.##.#.",
        };

        public static Sprite Rabbit { get { return Get("rabbit", () => { var p = new Pix(8, 7); p.Rows(RabbitMap); return p.ToSprite(0.5f, 0f); }); } }

        static readonly string[] DeerMap =
        {
            "........#...#.",
            ".........#.#..",
            ".........###..",
            ".........##...",
            "........###...",
            "##########....",
            "###########...",
            "##########....",
            ".#......#.....",
            ".#......#.....",
            ".#......#.....",
            "##......##....",
        };

        public static Sprite Deer { get { return Get("deer", () => { var p = new Pix(14, 12); p.Rows(DeerMap); return p.ToSprite(0.5f, 0f); }); } }

        /// <summary>Tek renk 4x4 kare: ölçeklenerek dikdörtgen (zemin, perde, çubuk) olarak kullanılır.</summary>
        public static Sprite Solid
        {
            get
            {
                return Get("solid", () =>
                {
                    var p = new Pix(4, 4);
                    p.Fill(Color.white);
                    return p.ToSprite(0.5f, 0.5f, 4f); // 1 birim = 1 sprite
                });
            }
        }

        public static Sprite Dot
        {
            get
            {
                return Get("dot", () =>
                {
                    var p = new Pix(2, 2);
                    p.Fill(Color.white);
                    return p.ToSprite(0.5f, 0.5f);
                });
            }
        }
    }
}
