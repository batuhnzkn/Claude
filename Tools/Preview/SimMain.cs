using System;
using System.Collections.Generic;
using UnityEngine;
using Kingdom2D;

public static class SimMain
{
    static int restarts;
    static int mode;
    static int gameOvers;

    public static int Main(string[] args)
    {
        int frames = args.Length > 0 ? int.Parse(args[0]) : 12000;
        mode = args.Length > 1 ? int.Parse(args[1]) : 0;
        float dt = 1f / 60f;

        var boot = new GameObject("Kingdom2D");
        var bootComp = boot.AddComponent<GameBootstrap>();
        if (mode == 1) bootComp.Config.startCoins = 0;   // pasif oyuncu: tac calinmali

        for (int i = 0; i < frames; i++)
        {
            try
            {
                DriveBot();
                if (mode == 2) ForceAttack(i);
                Runtime.Step(dt);
                TrackStats();
                if (mode == 3) ShotSchedule(i);
            }
            catch (Exception e)
            {
                Console.WriteLine("### KARE " + i + " HATA: " + e.GetType().Name + ": " + e.Message);
                Console.WriteLine(e.StackTrace);
                Report(i);
                return 1;
            }
            if (i % 1800 == 0) Report(i);
        }

        Report(frames);
        Console.WriteLine("### uretilen sprite: " + Sprite.Created + " | uretilen doku: " + Texture2D.Created +
                          "  (sabit kalmasi gerekir - onbellek sizintisi kontrolu)");
        Console.WriteLine("### en yakin greed mesafesi: " + minGreedX.ToString("0.0") +
                          " | hasarli duvar kareleri: " + wallsDamaged + " | kral soyulma: " + kingRobbed);
        Console.WriteLine("### SIMULASYON TAMAM - hata yok. Yeniden baslatma: " + restarts +
                          " | ses calma: " + AudioSource.PlayCount);
        foreach (var l in Debug.Logs) Console.WriteLine(l);
        return 0;
    }

    /// <summary>Basit yapay oyuncu: altın toplar, en yakın ödeme noktasını doldurur.</summary>
    static void DriveBot()
    {
        var gm = GameManager.I;
        Input.FakeRestart = false;
        if (gm == null || gm.King == null) { Input.FakeHorizontal = 0f; Input.FakePay = false; return; }

        if (gm.State == GameState.GameOver)
        {
            gameOvers++;
            if (gameOvers == 1) Console.WriteLine(">>> OYUN SONU: " + gm.GameOverReason + " | gun " + gm.Day + " | en iyi " + gm.BestDay);
            Input.FakeHorizontal = 0f;
            Input.FakePay = false;
            Input.FakeRestart = true;
            restarts++;
            return;
        }
        gameOvers = 0;

        if (mode == 1)
        {
            // pasif kral: üste oturur, kaçmaz -> greed tacı almalı
            Input.FakeHorizontal = Math.Abs(kx0(gm)) < 0.3f ? 0f : (kx0(gm) > 0 ? -1f : 1f);
            Input.FakePay = false;
            return;
        }

        float kx = gm.King.X;
        float targetX = kx;
        bool pay = false;

        bool night = gm.IsNight || gm.Darkness > 0.5f;

        if (gm.King.Coins > 0)
        {
            CoinSink best = null;
            float bestScore = -1f;
            for (int i = 0; i < gm.Sinks.Count; i++)
            {
                var s = gm.Sinks[i];
                if (s == null || !s.Active || s.Full) continue;
                float d = Math.Abs(s.X - kx);
                if (night && Math.Abs(s.X) > 22f) continue;     // gece uzaga gitme
                float prio = Priority(s, gm);
                if (prio <= 0f) continue;
                float score = prio / (1f + d * 0.06f);
                if (score > bestScore) { bestScore = score; best = s; }
            }
            if (best != null)
            {
                targetX = best.X;
                pay = Math.Abs(best.X - kx) <= gm.Config.payRange * 0.8f;
            }
            else if (night) targetX = 0f;
        }
        else
        {
            CoinPickup best = null;
            float bd = 1e9f;
            for (int i = 0; i < gm.Coins.Count; i++)
            {
                var c = gm.Coins[i];
                if (c == null || c.Claimed) continue;
                if (night && Math.Abs(c.X) > 25f) continue;
                float d = Math.Abs(c.X - kx);
                if (d < bd) { bd = d; best = c; }
            }
            if (best != null) targetX = best.X;
            else targetX = night ? 0f : Mathf.Sin(Time.time * 0.15f) * 28f;
        }

        float delta = targetX - kx;
        Input.FakeHorizontal = Math.Abs(delta) < 0.15f ? 0f : (delta > 0 ? 1f : -1f);
        Input.FakePay = pay;
    }

