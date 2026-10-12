using System;
using System.Collections.Generic;
using UnityEngine;
using Kingdom2D;

public static class ArtDump
{
    class Entry { public string name; public Sprite sp; }

    static List<Entry> list;
    static void Add(string n, Sprite s) { list.Add(new Entry { name = n, sp = s }); }

    public static int Main(string[] args)
    {
        list = new List<Entry>();
        string set = args.Length > 0 ? args[0] : "all";

        if (set == "king" || set == "all")
        {
            Add("king0", Art.King(0));
            Add("king1", Art.King(1));
        }
        if (set == "all" || set == "chars")
        {
            Add("peasant", Art.Villager(VillagerRole.Peasant, 0));
            Add("archer", Art.Villager(VillagerRole.Archer, 0));
            Add("builder", Art.Villager(VillagerRole.Builder, 0));
            Add("farmer", Art.Villager(VillagerRole.Farmer, 0));
            Add("knight", Art.Villager(VillagerRole.Knight, 0));
            Add("vagrant", Art.Vagrant);
            Add("floater", Art.Greed(GreedType.Floater, 0));
            Add("walker", Art.Greed(GreedType.Walker, 0));
            Add("brute", Art.Greed(GreedType.Brute, 0));
            Add("flyer", Art.Greed(GreedType.Flyer, 0));
            Add("rabbit", Art.Rabbit);
            Add("deer", Art.Deer);
            Add("coin", Art.Coin);
            Add("arrow", Art.Arrow);
        }
        if (set == "all" || set == "build")
        {
            Add("wall1", Art.Wall(1, false));
            Add("wall2", Art.Wall(2, false));
            Add("wall3", Art.Wall(3, false));
            Add("camp1", Art.Camp(1));
            Add("camp2", Art.Camp(2));
            Add("camp3", Art.Camp(3));
            Add("farm2", Art.Farm(2));
            Add("standBow", Art.Stand(StandKind.Bow));
            Add("standHammer", Art.Stand(StandKind.Hammer));
            Add("standScythe", Art.Stand(StandKind.Scythe));
            Add("standSword", Art.Stand(StandKind.Sword));
            Add("portal", Art.Portal(0));
            Add("tent", Art.Tent);
        }

        int scale = 6, pad = 4;
        int maxH = 0, totalW = 0;
        foreach (var e in list)
        {
            maxH = Math.Max(maxH, e.sp.texture.height);
            totalW += e.sp.texture.width + pad;
        }
        int W = totalW * scale, H = (maxH + 2) * scale;
        var buf = new byte[W * H * 4];
        // dama tahtası zemin
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                bool c = ((x / 12) + (y / 12)) % 2 == 0;
                byte v = (byte)(c ? 70 : 90);
                int i = (y * W + x) * 4;
                buf[i] = v; buf[i + 1] = v; buf[i + 2] = (byte)(v + 12); buf[i + 3] = 255;
            }

        int ox = 0;
        foreach (var e in list)
        {
            var t = e.sp.texture;
            for (int y = 0; y < t.height; y++)
                for (int x = 0; x < t.width; x++)
                {
                    var c = t.Pixel(x, y);
                    if (c.a == 0) continue;
                    for (int sy = 0; sy < scale; sy++)
                        for (int sx = 0; sx < scale; sx++)
                        {
                            int px = (ox + x) * scale + sx;
                            int py = H - 1 - ((y + 1) * scale + sy);
                            if (px < 0 || py < 0 || px >= W || py >= H) continue;
                            int i = (py * W + px) * 4;
                            float a = c.a / 255f;
                            buf[i] = (byte)(buf[i] * (1 - a) + c.r * a);
                            buf[i + 1] = (byte)(buf[i + 1] * (1 - a) + c.g * a);
                            buf[i + 2] = (byte)(buf[i + 2] * (1 - a) + c.b * a);
                        }
                }
            Console.WriteLine(e.name + " : " + t.width + "x" + t.height + " @x" + ox);
            ox += t.width + pad;
        }

        System.IO.File.WriteAllBytes("shots/sheet.raw", buf);
        Console.WriteLine("SHEET " + W + "x" + H);
        return 0;
    }
}
