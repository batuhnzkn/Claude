using System.Collections.Generic;
using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Tüm ses efektleri çalışma anında matematiksel olarak üretilir;
    /// projede ses dosyası yoktur.
    /// </summary>
    public class Sfx : MonoBehaviour
    {
        public static Sfx I;

        const int Rate = 22050;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        AudioSource[] sources;
        int next;

        [Range(0f, 1f)] public float masterVolume = 0.6f;

        void Awake()
        {
            I = this;
            sources = new AudioSource[6];
            for (int i = 0; i < sources.Length; i++)
            {
                var go = new GameObject("audio" + i);
                go.transform.SetParent(transform, false);
                var a = go.AddComponent<AudioSource>();
                a.playOnAwake = false;
                a.spatialBlend = 0f;
                sources[i] = a;
            }
            BuildClips();
        }

        void OnDestroy() { if (I == this) I = null; }

        public static void Play(string name, float volume = 1f, float pitchJitter = 0.06f)
        {
            if (I == null) return;
            I.PlayLocal(name, volume, pitchJitter);
        }

        void PlayLocal(string name, float volume, float pitchJitter)
        {
            AudioClip c;
            if (!clips.TryGetValue(name, out c) || c == null) return;
            var src = sources[next];
            next = (next + 1) % sources.Length;
            src.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            src.PlayOneShot(c, Mathf.Clamp01(volume * masterVolume));
        }

        // ------------------------------------------------------------ üretim

        static AudioClip Build(string name, float dur, System.Func<float, float, float> gen)
        {
            int n = Mathf.Max(8, (int)(dur * Rate));
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float u = i / (float)(n - 1);
                data[i] = Mathf.Clamp(gen(t, u), -1f, 1f);
            }
            var clip = AudioClip.Create(name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Sine(float t, float f) { return Mathf.Sin(t * f * Mathf.PI * 2f); }

        static float Square(float t, float f) { return Sine(t, f) >= 0f ? 1f : -1f; }

        static float Saw(float t, float f) { return (t * f) % 1f * 2f - 1f; }

        static System.Random noiseRng = new System.Random(1234);
        static float Noise() { return (float)(noiseRng.NextDouble() * 2.0 - 1.0); }

        static float Decay(float u, float power) { return Mathf.Pow(1f - u, power); }

        void BuildClips()
        {
            clips["coin"] = Build("coin", 0.18f, (t, u) =>
                (Sine(t, 1180f) * 0.6f + Sine(t, 1760f) * 0.4f) * Decay(u, 3f) * 0.5f);

            clips["drop"] = Build("drop", 0.08f, (t, u) => Sine(t, 540f + 300f * u) * Decay(u, 2f) * 0.4f);

            clips["bow"] = Build("bow", 0.14f, (t, u) =>
                (Noise() * 0.5f + Sine(t, 300f - 180f * u) * 0.5f) * Decay(u, 4f) * 0.45f);

            clips["arrowhit"] = Build("arrowhit", 0.07f, (t, u) => (Noise() * 0.7f + Square(t, 900f) * 0.3f) * Decay(u, 6f) * 0.4f);

            clips["greeddie"] = Build("greeddie", 0.3f, (t, u) =>
                (Square(t, 420f - 300f * u) * 0.35f + Noise() * 0.25f) * Decay(u, 2.2f) * 0.5f);

            clips["greedhit"] = Build("greedhit", 0.12f, (t, u) => Square(t, 180f - 60f * u) * Decay(u, 3f) * 0.35f);

            clips["recruit"] = Build("recruit", 0.34f, (t, u) =>
            {
                float f = u < 0.33f ? 520f : (u < 0.66f ? 700f : 940f);
                return Sine(t, f) * Decay((u * 3f) % 1f, 1.6f) * 0.4f;
            });

            clips["build"] = Build("build", 0.2f, (t, u) =>
                (Sine(t, 160f) * 0.6f + Noise() * 0.3f) * Decay(u, 3.5f) * 0.55f);

            clips["upgrade"] = Build("upgrade", 0.45f, (t, u) =>
            {
                float f = u < 0.3f ? 440f : (u < 0.6f ? 660f : 880f);
                return (Sine(t, f) * 0.7f + Sine(t, f * 2f) * 0.2f) * Decay((u * 3.33f) % 1f, 1.4f) * 0.4f;
            });

            clips["horn"] = Build("horn", 1.3f, (t, u) =>
            {
                float env = Mathf.Min(1f, u * 6f) * Decay(u, 1.3f);
                return (Saw(t, 104f) * 0.35f + Saw(t, 156f) * 0.2f + Sine(t, 52f) * 0.3f) * env * 0.5f;
            });

            clips["bloodmoon"] = Build("bloodmoon", 1.8f, (t, u) =>
            {
                float env = Mathf.Min(1f, u * 4f) * Decay(u, 1.1f);
                return (Saw(t, 61f) * 0.3f + Saw(t, 87f) * 0.25f + Noise() * 0.1f) * env * 0.5f;
            });

            clips["hurt"] = Build("hurt", 0.26f, (t, u) =>
                Square(t, 220f - 90f * u + Mathf.Sin(t * 40f) * 20f) * Decay(u, 2f) * 0.42f);

            clips["steal"] = Build("steal", 0.3f, (t, u) => Sine(t, 900f - 600f * u) * Decay(u, 1.8f) * 0.4f);

            clips["gameover"] = Build("gameover", 1.4f, (t, u) =>
            {
                float f = u < 0.25f ? 330f : (u < 0.5f ? 262f : (u < 0.75f ? 220f : 165f));
                return (Sine(t, f) * 0.5f + Sine(t, f * 0.5f) * 0.3f) * Decay((u * 4f) % 1f, 1.2f) * 0.45f;
            });

            clips["newday"] = Build("newday", 0.6f, (t, u) =>
            {
                float f = u < 0.5f ? 523f : 784f;
                return (Sine(t, f) * 0.6f + Sine(t, f * 1.5f) * 0.25f) * Decay((u * 2f) % 1f, 1.5f) * 0.35f;
            });

            clips["critter"] = Build("critter", 0.12f, (t, u) => Sine(t, 1400f - 500f * u) * Decay(u, 4f) * 0.25f);

            clips["break"] = Build("break", 0.4f, (t, u) => (Noise() * 0.6f + Sine(t, 120f - 60f * u) * 0.4f) * Decay(u, 2f) * 0.6f);
        }
    }
}
