using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Oyunun açılış noktası. Sahnede hiçbir hazırlık gerektirmez: oyun başlar
    /// başlamaz kamerayı, dünyayı, kralı ve arayüzü kod ile kurar.
    /// Boş bir sahnede Play'e basmak yeterlidir.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        /// <summary>Sahneye elle kurulum yapmak isteyenler kapatabilsin diye.</summary>
        public static bool AutoCreate = true;

        static GameBootstrap instance;

        [Tooltip("Oyunun tüm denge ayarları.")]
        public GameConfig Config = new GameConfig();

        GameObject gameRoot;
        int restartFrames;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (!AutoCreate || instance != null) return;
            var go = new GameObject("Kingdom2D");
            go.AddComponent<GameBootstrap>();
        }

        void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            EnsureAudioListener();
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        void Start()
        {
            if (gameRoot == null) Build();
        }

        void Update()
        {
            if (restartFrames > 0)
            {
                restartFrames--;
                if (restartFrames == 0) Build();
            }
        }

        public static void RequestRestart()
        {
            if (instance == null) return;
            instance.DoRestart();
        }

        void DoRestart()
        {
            if (gameRoot != null) Destroy(gameRoot);
            gameRoot = null;
            restartFrames = 3;
        }

        void EnsureAudioListener()
        {
            var cams = Camera.allCameras;
            for (int i = 0; i < cams.Length; i++)
                if (cams[i] != null && cams[i].GetComponent<AudioListener>() != null) return;
            if (GetComponent<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
        }

        void Build()
        {
            var root = new GameObject("Kingdom2D Game");
            gameRoot = root;

            var gm = root.AddComponent<GameManager>();
            gm.Config = Config;
            gm.WorldRoot = Util.Node(root.transform, "World", Vector3.zero).transform;
            gm.ActorRoot = Util.Node(root.transform, "Actors", Vector3.zero).transform;
            gm.FxRoot = Util.Node(root.transform, "Fx", Vector3.zero).transform;

            root.AddComponent<Sfx>();

            // --- kamera
            var others = Camera.allCameras;
            for (int i = 0; i < others.Length; i++)
                if (others[i] != null) others[i].enabled = false;

            var camGO = Util.Node(root.transform, "Kingdom Camera", new Vector3(0f, Config.cameraHeight, -10f));
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            var rig = camGO.AddComponent<CameraRig>();

            // --- saat
            var clock = root.AddComponent<DayNight>();
            gm.Clock = clock;

            // --- kral
            var king = PlayerKing.Create(gm.ActorRoot, 4.5f, Config.startCoins);
            gm.King = king;

            rig.Init(Config, cam, king.transform);
            gm.Rig = rig;

            // --- arayüz
            var hud = root.AddComponent<Hud>();
            gm.Hud = hud;
            hud.Init(Config, gm, cam);

            // --- dünya
            var map = root.AddComponent<WorldMap>();
            gm.Map = map;
            map.Build(Config, gm.WorldRoot, gm.ActorRoot);

            // --- arka plan
            var bg = root.AddComponent<Background>();
            gm.Bg = bg;
            bg.Init(Config, clock, rig, gm.WorldRoot);

            clock.Init(Config, 1);
            gm.HookClock();

            hud.Flash("KRALLIGINI KUR", Pal.Gold);
        }
    }
}
