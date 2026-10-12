// Çalışan sahte Unity motoru: oyunu Unity olmadan simüle edip çökme/mantık hatası aramak için.
// Projeye dahil DEĞİLDİR, sadece doğrulama amaçlıdır.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine
{
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public float magnitude { get { return (float)Math.Sqrt(x * x + y * y); } }
        public Vector2 normalized { get { float m = magnitude; return m > 1e-6f ? new Vector2(x / m, y / m) : new Vector2(0, 0); } }
        public static Vector2 zero { get { return new Vector2(0, 0); } }
        public static Vector2 one { get { return new Vector2(1, 1); } }
        public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.x + b.x, a.y + b.y); }
        public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.x - b.x, a.y - b.y); }
        public static Vector2 operator *(Vector2 a, float b) { return new Vector2(a.x * b, a.y * b); }
        public static Vector2 operator *(float b, Vector2 a) { return new Vector2(a.x * b, a.y * b); }
        public static Vector2 operator /(Vector2 a, float b) { return new Vector2(a.x / b, a.y / b); }
        public static implicit operator Vector3(Vector2 v) { return new Vector3(v.x, v.y, 0f); }
        public static implicit operator Vector2(Vector3 v) { return new Vector2(v.x, v.y); }
        public override string ToString() { return "(" + x.ToString("0.0") + "," + y.ToString("0.0") + ")"; }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y) { this.x = x; this.y = y; this.z = 0f; }
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero { get { return new Vector3(0, 0, 0); } }
        public static Vector3 one { get { return new Vector3(1, 1, 1); } }
        public float magnitude { get { return (float)Math.Sqrt(x * x + y * y + z * z); } }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }
        public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); }
        public static Vector3 operator *(Vector3 a, float b) { return new Vector3(a.x * b, a.y * b, a.z * b); }
        public static Vector3 operator *(float b, Vector3 a) { return a * b; }
        public override string ToString() { return "(" + x.ToString("0.0") + "," + y.ToString("0.0") + ")"; }
    }

    public struct Quaternion
    {
        public float z;
        public static Quaternion identity { get { return new Quaternion(); } }
        public static Quaternion Euler(float x, float y, float zz) { var q = new Quaternion(); q.z = zz; return q; }
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; this.a = 1f; }
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white { get { return new Color(1, 1, 1, 1); } }
        public static Color black { get { return new Color(0, 0, 0, 1); } }
        public static Color clear { get { return new Color(0, 0, 0, 0); } }
        public static Color Lerp(Color x, Color y, float t)
        {
            t = Mathf.Clamp01(t);
            return new Color(x.r + (y.r - x.r) * t, x.g + (y.g - x.g) * t, x.b + (y.b - x.b) * t, x.a + (y.a - x.a) * t);
        }
        public static implicit operator Color32(Color c)
        {
            return new Color32((byte)(Mathf.Clamp01(c.r) * 255), (byte)(Mathf.Clamp01(c.g) * 255),
                               (byte)(Mathf.Clamp01(c.b) * 255), (byte)(Mathf.Clamp01(c.a) * 255));
        }
        public static implicit operator Color(Color32 c) { return new Color(c.r / 255f, c.g / 255f, c.b / 255f, c.a / 255f); }
    }

    public struct Color32
    {
        public byte r, g, b, a;
        public Color32(byte r, byte g, byte b, byte a) { this.r = r; this.g = g; this.b = b; this.a = a; }
    }

    public struct Rect
    {
        public float x, y, width, height;
        public Rect(float x, float y, float w, float h) { this.x = x; this.y = y; width = w; height = h; }
    }

    public struct Bounds
    {
        public Vector3 size;
        public Bounds(Vector3 s) { size = s; }
    }

    public static class Mathf
    {
        public const float PI = 3.14159265f;
        public const float Rad2Deg = 57.29578f;
        public const float Deg2Rad = 0.0174533f;
        public static float Abs(float v) { return Math.Abs(v); }
        public static int Abs(int v) { return Math.Abs(v); }
        public static float Min(float a, float b) { return Math.Min(a, b); }
        public static int Min(int a, int b) { return Math.Min(a, b); }
        public static float Max(float a, float b) { return Math.Max(a, b); }
        public static int Max(int a, int b) { return Math.Max(a, b); }
        public static float Clamp(float v, float a, float b) { return v < a ? a : (v > b ? b : v); }
        public static int Clamp(int v, int a, int b) { return v < a ? a : (v > b ? b : v); }
        public static float Clamp01(float v) { return v < 0f ? 0f : (v > 1f ? 1f : v); }
        public static float Lerp(float a, float b, float t) { return a + (b - a) * Clamp01(t); }
        public static float MoveTowards(float a, float b, float d)
        {
            if (Math.Abs(b - a) <= d) return b;
            return a + Math.Sign(b - a) * d;
        }
        public static float Sin(float v) { return (float)Math.Sin(v); }
        public static float Cos(float v) { return (float)Math.Cos(v); }
        public static float Atan2(float a, float b) { return (float)Math.Atan2(a, b); }
        public static float Sqrt(float v) { return (float)Math.Sqrt(v); }
        public static float Pow(float a, float b) { return (float)Math.Pow(a, b); }
        public static float Exp(float a) { return (float)Math.Exp(a); }
        public static float Floor(float v) { return (float)Math.Floor(v); }
        public static int FloorToInt(float v) { return (int)Math.Floor(v); }
        public static int CeilToInt(float v) { return (int)Math.Ceiling(v); }
        public static int RoundToInt(float v) { return (int)Math.Round(v, MidpointRounding.AwayFromZero); }
        public static float Repeat(float t, float len) { return t - (float)Math.Floor(t / len) * len; }
        public static float Sign(float v) { return v >= 0 ? 1f : -1f; }
    }

    public static class Time
    {
        public static float deltaTime = 1f / 60f;
        public static float time = 0f;
    }

    public static class Random
    {
        static System.Random rng = new System.Random(12345);
        public static float value { get { return (float)rng.NextDouble(); } }
        public static float Range(float a, float b) { return a + (float)rng.NextDouble() * (b - a); }
        public static int Range(int a, int b) { return b <= a ? a : a + rng.Next(b - a); }
    }

    public static class Debug
    {
        public static List<string> Logs = new List<string>();
        public static void Log(object o) { Logs.Add("LOG: " + o); }
        public static void LogWarning(object o) { Logs.Add("WARN: " + o); }
        public static void LogError(object o) { Logs.Add("ERR: " + o); }
    }

    public class Object
    {
        public string name { get; set; }
        internal bool destroyed;
        static bool IsNull(Object o) { return ReferenceEquals(o, null) || o.destroyed; }
        public static void Destroy(Object o) { Runtime.Destroy(o); }
        public static void Destroy(Object o, float t) { Runtime.Destroy(o); }
        public static bool operator ==(Object a, Object b)
        {
            if (IsNull(a) && IsNull(b)) return true;
            return ReferenceEquals(a, b);
        }
        public static bool operator !=(Object a, Object b) { return !(a == b); }
        public override bool Equals(object o) { return ReferenceEquals(this, o); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public static implicit operator bool(Object o) { return !IsNull(o); }
    }

    public class Component : Object
    {
        internal GameObject go;
        public GameObject gameObject { get { return go; } }
        public Transform transform { get { return go != null ? go.transform : null; } }
        public string tag { get { return go.tag; } set { go.tag = value; } }
        public T GetComponent<T>() where T : class { return go.GetComponent<T>(); }
        public T AddComponent<T>() where T : Component, new() { return go.AddComponent<T>(); }
    }

    public class Behaviour : Component { public bool enabled { get; set; } public Behaviour() { enabled = true; } }

    public class MonoBehaviour : Behaviour { }

    public class Transform : Component
    {
        Vector3 local;
        public Transform parent;
        public List<Transform> children = new List<Transform>();
        public Vector3 localScale = Vector3.one;
        public Quaternion rotation;

        public Vector3 localPosition
        {
            get { return local; }
            set { local = value; }
        }

        public Vector3 position
        {
            get
            {
                var p = local;
                var t = parent;
                int guard = 0;
                while (t != null && guard++ < 64) { p = p + t.localPosition; t = t.parent; }
                return p;
            }
            set
            {
                var basePos = Vector3.zero;
                var t = parent;
                int guard = 0;
                while (t != null && guard++ < 64) { basePos = basePos + t.localPosition; t = t.parent; }
                local = value - basePos;
            }
        }

        public void SetParent(Transform p, bool worldPositionStays)
        {
            if (parent != null) parent.children.Remove(this);
            parent = p;
            if (p != null) p.children.Add(this);
        }

        public void Rotate(float x, float y, float z) { rotation.z += z; }
    }

    public class GameObject : Object
    {
        public Transform transform;
        public string tag = "Untagged";
        public bool activeSelf = true;
        internal List<Component> components = new List<Component>();

        public GameObject() : this("GameObject") { }

        public GameObject(string n)
        {
            name = n;
            transform = new Transform();
            transform.go = this;
            components.Add(transform);
            Runtime.Register(this);
        }

        public void SetActive(bool v) { activeSelf = v; }

        public T AddComponent<T>() where T : Component, new()
        {
            var c = new T();
            c.go = this;
            c.name = name;
            components.Add(c);
            Runtime.OnComponentAdded(c);
            return c;
        }

        public T GetComponent<T>() where T : class
        {
            for (int i = 0; i < components.Count; i++)
                if (components[i] is T) return components[i] as T;
            return null;
        }
    }

    public class Renderer : Component
    {
        public bool enabled { get; set; }
        public int sortingOrder { get; set; }
        public Renderer() { enabled = true; }
    }

    public class SpriteRenderer : Renderer
    {
        public Sprite sprite { get; set; }
        public Color color { get; set; }
        public bool flipX { get; set; }
        public bool flipY { get; set; }
        public SpriteRenderer() { color = Color.white; }
    }

    public enum SpriteMeshType { FullRect, Tight }

    public class Sprite : Object
    {
        public static int Created;
        float w, h;
        public Texture2D texture;
        public Rect rect;
        public Vector2 pivot;
        public float ppu = 100f;
        public Bounds bounds { get { return new Bounds(new Vector3(w, h, 0f)); } }
        public static Sprite Create(Texture2D t, Rect r, Vector2 pivot) { return Create(t, r, pivot, 100f); }
        public static Sprite Create(Texture2D t, Rect r, Vector2 pivot, float ppu)
        {
            if (t == null) throw new Exception("Sprite.Create: texture null");
            if (ppu <= 0f) throw new Exception("Sprite.Create: ppu <= 0");
            var s = new Sprite();
            Created++;
            s.texture = t;
            s.rect = r;
            s.pivot = pivot;
            s.ppu = ppu;
            s.w = r.width / ppu; s.h = r.height / ppu;
            return s;
        }
        public static Sprite Create(Texture2D t, Rect r, Vector2 pivot, float ppu, uint extrude, SpriteMeshType mt)
        {
            return Create(t, r, pivot, ppu);
        }
    }

    public enum TextureFormat { RGBA32, ARGB32 }
    public enum FilterMode { Point, Bilinear, Trilinear }
    public enum TextureWrapMode { Repeat, Clamp }

    public class Texture : Object { }

    public class Texture2D : Texture
    {
        public static int Created;
        public int width, height;
        public Color32[] data;
        public Texture2D(int w, int h) { width = w; height = h; data = new Color32[w * h]; }
        public Texture2D(int w, int h, TextureFormat f, bool mip) { width = w; height = h; data = new Color32[w * h]; Created++; }
        public FilterMode filterMode { get; set; }
        public TextureWrapMode wrapMode { get; set; }
        public void SetPixels32(Color32[] px)
        {
            if (px == null || px.Length != width * height)
                throw new Exception("SetPixels32 boyut hatasi: " + (px == null ? 0 : px.Length) + " != " + (width * height));
            Array.Copy(px, data, px.Length);
        }
        public Color32 Pixel(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return new Color32(0, 0, 0, 0);
            return data[y * width + x];
        }
        public void Apply() { }
        public void Apply(bool m) { }
    }

    public enum CameraClearFlags { Skybox, SolidColor, Depth, Nothing }

    public class Camera : Behaviour
    {
        public bool orthographic { get; set; }
        public float orthographicSize { get; set; }
        public CameraClearFlags clearFlags { get; set; }
        public Color backgroundColor { get; set; }
        public float nearClipPlane { get; set; }
        public float farClipPlane { get; set; }
        public float aspect { get { return 16f / 9f; } }
        public static Camera main { get { return null; } }
        public static Camera[] allCameras { get { return Runtime.ActiveCameras(); } }
    }

    public class AudioClip : Object
    {
        public int samples;
        public static AudioClip Create(string n, int lengthSamples, int ch, int freq, bool stream)
        {
            if (lengthSamples <= 0) throw new Exception("AudioClip.Create: gecersiz uzunluk");
            var c = new AudioClip(); c.samples = lengthSamples; c.name = n; return c;
        }
        public bool SetData(float[] data, int offset)
        {
            if (data == null || data.Length != samples) throw new Exception("AudioClip.SetData boyut hatasi");
            return true;
        }
    }

    public class AudioSource : Behaviour
    {
        public bool playOnAwake { get; set; }
        public float spatialBlend { get; set; }
        public float pitch { get; set; }
        public float volume { get; set; }
        public static int PlayCount;
        public void PlayOneShot(AudioClip c, float v) { if (c != null) PlayCount++; }
        public void PlayOneShot(AudioClip c) { PlayCount++; }
    }

    public class AudioListener : Behaviour { }

    public static class PlayerPrefs
    {
        static Dictionary<string, int> ints = new Dictionary<string, int>();
        public static int GetInt(string k, int d) { int v; return ints.TryGetValue(k, out v) ? v : d; }
        public static void SetInt(string k, int v) { ints[k] = v; }
        public static void Save() { }
        public static void DeleteKey(string k) { ints.Remove(k); }
    }

    public static class Application
    {
        public static int targetFrameRate { get; set; }
        public static bool isPlaying { get { return true; } }
        public static void Quit() { }
    }

    public enum RuntimeInitializeLoadType { AfterSceneLoad, BeforeSceneLoad, BeforeSplashScreen, SubsystemRegistration }

    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeInitializeOnLoadMethodAttribute : Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute() { }
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType t) { }
    }

    [AttributeUsage(AttributeTargets.Field)] public class HeaderAttribute : Attribute { public HeaderAttribute(string h) { } }
    [AttributeUsage(AttributeTargets.Field)] public class TooltipAttribute : Attribute { public TooltipAttribute(string h) { } }
    [AttributeUsage(AttributeTargets.Field)] public class RangeAttribute : Attribute { public RangeAttribute(float a, float b) { } }
    [AttributeUsage(AttributeTargets.Field)] public class HideInInspector : Attribute { }
    [AttributeUsage(AttributeTargets.Field)] public class SerializeField : Attribute { }

    public enum KeyCode { A, D, S, W, R, P, Space, LeftArrow, RightArrow, DownArrow, UpArrow, Escape, JoystickButton0 }

    public static class Input
    {
        public static float FakeHorizontal;
        public static bool FakePay;
        public static bool FakeRestart;
        public static bool GetKey(KeyCode k)
        {
            if (k == KeyCode.A || k == KeyCode.LeftArrow) return FakeHorizontal < -0.5f;
            if (k == KeyCode.D || k == KeyCode.RightArrow) return FakeHorizontal > 0.5f;
            if (k == KeyCode.Space) return FakePay;
            return false;
        }
        public static bool GetKeyDown(KeyCode k) { return k == KeyCode.R && FakeRestart; }
        public static float GetAxisRaw(string a) { return 0f; }
        public static float GetAxis(string a) { return 0f; }
    }

    /// <summary>Sahte oyun döngüsü: Awake/Start/Update/LateUpdate çağırır, Destroy'u işler.</summary>
    public static class Runtime
    {
        static readonly List<GameObject> objects = new List<GameObject>();
        static readonly List<Component> pendingStart = new List<Component>();
        static readonly Dictionary<Type, MethodInfo[]> cache = new Dictionary<Type, MethodInfo[]>();
        public static int Frame;

        public static void Reset() { objects.Clear(); pendingStart.Clear(); Frame = 0; Time.time = 0f; }

        public static void Register(GameObject go) { objects.Add(go); }

        static MethodInfo[] Methods(Type t)
        {
            MethodInfo[] m;
            if (cache.TryGetValue(t, out m)) return m;
            const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            m = new MethodInfo[4];
            m[0] = Find(t, "Awake"); m[1] = Find(t, "Start"); m[2] = Find(t, "Update"); m[3] = Find(t, "LateUpdate");
            cache[t] = m;
            return m;
        }

        static MethodInfo Find(Type t, string name)
        {
            const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            while (t != null && t != typeof(object))
            {
                var mi = t.GetMethod(name, F, null, Type.EmptyTypes, null);
                if (mi != null) return mi;
                t = t.BaseType;
            }
            return null;
        }

        public static void OnComponentAdded(Component c)
        {
            var m = Methods(c.GetType());
            if (m[0] != null) Invoke(m[0], c);
            if (m[1] != null) pendingStart.Add(c);
        }

        static void Invoke(MethodInfo mi, Component c)
        {
            try { mi.Invoke(c, null); }
            catch (TargetInvocationException e)
            {
                var inner = e.InnerException;
                throw new Exception(c.GetType().Name + "." + mi.Name + " -> " + inner.GetType().Name +
                                    ": " + inner.Message + "\n--- ic yigin ---\n" + inner.StackTrace, inner);
            }
        }

        public static void Destroy(Object o)
        {
            if (o == null) return;
            var go = o as GameObject;
            if (go != null) { DestroyGO(go); return; }
            var c = o as Component;
            if (c != null) { c.destroyed = true; if (c.go != null) c.go.components.Remove(c); }
        }

        static void DestroyGO(GameObject go)
        {
            if (go.destroyed) return;
            go.destroyed = true;
            for (int i = 0; i < go.components.Count; i++)
            {
                var c = go.components[i];
                c.destroyed = true;
                var m = Methods(c.GetType());
                var od = Find(c.GetType(), "OnDestroy");
                if (od != null) Invoke(od, c);
            }
            var kids = go.transform.children.ToArray();
            for (int i = 0; i < kids.Length; i++)
                if (kids[i] != null && kids[i].go != null) DestroyGO(kids[i].go);
        }

        public static Camera[] ActiveCameras()
        {
            var list = new List<Camera>();
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].destroyed) continue;
                var c = objects[i].GetComponent<Camera>();
                if (c != null && c.enabled) list.Add(c);
            }
            return list.ToArray();
        }

        public static List<GameObject> AllObjects() { return objects; }

        public static int LiveObjects
        {
            get { int n = 0; for (int i = 0; i < objects.Count; i++) if (!objects[i].destroyed) n++; return n; }
        }

        public static void Step(float dt)
        {
            Frame++;
            Time.deltaTime = dt;
            Time.time += dt;

            var starts = pendingStart.ToArray();
            pendingStart.Clear();
            for (int i = 0; i < starts.Length; i++)
            {
                if (starts[i].destroyed) continue;
                var m = Methods(starts[i].GetType());
                if (m[1] != null) Invoke(m[1], starts[i]);
            }

            var snapshot = objects.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                var go = snapshot[i];
                if (go.destroyed || !go.activeSelf) continue;
                var comps = go.components.ToArray();
                for (int j = 0; j < comps.Length; j++)
                {
                    var c = comps[j];
                    if (c.destroyed) continue;
                    var b = c as Behaviour;
                    if (b != null && !b.enabled) continue;
                    var m = Methods(c.GetType());
                    if (m[2] != null) Invoke(m[2], c);
                }
            }

            for (int i = 0; i < snapshot.Length; i++)
            {
                var go = snapshot[i];
                if (go.destroyed || !go.activeSelf) continue;
                var comps = go.components.ToArray();
                for (int j = 0; j < comps.Length; j++)
                {
                    var c = comps[j];
                    if (c.destroyed) continue;
                    var b = c as Behaviour;
                    if (b != null && !b.enabled) continue;
                    var m = Methods(c.GetType());
                    if (m[3] != null) Invoke(m[3], c);
                }
            }

            objects.RemoveAll(o => o.destroyed);
        }
    }
}

