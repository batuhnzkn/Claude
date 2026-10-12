using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Ekran arayüzü. Unity UI paketine bağımlı olmamak için kameraya bağlı
    /// sprite'lar ve piksel fontla çizilir. Tüm ölçüler kamera boyutuna oranlıdır.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        const float RefSize = 4.9f;   // ölçeklerin tasarlandığı kamera boyutu

        static readonly VillagerRole[] RosterRoles =
        {
            VillagerRole.Peasant, VillagerRole.Archer, VillagerRole.Builder,
            VillagerRole.Farmer, VillagerRole.Knight
        };

        GameConfig cfg;
        GameManager gm;
        Camera cam;
        Transform camT;

        SpriteRenderer topBar;
        SpriteRenderer coinIcon, coinText;
        SpriteRenderer[] rosterIcons = new SpriteRenderer[5];
        SpriteRenderer[] rosterCounts = new SpriteRenderer[5];
        SpriteRenderer dayText, barBack, barFill;
        SpriteRenderer flashText, hintText, hintText2;
        SpriteRenderer leftWarn, rightWarn;

        GameObject overRoot;
        SpriteRenderer overDim, overTitle, overReason, overDay, overBest, overHint;

        float flashTimer;
        float hintTimer = 26f;

        public void Init(GameConfig config, GameManager manager, Camera camera)
        {
            cfg = config;
            gm = manager;
            cam = camera;
            camT = camera.transform;

            topBar = Util.Spawn(camT, Art.FadeStrip(32), Vector3.zero, Layer.Hud - 2, "hud_topbar");
            topBar.color = new Color(0.03f, 0.04f, 0.08f, 0.5f);

            coinIcon = Util.Spawn(camT, Art.Coin, Vector3.zero, Layer.Hud, "hud_coinIcon");
            coinText = Text("hud_coins");

            for (int i = 0; i < RosterRoles.Length; i++)
            {
                rosterIcons[i] = Util.Spawn(camT, Art.RoleIcon(RosterRoles[i]), Vector3.zero, Layer.Hud, "hud_role");
                rosterCounts[i] = Text("hud_roleCount");
            }

            dayText = Text("hud_day", 0.5f);
            flashText = Text("hud_flash", 0.5f);
            hintText = Text("hud_hint", 0.5f);
            hintText2 = Text("hud_hint2", 0.5f);
            leftWarn = Text("hud_warnL", 0.5f);
            rightWarn = Text("hud_warnR", 0.5f);
            Set(leftWarn, "<", Color.white, 0.5f);
            Set(rightWarn, ">", Color.white, 0.5f);

            barBack = Util.Spawn(camT, Art.Solid, Vector3.zero, Layer.Hud, "hud_barBack");
            barFill = Util.Spawn(camT, Art.Solid, Vector3.zero, Layer.Hud + 1, "hud_barFill");
            barBack.color = new Color(0f, 0f, 0f, 0.5f);
            barFill.color = Pal.Gold;

            BuildGameOver();
        }

        SpriteRenderer Text(string name, float pivotX = 0f)
        {
            return Util.Spawn(camT, PixelFont.Make(" ", Pal.White, 1, pivotX, 0.5f), Vector3.zero, Layer.HudText, name);
        }

        void Set(SpriteRenderer sr, string text, Color color, float pivotX = 0f)
        {
            if (sr != null) sr.sprite = PixelFont.Make(text, color, 1, pivotX, 0.5f);
        }

        static void Place(SpriteRenderer sr, float x, float y, float scale)
        {
            sr.transform.localPosition = new Vector3(x, y, 10f);
            sr.transform.localScale = Vector3.one * scale;
        }

        static void Stretch(SpriteRenderer sr, float x, float y, float w, float h)
        {
            Vector2 s = sr.sprite.bounds.size;
            sr.transform.localPosition = new Vector3(x, y, 9.5f);
            sr.transform.localScale = new Vector3(w / s.x, h / s.y, 1f);
        }

        void BuildGameOver()
        {
            overRoot = Util.Node(camT, "hud_gameover", Vector3.zero);
            overDim = Util.Spawn(overRoot.transform, Art.Solid, new Vector3(0f, 0f, 8f), Layer.Hud - 1, "dim");
            overDim.color = new Color(0.02f, 0.01f, 0.04f, 0.84f);
            overTitle = GoText();
            overReason = GoText();
            overDay = GoText();
            overBest = GoText();
            overHint = GoText();
            overRoot.SetActive(false);
        }

        SpriteRenderer GoText()
        {
            return Util.Spawn(overRoot.transform, PixelFont.Make(" ", Pal.White, 1, 0.5f, 0.5f), Vector3.zero,
                Layer.HudText, "go_text");
        }

        // ----------------------------------------------------------- mesajlar

        public void Flash(string message, Color color)
        {
            Set(flashText, message, color, 0.5f);
            flashTimer = 2.8f;
        }

        public void ShowGameOver(int day, string reason, int best)
        {
            overRoot.SetActive(true);
            Set(overTitle, "OYUN BITTI", Pal.Cape, 0.5f);
            Set(overReason, reason, Pal.White, 0.5f);
            Set(overDay, day + ". GUNE KADAR DAYANDIN", Pal.Gold, 0.5f);
            Set(overBest, "EN IYI: " + Mathf.Max(best, day) + ". GUN", Pal.White, 0.5f);
            Set(overHint, "R - YENIDEN BASLA", Pal.White, 0.5f);
        }

        // -------------------------------------------------------------- çizim

        void LateUpdate()
        {
            if (cfg == null || gm == null || cam == null) return;

            float hh = cam.orthographicSize;
            float hw = hh * cam.aspect;
            float ui = hh / RefSize;                 // her şey kamera boyutuna oranlı
            float top = hh - 0.55f * ui;
            float left = -hw + 0.55f * ui;

            Stretch(topBar, 0f, hh - 1.1f * ui, hw * 2f, 2.2f * ui);

            // --- altın
            var king = gm.King;
            int coins = king != null ? king.Coins : 0;
            int cap = king != null ? king.Capacity : 0;
            Place(coinIcon, left + 0.17f * ui, top, 1.5f * ui);
            Place(coinText, left + 0.42f * ui, top, 1.8f * ui);
            Set(coinText, coins + "/" + cap, coins >= cap ? Pal.Flame : Pal.Gold);

            // --- nüfus (ikon + sayı)
            float rx = left + 0.15f * ui;
            float ry = top - 0.72f * ui;
            for (int i = 0; i < RosterRoles.Length; i++)
            {
                int n = gm.CountRole(RosterRoles[i]);
                Place(rosterIcons[i], rx, ry, 0.85f * ui);
                rosterIcons[i].color = n > 0 ? Color.white : new Color(1f, 1f, 1f, 0.3f);
                Place(rosterCounts[i], rx + 0.3f * ui, ry - 0.02f * ui, 1.25f * ui);
                Set(rosterCounts[i], n.ToString(), n > 0 ? Pal.White : new Color(1f, 1f, 1f, 0.45f));
                rx += (0.92f + (n >= 10 ? 0.24f : 0f)) * ui;
            }

            // --- gün + gece sayacı
            bool night = gm.IsNight;
            bool blood = gm.Clock != null && gm.Clock.BloodMoon;
            Place(dayText, 0f, top, 1.9f * ui);
            Set(dayText, gm.Day + ". GUN", blood ? Pal.Cape : Pal.White, 0.5f);

            float p = night ? 1f - gm.Clock.NightProgress : gm.Clock.DayProgress;
            p = Mathf.Clamp01(p);
            bool darkish = gm.Darkness > 0.5f;
            float barW = 4.4f * ui;
            Stretch(barBack, 0f, top - 0.6f * ui, barW, 0.17f * ui);
            Stretch(barFill, -barW * 0.5f + barW * 0.5f * p, top - 0.6f * ui, Mathf.Max(0.001f, barW * p), 0.11f * ui);
            barFill.color = darkish ? (blood ? Pal.Cape : new Color(0.6f, 0.7f, 1f)) : Pal.Gold;

            // --- uçuşan mesaj
            flashTimer -= Time.deltaTime;
            Place(flashText, 0f, hh * 0.45f, 2.3f * ui);
            var fc = flashText.color;
            flashText.color = new Color(fc.r, fc.g, fc.b, Mathf.Clamp01(flashTimer / 0.8f));
            flashText.enabled = flashTimer > 0f;

            // --- başlangıç ipuçları
            hintTimer -= Time.deltaTime;
            float ha = Mathf.Clamp01(hintTimer / 3f);
            Place(hintText, 0f, -hh + 0.95f * ui, 0.95f * ui);
            Set(hintText, "A/D YURU   SPACE ALTIN AT   R YENIDEN", Pal.White, 0.5f);
            hintText.color = new Color(1f, 1f, 1f, ha);
            hintText.enabled = hintTimer > 0f;

            Place(hintText2, 0f, -hh + 0.58f * ui, 0.85f * ui);
            Set(hintText2, "CADIRDAN KOYLU TOPLA - TEZGAHTAN ALET VER - GECEYE OKCU HAZIRLA", Pal.Gold, 0.5f);
            hintText2.color = new Color(1f, 1f, 1f, ha);
            hintText2.enabled = hintTimer > 0f;

            UpdateWarnings(hw, hh, ui);
            UpdateGameOver(hw, hh, ui);
        }

        void UpdateWarnings(float hw, float hh, float ui)
        {
            bool l = false, r = false;
            float camX = camT.position.x;
            for (int i = 0; i < gm.Greeds.Count; i++)
            {
                var g = gm.Greeds[i];
                if (g == null || !g.Alive || g.Fleeing) continue;
                if (g.X < camX - hw) l = true;
                else if (g.X > camX + hw) r = true;
            }
            float pulse = 0.35f + Util.Pulse(Time.time, 7f) * 0.65f;
            var warnColor = new Color(1f, 1f, 1f, pulse);
            leftWarn.enabled = l;
            rightWarn.enabled = r;
            Place(leftWarn, -hw + 0.4f * ui, 0.4f * ui, 2.4f * ui);
            Place(rightWarn, hw - 0.4f * ui, 0.4f * ui, 2.4f * ui);
            leftWarn.color = warnColor;
            rightWarn.color = warnColor;
        }

        void UpdateGameOver(float hw, float hh, float ui)
        {
            if (overRoot == null || !overRoot.activeSelf) return;
            Vector2 s = overDim.sprite.bounds.size;
            overDim.transform.localPosition = new Vector3(0f, 0f, 8f);
            overDim.transform.localScale = new Vector3(hw * 2.4f / s.x, hh * 2.4f / s.y, 1f);
            Place(overTitle, 0f, 1.9f * ui, 3.8f * ui);
            Place(overReason, 0f, 0.95f * ui, 1.7f * ui);
            Place(overDay, 0f, 0.1f * ui, 1.9f * ui);
            Place(overBest, 0f, -0.7f * ui, 1.5f * ui);
            Place(overHint, 0f, -1.9f * ui, 1.8f * ui);
            overHint.color = new Color(1f, 1f, 1f, 0.35f + Util.Pulse(Time.time, 4f) * 0.65f);
        }
    }
}