    static float kx0(GameManager gm) { return gm.King.X; }

    static float minGreedX = 999f;
    static int wallsDamaged, kingRobbed;
    static int lastCoins = -1;

    static void TrackStats()
    {
        var gm = GameManager.I;
        if (gm == null || gm.King == null) return;
        for (int i = 0; i < gm.Greeds.Count; i++)
        {
            var g = gm.Greeds[i];
            if (g == null || g.Fleeing) continue;
            float d = Math.Abs(g.X);
            if (d < minGreedX) minGreedX = d;
        }
        for (int i = 0; i < gm.Walls.Count; i++)
        {
            var w = gm.Walls[i];
            if (w != null && w.Standing && w.Hp < w.MaxHp - 0.5f) { wallsDamaged++; break; }
        }
        if (lastCoins > 0 && gm.King.Coins < lastCoins - 2) kingRobbed++;
        lastCoins = gm.King.Coins;
    }

    static void ForceAttack(int frame)
    {
        var gm = GameManager.I;
        if (gm == null || gm.King == null || gm.State != GameState.Playing) return;
        if (frame % 120 != 0 || frame < 240) return;
        Greed.Spawn(gm.ActorRoot, gm.King.X + 4f, 1, GreedType.Walker, 1f);
    }

    static float Priority(CoinSink s, GameManager gm)
    {
        var go = s.gameObject;
        if (go.GetComponent<VagrantCamp>() != null)
            return gm.CountRole(VillagerRole.Peasant) < 3 ? 10f : 2.5f;
        var stand = go.GetComponent<ToolStand>();
        if (stand != null)
        {
            switch (stand.Kind)
            {
                case StandKind.Bow: return gm.CountRole(VillagerRole.Archer) < 6 ? 9f : 1f;
                case StandKind.Hammer: return gm.CountRole(VillagerRole.Builder) < 2 ? 8.5f : 0.5f;
                case StandKind.Scythe: return gm.CountRole(VillagerRole.Farmer) < 4 ? 7f : 0.5f;
                default: return gm.CountRole(VillagerRole.Knight) < 3 ? 6.5f : 0.3f;
            }
        }
        if (go.GetComponent<Farm>() != null) return 6f;
        var wall = go.GetComponent<Wall>();
        if (wall != null) return Math.Abs(wall.X) < 45f ? 5f : 1.5f;
        if (go.GetComponent<BaseCamp>() != null) return 1.2f;
        return 1f;
    }

    const int SW = 960, SH = 540;

    static void ShotSchedule(int frame)
    {
        switch (frame)
        {
            case 900: Capture("01_sabah", "king"); break;
            case 2400: Capture("02_gunduz_us", "base"); break;
            case 3300: Capture("03_aksam", "king"); break;
            case 4800: Capture("04_gece1", "action"); break;
            case 20400: Capture("05_gunduz_duvar", "wall"); break;
            case 22400: Capture("06_gece_savunma", "action"); break;
            case 22600: Capture("14_duvar_gece", "wall"); break;
            case 29900: Capture("07_kanli_ay", "action"); break;
            case 30400: Capture("08_kanli_ay_duvar", "action"); break;
            case 30500: Capture("11_yakin_greed", "greed", 2.6f); break;
            case 1500: Capture("12_yakin_kral", "king", 2.3f); break;
            case 1800: Capture("13_yakin_koylu", "villager", 2.3f); break;
            case 40200: Capture("09_krallik", "base"); break;
            case 40800: Capture("10_tarla", "farm"); break;
        }
    }

    static void Capture(string name, string focus) { Capture(name, focus, 0f); }