namespace UnityEngine.SceneManagement { public class Scene { } }

namespace UnityEngine
{
    /// <summary>Sahnedeki tüm SpriteRenderer'ları yazılımda birleştirip ham RGBA dosyası yazar.</summary>
    public static class Shot
    {
        struct Item { public SpriteRenderer sr; public int order; public int seq; }

        public static void Capture(Camera cam, int W, int H, string path)
        {
            var fb = new Color[W * H];
            Color bg = cam.backgroundColor;
            bg.a = 1f;
            for (int i = 0; i < fb.Length; i++) fb[i] = bg;

            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            Vector3 camPos = cam.transform.position;
            float left = camPos.x - halfW;
            float bottom = camPos.y - halfH;
            float pxPerUnit = W / (2f * halfW);

            var items = new List<Item>();
            var objs = Runtime.AllObjects();
            int seq = 0;
            for (int i = 0; i < objs.Count; i++)
            {
                var go = objs[i];
                if (go.destroyed || !go.activeSelf) continue;
                if (!AncestorsActive(go)) continue;
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr == null || sr.destroyed || !sr.enabled || sr.sprite == null) continue;
                items.Add(new Item { sr = sr, order = sr.sortingOrder, seq = seq++ });
            }
            items.Sort((a, b) => a.order != b.order ? a.order.CompareTo(b.order) : a.seq.CompareTo(b.seq));

