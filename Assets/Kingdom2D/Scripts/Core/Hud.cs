using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Ekran arayüzü. Unity UI paketine bağımlı olmamak için kameraya bağlı
    /// sprite'lar ve piksel fontla çizilir.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        GameConfig cfg;
        GameManager gm;
        Camera cam;
        Transform camT;

        SpriteRenderer coinIcon, coinText, rosterText;
        SpriteRenderer dayText, barBack, barFill;
        SpriteRenderer flashText;
        SpriteRenderer hintText;
        SpriteRenderer hintText2;
        SpriteRenderer leftWarn, rightWarn;

        GameObject overRoot;
        SpriteRenderer overDim, overTitle, overReason, overDay, overBest, overHint;

        float flashTimer;
        float hintTimer = 24f;

        public void Init(GameConfig config, GameManager manager, Camera camera)
        {
            cfg = config;
            gm = manager;
            cam = camera;
            camT = camera.transform;

            coinIcon = Util.Spawn(camT, Art.Coin, Vector3.zero, Layer.Hud, "hud_coinIcon");
            coinIcon.transform.localScale = Vector3.one * 2.2f;
            coinText = Text("hud_coins", 2.4f);
            rosterText = Text("hud_roster", 1.5f);
            dayText = Text("hud_day", 2.6f, 0.5f);
            flashText = Text("hud_flash", 3f, 0.5f);
            hintText = Text("hud_hint", 1.5f, 0.5f);
            hintText2 = Text("hud_hint2", 1.4f, 0.5f);
            leftWarn = Text("hud_warnL", 3.4f, 0.5f);
            rightWarn = Text("hud_warnR", 3.4f, 0.5f);
            Set(leftWarn, "<", Color.white, 0.5f);
            Set(rightWarn, ">", Color.white, 0.5f);

            barBack = Util.Spawn(camT, Art.Solid, Vector3.zero, Layer.Hud, "hud_barBack");
            barFill = Util.Spawn(camT, Art.Solid, Vector3.zero, Layer.Hud + 1, "hud_barFill");
            barBack.color = new Color(0f, 0f, 0f, 0.45f);
            barFill.color = Pal.Gold;

            BuildGameOver();
        }

        SpriteRenderer Text(string name, float scale, float pivotX = 0f)
        {
            var sr = Util.Spawn(camT, PixelFont.Make(" ", Pal.White, 1, pivotX, 0.5f), Vector3.zero,
                Layer.HudText, name);
            sr.transform.localScale = Vector3.one * scale;
            return sr;
        }

        void Set(SpriteRenderer sr, string text, Color color, float pivotX = 0f)
        {
            if (sr == null) return;
            sr.sprite = PixelFont.Make(text, color, 1, pivotX, 0.5f);
        }

        static void Scale(SpriteRenderer sr, float w, float h)
        {
            Vector2 s = sr.sprite.bounds.size;
            sr.transform.localScale = new Vector3(w / s.x, h / s.y, 1f);
        }

        void BuildGameOver()
        {
            overRoot = Util.Node(camT, "hud_gameover", Vector3.zero);
            overDim = Util.Spawn(overRoot.transform, Art.Solid, new Vector3(0f, 0f, 8f), Layer.Hud - 1, "dim");
            overDim.color = new Color(0.02f, 0.01f, 0.04f, 0.82f);

            overTitle = GoText(5.5f);
            overReason = GoText(2.2f);
            overDay = GoText(2.6f);
            overBest = GoText(1.9f);
            overHint = GoText(2.2f);
            overRoot.SetActive(false);
        }

        SpriteRenderer GoText(float scale)
        {
            var sr = Util.Spawn(overRoot.transform, PixelFont.Make(" ", Pal.White, 1, 0.5f, 0.5f), Vector3.zero,
                Layer.HudText, "go_text");
            sr.transform.localScale = Vector3.one * scale;
            return sr;
        }

        // ----------------------------------------------------------- mesajlar

        public void Flash(string message, Color color)
        {
            Set(flashText, message, color, 0.5f);
            flashTimer = 2.6f;
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
            float top = hh - 0.75f;
            float left = -hw + 0.7f;

            // altın
            var king = gm.King;
            int coins = king != null ? king.Coins : 0;
            int cap = king != null ? king.Capacity : 0;
            coinIcon.transform.localPosition = new Vector3(left, top, 10f);
            coinText.transform.localPosition = new Vector3(left + 0.45f, top, 10f);
            Set(coinText, coins + "/" + cap, coins >= cap ? Pal.Flame : Pal.Gold);

            // nüfus
            rosterText.transform.localPosition = new Vector3(left - 0.1f, top - 0.95f, 10f);
            Set(rosterText, "KOYLU:" + gm.CountRole(VillagerRole.Peasant)
                            + " OKCU:" + gm.CountRole(VillagerRole.Archer)
                            + " INSA:" + gm.CountRole(VillagerRole.Builder)
                            + " CIFT:" + gm.CountRole(VillagerRole.Farmer)
                            + " SOVALYE:" + gm.CountRole(VillagerRole.Knight), Pal.White);

            // gün
            bool night = gm.IsNight;
            bool blood = gm.Clock != null && gm.Clock.BloodMoon;
            dayText.transform.localPosition = new Vector3(0f, top, 10f);
            Set(dayText, gm.Day + ". GUN", blood && night ? Pal.Cape : Pal.White, 0.5f);

            float p = night ? 1f - gm.Clock.NightProgress : gm.Clock.DayProgress;
            barBack.transform.localPosition = new Vector3(0f, top - 0.75f, 10f);
            Scale(barBack, 5.2f, 0.22f);
            Scale(barFill, Mathf.Max(0.001f, 5f * Mathf.Clamp01(p)), 0.14f);
            barFill.transform.localPosition = new Vector3(-2.5f + 2.5f * Mathf.Clamp01(p), top - 0.75f, 10f);
            barFill.color = night ? (blood ? Pal.Cape : new Color(0.55f, 0.65f, 1f)) : Pal.Gold;

            // mesaj
            flashTimer -= Time.deltaTime;
            flashText.transform.localPosition = new Vector3(0f, hh * 0.45f, 10f);
            float fa = Mathf.Clamp01(flashTimer / 0.8f);
            var fc = flashText.color;
            flashText.color = new Color(fc.r, fc.g, fc.b, fa);
            flashText.enabled = flashTimer > 0f;

            // kontrol ipucu
            hintTimer -= Time.deltaTime;
            hintText.transform.localPosition = new Vector3(0f, -hh + 0.7f, 10f);
            Set(hintText, "A/D YURU   SPACE ALTIN AT   R YENIDEN", Pal.White, 0.5f);
            float ha = Mathf.Clamp01(hintTimer / 3f);
            var hc = hintText.color;
            hintText.color = new Color(hc.r, hc.g, hc.b, ha);
            hintText.enabled = hintTimer > 0f;

            hintText2.transform.localPosition = new Vector3(0f, -hh + 1.25f, 10f);
            Set(hintText2, "CADIRDAN KOYLU TOPLA - TEZGAHTAN ALET VER - GECEYE OKCU HAZIRLA", Pal.Gold, 0.5f);
            hintText2.color = new Color(1f, 1f, 1f, ha);
            hintText2.enabled = hintTimer > 0f;

            UpdateWarnings(hw, hh);
            UpdateGameOver(hh);
        }

        void UpdateWarnings(float hw, float hh)
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
            float pulse = 0.4f + Util.Pulse(Time.time, 7f) * 0.6f;
            leftWarn.enabled = l;
            rightWarn.enabled = r;
            leftWarn.transform.localPosition = new Vector3(-hw + 0.5f, 0.6f, 10f);
            rightWarn.transform.localPosition = new Vector3(hw - 0.5f, 0.6f, 10f);
            var warnColor = new Color(Pal.Cape.r, Pal.Cape.g, Pal.Cape.b, pulse);
            leftWarn.color = warnColor;
            rightWarn.color = warnColor;
        }

        void UpdateGameOver(float hh)
        {
            if (overRoot == null || !overRoot.activeSelf) return;
            float hw = hh * cam.aspect;
            Scale(overDim, hw * 2.4f, hh * 2.4f);
            overDim.transform.localPosition = new Vector3(0f, 0f, 8f);
            overTitle.transform.localPosition = new Vector3(0f, 2.4f, 7.9f);
            overReason.transform.localPosition = new Vector3(0f, 1.2f, 7.9f);
            overDay.transform.localPosition = new Vector3(0f, 0.2f, 7.9f);
            overBest.transform.localPosition = new Vector3(0f, -0.7f, 7.9f);
            overHint.transform.localPosition = new Vector3(0f, -2.2f, 7.9f);
            float a = 0.35f + Util.Pulse(Time.time, 4f) * 0.65f;
            overHint.color = new Color(1f, 1f, 1f, a);
        }
    }
}