    static void Capture(string name, string focus, float zoom)
    {
        var gm = GameManager.I;
        if (gm == null || gm.Rig == null || gm.Rig.Cam == null) return;
        var cam = gm.Rig.Cam;
        var t = cam.transform;
        var saved = t.position;
        float savedSize = cam.orthographicSize;
        float camY = saved.y;
        if (zoom > 0f) { cam.orthographicSize = zoom; camY = zoom * 0.52f; }

        float fx = saved.x;
        if (focus == "base") fx = gm.BaseX;
        else if (focus == "greed")
        {
            for (int i = 0; i < gm.Greeds.Count; i++)
                if (gm.Greeds[i] != null && gm.Greeds[i].Alive) { fx = gm.Greeds[i].X; break; }
        }
        else if (focus == "king") fx = gm.King.X;
        else if (focus == "villager")
        {
            for (int i = 0; i < gm.Villagers.Count; i++)
                if (gm.Villagers[i] != null) { fx = gm.Villagers[i].X; break; }
        }
        else if (focus == "action")
        {
            // en çok greed'in toplandığı yer
            float best = fx;
            int bestN = -1;
            for (int i = 0; i < gm.Greeds.Count; i++)
            {
                var g = gm.Greeds[i];
                if (g == null || !g.Alive) continue;
                int n = 0;
                for (int j = 0; j < gm.Greeds.Count; j++)
                {
                    var o = gm.Greeds[j];
                    if (o != null && o.Alive && Math.Abs(o.X - g.X) < 11f) n++;
                }
                if (n > bestN) { bestN = n; best = g.X; }
            }
            fx = best;
        }
        else if (focus == "wall")
        {
            Wall bw = null;
            for (int i = 0; i < gm.Walls.Count; i++)
            {
                var w = gm.Walls[i];
                if (w == null || !w.Standing) continue;
                if (bw == null || w.Tier > bw.Tier) bw = w;
            }
            if (bw != null) fx = bw.X;
        }
        else if (focus == "farm")
        {
            for (int i = 0; i < gm.Farms.Count; i++)
                if (gm.Farms[i] != null && gm.Farms[i].Active) { fx = gm.Farms[i].X; break; }
        }

        float limit = Math.Max(0f, gm.Config.worldHalfWidth - cam.orthographicSize * cam.aspect);
        fx = Mathf.Clamp(fx, -limit, limit);
        t.position = new Vector3(fx, camY, saved.z);
        Console.WriteLine("    [debug] " + name + " focus=" + focus + " camX=" + fx.ToString("0.0") +
                          " kingX=" + gm.King.X.ToString("0.0") + " kingY=" + gm.King.transform.position.y.ToString("0.00") +
                          " camY=" + camY.ToString("0.0") + " size=" + cam.orthographicSize.ToString("0.0"));
        Shot.Capture(cam, SW, SH, "shots/" + name + ".raw");
        t.position = saved;
        cam.orthographicSize = savedSize;
    }

    static void Report(int frame)
    {
        var gm = GameManager.I;
        if (gm == null) { Console.WriteLine("kare " + frame + ": oyun yok"); return; }
        int walls = 0, wallTiers = 0;
        for (int i = 0; i < gm.Walls.Count; i++)
            if (gm.Walls[i] != null && gm.Walls[i].Standing) { walls++; wallTiers += gm.Walls[i].Tier; }

        Console.WriteLine(string.Format(
            "kare {0,6} | gun {1,2} {2,-5} | altin {3,2}/{4,2} | koylu {5} okcu {6} insa {7} cift {8} sov {9} | greed {10,2} | duvar {11} (kademe {12}) | us {13} | tarla {14} | nesne {15}",
            frame, gm.Day, gm.Clock.Phase, gm.King != null ? gm.King.Coins : -1, gm.King != null ? gm.King.Capacity : -1,
            gm.CountRole(VillagerRole.Peasant), gm.CountRole(VillagerRole.Archer), gm.CountRole(VillagerRole.Builder),
            gm.CountRole(VillagerRole.Farmer), gm.CountRole(VillagerRole.Knight),
            gm.Greeds.Count, walls, wallTiers, gm.Camp != null ? gm.Camp.Tier : 0,
            gm.Farms.Count, Runtime.LiveObjects));
    }
}