            for (int i = 0; i < items.Count; i++) Blit(items[i].sr, fb, W, H, left, bottom, pxPerUnit);

            var bytes = new byte[W * H * 4];
            for (int i = 0; i < fb.Length; i++)
            {
                bytes[i * 4 + 0] = (byte)(Mathf.Clamp01(fb[i].r) * 255f);
                bytes[i * 4 + 1] = (byte)(Mathf.Clamp01(fb[i].g) * 255f);
                bytes[i * 4 + 2] = (byte)(Mathf.Clamp01(fb[i].b) * 255f);
                bytes[i * 4 + 3] = 255;
            }
            System.IO.File.WriteAllBytes(path, bytes);
            Console.WriteLine("[shot] " + path + " (" + W + "x" + H + ", " + items.Count + " sprite)");
        }

        static bool AncestorsActive(GameObject go)
        {
            var t = go.transform.parent;
            int guard = 0;
            while (t != null && guard++ < 64)
            {
                if (t.go == null || t.go.destroyed || !t.go.activeSelf) return false;
                t = t.parent;
            }
            return true;
        }

        static void Accum(Transform t, out Vector3 scale, out float angleDeg)
        {
            scale = new Vector3(1, 1, 1);
            angleDeg = 0f;
            var cur = t;
            int guard = 0;
            while (cur != null && guard++ < 64)
            {
                scale.x *= cur.localScale.x;
                scale.y *= cur.localScale.y;
                angleDeg += cur.rotation.z;
                cur = cur.parent;
            }
        }

