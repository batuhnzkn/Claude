using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// İnşa edilebilen / hasar alabilen yapıların ortak temeli.
    /// Ödeme yapılınca "şantiye" olur, inşaatçılar tamamlar.
    /// </summary>
    public abstract class Structure : MonoBehaviour
    {
        public float X;
        public int Tier;
        public float Hp;
        public float MaxHp = 10f;
        public float WorkOffset = 1.4f;
        public float HalfWidth = 1f;
        public float BuildTime = 5f;

        public bool UnderConstruction;
        public float BuildProgress;
        protected int pendingTier;

        protected SpriteRenderer sr;
        float shake;

        protected GameManager GM { get { return GameManager.I; } }
        protected GameConfig Cfg { get { return GameManager.I != null ? GameManager.I.Config : null; } }

        public virtual bool NeedsWork { get { return UnderConstruction || (Tier > 0 && Hp < MaxHp - 0.05f); } }

        protected void Register()
        {
            if (GameManager.I != null) GameManager.I.Structures.Add(this);
        }

        protected virtual void OnDestroy()
        {
            if (GameManager.I != null) GameManager.I.Structures.Remove(this);
        }

        /// <summary>İnşaatçı emeği.</summary>
        public virtual void Work(float amount)
        {
            if (UnderConstruction)
            {
                BuildProgress += amount / Mathf.Max(0.5f, BuildTime);
                if (Util.Chance(Time.deltaTime * 4f))
                {
                    Fx.Burst(new Vector2(X, GroundY + 0.5f), Pal.Wood, 1, 1.6f, 0.6f);
                    Sfx.Play("build", 0.22f);
                }
                if (BuildProgress >= 1f) FinishBuild();
            }
            else if (Hp < MaxHp)
            {
                Hp = Mathf.Min(MaxHp, Hp + amount * MaxHp * 0.12f);
                if (Util.Chance(Time.deltaTime * 3f)) Fx.Burst(new Vector2(X, GroundY + 0.6f), Pal.Wood, 1, 1.4f, 0.5f);
                UpdateVisual();
            }
        }

        public float GroundY { get { return Cfg != null ? Cfg.groundY : 0f; } }

        protected void BeginBuild(int targetTier)
        {
            pendingTier = targetTier;
            UnderConstruction = true;
            BuildProgress = 0f;
            OnConstructionStarted();
        }

        protected virtual void OnConstructionStarted() { }

        protected virtual void FinishBuild()
        {
            UnderConstruction = false;
            BuildProgress = 0f;
            Tier = pendingTier;
            Hp = MaxHp = HpForTier(Tier);
            Sfx.Play("upgrade", 0.7f);
            Fx.Burst(new Vector2(X, GroundY + 0.8f), Pal.Gold, 12, 3f);
            UpdateVisual();
            OnTierChanged();
        }

        protected virtual void OnTierChanged() { }

        protected virtual float HpForTier(int tier) { return 10f * tier; }

        public virtual void Damage(float dmg)
        {
            if (Tier <= 0 && !UnderConstruction) return;
            Hp -= dmg;
            shake = 0.12f;
            if (Hp <= 0f) Collapse();
            UpdateVisual();
        }

        protected virtual void Collapse() { }

        protected abstract void UpdateVisual();

        protected virtual void Update()
        {
            if (sr == null) return;
            if (shake > 0f)
            {
                shake -= Time.deltaTime;
                sr.transform.localPosition = new Vector3(Util.Rand(-0.06f, 0.06f), 0f, 0f);
            }
            else
            {
                sr.transform.localPosition = Vector3.zero;
            }
        }
    }
}
