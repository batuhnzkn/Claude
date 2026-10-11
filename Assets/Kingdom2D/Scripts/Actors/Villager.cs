using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Krallığın insanı. Rolü (çiftçi, okçu, inşaatçı, şövalye) aldığı alete göre
    /// değişir; davranışı RoleBehavior sınıflarında tanımlıdır.
    /// </summary>
    public class Villager : Actor
    {
        public VillagerRole Role = VillagerRole.Peasant;
        public int CarriedCoins;
        public float Hp = 3f;

        public bool Captured;
        public Greed Captor;

        // roller tarafından kullanılan durum
        public Wall Post;
        public int PostIndex = -1;
        public float TargetHeight;

        // tezgaha gidip alet alma
        public bool HasPendingTool;
        public VillagerRole PendingRole;
        public float PendingStandX;
        public Farm WorkFarm;
        public float Timer;
        public float AttackTimer;
        public float WanderX;

        RoleBehavior behavior;
        SpriteRenderer carryIcon;
        float hurtFlash;

        public bool Alive { get { return this != null && Hp > 0f; } }

        public static Villager Create(Transform parent, float x, VillagerRole role)
        {
            var sr = Util.Spawn(parent, Art.Villager(VillagerRole.Peasant, 0), new Vector3(x, 0f, 0f),
                Layer.Villager, "Villager");
            var v = sr.gameObject.AddComponent<Villager>();
            v.X = x;
            v.WanderX = x;
            v.animStep = 0.22f;
            if (GameManager.I != null) GameManager.I.Villagers.Add(v);
            v.SetRole(role);
            return v;
        }

        public void SetRole(VillagerRole role)
        {
            if (behavior != null) behavior.Exit();
            LeavePost();
            LeaveFarm();
            Role = role;
            HasPendingTool = false;
            switch (role)
            {
                case VillagerRole.Archer: behavior = new ArcherRole(); Hp = 3f; break;
                case VillagerRole.Builder: behavior = new BuilderRole(); Hp = 4f; break;
                case VillagerRole.Farmer: behavior = new FarmerRole(); Hp = 3f; break;
                case VillagerRole.Knight: behavior = new KnightRole(); Hp = 34f; break;
                default: behavior = new PeasantRole(); Hp = 3f; break;
            }
            behavior.Enter(this);
            if (sr != null) sr.sprite = Art.Villager(Role, 0);
            Sfx.Play("recruit", 0.5f);
            Fx.Burst(Center, Pal.Gold, 6, 2.2f, 0.7f);
        }

        protected override void UpdateSprite()
        {
            sr.sprite = Art.Villager(Role, frame);
            sr.color = hurtFlash > 0f ? Color.Lerp(Color.white, new Color(1f, 0.3f, 0.3f), hurtFlash) : Color.white;
        }

        void Update()
        {
            if (GM == null || Cfg == null) return;

            hurtFlash = Mathf.MoveTowards(hurtFlash, 0f, Time.deltaTime * 3f);
            AttackTimer = Mathf.MoveTowards(AttackTimer, 0f, Time.deltaTime);
            Timer = Mathf.MoveTowards(Timer, 0f, Time.deltaTime);

            if (GM.State != GameState.Playing) { Apply(); UpdateCarryIcon(); return; }

            if (Captured)
            {
                if (Captor == null || !Captor.Alive) { Release(); }
                else
                {
                    X = Captor.X;
                    Height = 0.45f;
                    transform.rotation = Quaternion.Euler(0f, 0f, 160f);
                    Apply();
                    UpdateCarryIcon();
                    return;
                }
            }

            if (behavior != null) behavior.Tick();
            Height = Mathf.MoveTowards(Height, TargetHeight, 5f * Time.deltaTime);
            ClampToWorld(1f);
            Apply();
            UpdateCarryIcon();
        }

        void UpdateCarryIcon()
        {
            if (CarriedCoins > 0)
            {
                if (carryIcon == null)
                    carryIcon = Util.Spawn(transform, Art.Coin, new Vector3(0f, 0.82f, 0f), Layer.Villager + 2, "carry");
                carryIcon.enabled = true;
                carryIcon.transform.localPosition = new Vector3(0f, 0.82f + Mathf.Sin(Time.time * 6f) * 0.03f, 0f);
            }
            else if (carryIcon != null) carryIcon.enabled = false;
        }

        // --------------------------------------------------------- yardımcılar

        public bool GoTo(float targetX, float speed, float tol = 0.25f) { return StepTo(targetX, speed, tol); }
        public void Idle() { animTime = 0f; }
        public void Face(float dir) { SetFacing(dir); }

        public void AddCarried(int amount)
        {
            CarriedCoins += amount;
        }

        /// <summary>Taşıdığı altınları üsse bırakır (kral oradan toplar).</summary>
        public void DropCarriedAtBase()
        {
            if (CarriedCoins <= 0) return;
            int n = CarriedCoins;
            CarriedCoins = 0;
            for (int i = 0; i < n; i++)
                CoinPickup.Spawn(GM.WorldRoot, GM.BaseX + Util.Rand(-1.6f, 1.6f), 1, Util.Rand(2f, 4f));
            Sfx.Play("coin", 0.35f);
        }

        public void Hurt(float dmg)
        {
            Hp -= dmg;
            hurtFlash = 1f;
            Fx.Burst(Center, Pal.Cape, 4, 2f, 0.6f);
            if (Hp <= 0f) Die();
        }

        public void Die()
        {
            Fx.Burst(Center, Pal.Cape, 10, 3f);
            if (CarriedCoins > 0)
                for (int i = 0; i < CarriedCoins; i++) CoinPickup.Spawn(GM.WorldRoot, X + Util.Rand(-0.5f, 0.5f), 1, 3f);
            Remove();
        }

        public void Remove()
        {
            LeavePost();
            LeaveFarm();
            if (GM != null) GM.Villagers.Remove(this);
            if (Captor != null) Captor.Captive = null;
            Destroy(gameObject);
        }

        public void Capture(Greed g)
        {
            if (Captured) return;
            Captured = true;
            Captor = g;
            LeavePost();
            LeaveFarm();
            Height = 0.45f;
            Sfx.Play("hurt", 0.6f);
        }

        public void Release()
        {
            Captured = false;
            Captor = null;
            Height = 0f;
            TargetHeight = 0f;
            transform.rotation = Quaternion.identity;
        }

        public void LeavePost()
        {
            if (Post != null) Post.ReleasePost(PostIndex, this);
            Post = null;
            PostIndex = -1;
            TargetHeight = 0f;
        }

        /// <summary>Tezgahtan alet almaya yollar; varınca rolü değişir.</summary>
        public void SendToStand(float standX, VillagerRole role)
        {
            HasPendingTool = true;
            PendingStandX = standX;
            PendingRole = role;
        }

        public void LeaveFarm()
        {
            if (WorkFarm != null) WorkFarm.ReleaseWorker(this);
            WorkFarm = null;
        }

        public float WalkSpeed { get { return Cfg.villagerSpeed; } }
        public float RunSpeed { get { return Cfg.villagerRunSpeed; } }
    }
}