        static void Blit(SpriteRenderer sr, Color[] fb, int W, int H, float left, float bottom, float ppx)
        {
            var sp = sr.sprite;
            var tex = sp.texture;
            if (tex == null || tex.data == null) return;

            Vector3 scale;
            float angle;
            Accum(sr.transform, out scale, out angle);
            bool flipX = sr.flipX ^ (scale.x < 0f);
            bool flipY = scale.y < 0f;
            float sw = sp.rect.width / sp.ppu * Mathf.Abs(scale.x);
            float sh = sp.rect.height / sp.ppu * Mathf.Abs(scale.y);
            if (sw <= 0f || sh <= 0f) return;

            Vector3 pos = sr.transform.position;
            float pvx = sp.pivot.x * sw;
            float pvy = sp.pivot.y * sh;

            // döndürülmüş dörtgenin ekran sınırları
            float rad = angle * Mathf.Deg2Rad;
            float ca = Mathf.Cos(rad), sa = Mathf.Sin(rad);
            float minX = 1e9f, maxX = -1e9f, minY = 1e9f, maxY = -1e9f;
            for (int c = 0; c < 4; c++)
            {
                float lx = (c == 0 || c == 3) ? -pvx : sw - pvx;
                float ly = (c < 2) ? -pvy : sh - pvy;
                float wx = pos.x + lx * ca - ly * sa;
                float wy = pos.y + lx * sa + ly * ca;
                if (wx < minX) minX = wx; if (wx > maxX) maxX = wx;
                if (wy < minY) minY = wy; if (wy > maxY) maxY = wy;
            }

            int x0 = Mathf.Max(0, Mathf.FloorToInt((minX - left) * ppx));
            int x1 = Mathf.Min(W - 1, Mathf.CeilToInt((maxX - left) * ppx));
            int y0 = Mathf.Max(0, Mathf.FloorToInt((minY - bottom) * ppx));
            int y1 = Mathf.Min(H - 1, Mathf.CeilToInt((maxY - bottom) * ppx));
            if (x1 < x0 || y1 < y0) return;

            Color tint = sr.color;
            for (int py = y0; py <= y1; py++)
            {
                float wy = bottom + (py + 0.5f) / ppx;
                int row = (H - 1 - py) * W;
                for (int px = x0; px <= x1; px++)
                {
                    float wx = left + (px + 0.5f) / ppx;
                    float dx = wx - pos.x, dy = wy - pos.y;
                    float lx = dx * ca + dy * sa;      // ters döndürme
                    float ly = -dx * sa + dy * ca;
                    float u = (lx + pvx) / sw;
                    float v = (ly + pvy) / sh;
                    if (u < 0f || u >= 1f || v < 0f || v >= 1f) continue;
                    if (flipX) u = 1f - u;
                    if (flipY) v = 1f - v;
                    int tx = (int)(sp.rect.x + u * sp.rect.width);
                    int ty = (int)(sp.rect.y + v * sp.rect.height);
                    Color32 c32 = tex.Pixel(tx, ty);
                    if (c32.a == 0) continue;
                    float a = (c32.a / 255f) * tint.a;
                    if (a <= 0.004f) continue;
                    float r = (c32.r / 255f) * tint.r;
                    float g = (c32.g / 255f) * tint.g;
                    float b = (c32.b / 255f) * tint.b;
                    var dst = fb[row + px];
                    fb[row + px] = new Color(dst.r + (r - dst.r) * a, dst.g + (g - dst.g) * a, dst.b + (b - dst.b) * a, 1f);
                }
            }
        }
    }
}
