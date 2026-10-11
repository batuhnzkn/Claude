using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Gökyüzü degradesi, yıldızlar, güneş/ay, bulutlar, paralaks tepeler,
    /// zemin şeridi ve gece karartması.</summary>
    public class Background : MonoBehaviour
    {
        class ParallaxLayer
        {
            public SpriteRenderer[] tiles;
            public float factor;
            public float tileWidth;
            public Color dayColor;
        }

        GameConfig cfg;
        DayNight dayNight;
        CameraRig rig;
        Transform camT;

        SpriteRenderer sky;
        Texture2D skyTex;
        Color32[] skyPixels;
        SpriteRenderer stars;
        SpriteRenderer sun;
        SpriteRenderer moon;
        SpriteRenderer night;
        readonly System.Collections.Generic.List<ParallaxLayer> layers = new System.Collections.Generic.List<ParallaxLayer>();
        readonly System.Collections.Generic.List<SpriteRenderer> clouds = new System.Collections.Generic.List<SpriteRenderer>();
        readonly System.Collections.Generic.List<float> cloudSpeeds = new System.Collections.Generic.List<float>();

        const int SkyRes = 64;

        public void Init(GameConfig config, DayNight dn, CameraRig cameraRig, Transform worldRoot)
        {
            cfg = config;
            dayNight = dn;
            rig = cameraRig;
            camT = rig.transform;

            float viewH = cfg.cameraSize * 2f;
            float viewW = rig.HalfWidth * 2f;

            // --- gökyüzü (kameraya bağlı)
            Texture2D tex;
            var skySprite = Art.MakeGradientSprite(SkyRes, out tex);
            skyTex = tex;
            skyPixels = new Color32[SkyRes];
            sky = Util.Spawn(camT, skySprite, new Vector3(0, 0, 20f), Layer.Sky, "sky");
            Fit(sky, viewW * 1.2f, viewH * 1.2f);

            // --- yıldızlar
            stars = Util.Spawn(camT, Art.Stars(240, 140, 420, 7), new Vector3(0, viewH * 0.18f, 19f), Layer.Stars, "stars");
            Fit(stars, viewW * 1.2f, viewH * 0.9f);
            stars.color = new Color(1, 1, 1, 0);

            // --- güneş & ay
            sun = Util.Spawn(camT, Art.Sun, new Vector3(0, 0, 18f), Layer.Celestial, "sun");
            moon = Util.Spawn(camT, Art.Moon, new Vector3(0, 0, 18f), Layer.Celestial, "moon");
            sun.transform.localScale = Vector3.one * 1.4f;
            moon.transform.localScale = Vector3.one * 1.2f;

            // --- bulutlar (dünyada, hafif paralaks)
            for (int i = 0; i < 10; i++)
            {
                var c = Util.Spawn(worldRoot, Art.Cloud(i % 4), Vector3.zero, Layer.Cloud, "cloud");
                c.transform.position = new Vector3(Util.Rand(-cfg.worldHalfWidth, cfg.worldHalfWidth),
                    Util.Rand(5.5f, 10f), 0f);
                c.transform.localScale = Vector3.one * Util.Rand(0.8f, 1.8f);
                c.color = new Color(1f, 1f, 1f, Util.Rand(0.45f, 0.8f));
                clouds.Add(c);
                cloudSpeeds.Add(Util.Rand(0.15f, 0.5f));
            }

            // --- paralaks tepeler
            AddLayer(worldRoot, Art.Hill(256, 72, 11, Color.white, 0.4f), 0.12f, Layer.HillFar, Pal.HillFar, -1.5f);
            AddLayer(worldRoot, Art.Hill(256, 96, 23, Color.white, 0.5f), 0.26f, Layer.HillMid, Pal.HillMid, -2.2f);
            AddLayer(worldRoot, Art.Hill(256, 120, 37, Color.white, 0.55f), 0.42f, Layer.HillNear, Pal.HillNear, -3.5f);
            AddLayer(worldRoot, Art.ForestStrip(256, 56, 51, Color.white), 0.66f, Layer.ForestBack, Pal.ForestBack, -0.4f);

            // --- zemin şeridi
            var groundRoot = Util.Node(worldRoot, "ground", Vector3.zero).transform;
            var strip = Art.GroundStrip(256, 128, 5);
            float stripW = strip.bounds.size.x;
            int count = Mathf.CeilToInt((cfg.worldHalfWidth * 2f + 40f) / stripW) + 1;
            for (int i = 0; i < count; i++)
            {
                float x = -cfg.worldHalfWidth - 20f + i * stripW;
                Util.Spawn(groundRoot, strip, new Vector3(x, cfg.groundY, 0f), Layer.Ground, "ground_strip");
            }

            // --- gece karartması
            night = Util.Spawn(camT, Art.Solid, new Vector3(0, 0, 9f), Layer.Night, "nightOverlay");
            Fit(night, viewW * 1.3f, viewH * 1.3f);
            night.color = new Color(0, 0, 0, 0);
        }

        void AddLayer(Transform parent, Sprite sprite, float factor, int order, Color color, float baseY)
        {
            float w = sprite.bounds.size.x;
            int n = Mathf.CeilToInt((rig.HalfWidth * 2f) / w) + 3;
            var layer = new ParallaxLayer { factor = factor, tileWidth = w, dayColor = color, tiles = new SpriteRenderer[n] };
            var root = Util.Node(parent, "parallax_" + order, Vector3.zero).transform;
            for (int i = 0; i < n; i++)
            {
                var sr = Util.Spawn(root, sprite, new Vector3(i * w, baseY, 0f), order, "tile");
                sr.color = color;
                layer.tiles[i] = sr;
            }
            layers.Add(layer);
        }

        static void Fit(SpriteRenderer sr, float w, float h)
        {
            Vector2 s = sr.sprite.bounds.size;
            sr.transform.localScale = new Vector3(w / Mathf.Max(0.0001f, s.x), h / Mathf.Max(0.0001f, s.y), 1f);
        }

        void LateUpdate()
        {
            if (cfg == null || dayNight == null) return;

            float dark = dayNight.Darkness;
            bool blood = dayNight.BloodMoon;
            float camX = camT.position.x;

            UpdateSky(dark, blood);
            UpdateCelestial(dark, blood);
            UpdateParallax(camX, dark, blood);
            UpdateClouds(dark, blood);

            Color nightTint = blood ? new Color(0.35f, 0.03f, 0.08f) : new Color(0.04f, 0.06f, 0.16f);
            night.color = new Color(nightTint.r, nightTint.g, nightTint.b, dark * (blood ? 0.5f : 0.62f));
            stars.color = new Color(1f, 1f, 1f, Mathf.Clamp01(dark * 1.2f - 0.15f) * (blood ? 0.4f : 1f));
        }

        void UpdateSky(float dark, bool blood)
        {
            Color top, bot;
            if (dark < 0.5f)
            {
                float t = dark / 0.5f;
                top = Color.Lerp(Pal.SkyDayTop, Pal.SkyDuskTop, t);
                bot = Color.Lerp(Pal.SkyDayBot, Pal.SkyDuskBot, t);
            }
            else
            {
                float t = (dark - 0.5f) / 0.5f;
                top = Color.Lerp(Pal.SkyDuskTop, blood ? Pal.SkyBloodTop : Pal.SkyNightTop, t);
                bot = Color.Lerp(Pal.SkyDuskBot, blood ? Pal.SkyBloodBot : Pal.SkyNightBot, t);
            }

            for (int y = 0; y < SkyRes; y++)
            {
                float u = y / (float)(SkyRes - 1);
                skyPixels[y] = Color.Lerp(bot, top, Mathf.Pow(u, 0.8f));
            }
            skyTex.SetPixels32(skyPixels);
            skyTex.Apply(false);
            if (rig.Cam != null) rig.Cam.backgroundColor = bot;
        }

        void UpdateCelestial(float dark, bool blood)
        {
            float t = dayNight.CelestialT;
            bool isNight = dayNight.IsNight;
            float hw = rig.HalfWidth, hh = cfg.cameraSize;
            float x = Mathf.Lerp(-hw * 0.8f, hw * 0.8f, t);
            float y = Mathf.Sin(t * Mathf.PI) * hh * 0.72f + hh * 0.05f;

            sun.enabled = !isNight;
            moon.enabled = isNight;
            if (!isNight)
            {
                sun.transform.localPosition = new Vector3(x, y, 18f);
                sun.color = Color.Lerp(Color.white, new Color(1f, 0.6f, 0.35f), dark);
            }
            else
            {
                moon.transform.localPosition = new Vector3(x, y, 18f);
                moon.color = blood ? new Color(1f, 0.32f, 0.3f) : Color.white;
                moon.transform.localScale = Vector3.one * (blood ? 1.7f : 1.2f);
            }
        }

        void UpdateParallax(float camX, float dark, bool blood)
        {
            for (int l = 0; l < layers.Count; l++)
            {
                var layer = layers[l];
                int n = layer.tiles.Length;
                float span = layer.tileWidth * n;
                float basePos = camX * (1f - layer.factor);
                float left = camX - span * 0.5f;
                for (int i = 0; i < n; i++)
                {
                    var tr = layer.tiles[i].transform;
                    float x = basePos + i * layer.tileWidth;
                    x = left + Mathf.Repeat(x - left, span);
                    tr.position = new Vector3(x, tr.position.y, 0f);
                }

                Color c = layer.dayColor;
                Color nightC = blood
                    ? new Color(c.r * 0.45f, c.g * 0.16f, c.b * 0.2f)
                    : new Color(c.r * 0.26f, c.g * 0.3f, c.b * 0.5f);
                Color final = Color.Lerp(c, nightC, dark);
                for (int i = 0; i < n; i++) layer.tiles[i].color = final;
            }
        }

        void UpdateClouds(float dark, bool blood)
        {
            float limit = cfg.worldHalfWidth + 30f;
            for (int i = 0; i < clouds.Count; i++)
            {
                var tr = clouds[i].transform;
                float x = tr.position.x + cloudSpeeds[i] * Time.deltaTime;
                if (x > limit) x = -limit;
                tr.position = new Vector3(x, tr.position.y, 0f);
                Color c = Color.Lerp(Color.white, blood ? new Color(0.5f, 0.2f, 0.25f) : new Color(0.25f, 0.3f, 0.5f), dark);
                clouds[i].color = new Color(c.r, c.g, c.b, clouds[i].color.a);
            }
        }
    }
}
