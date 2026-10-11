using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Yürüyen her şeyin ortak temeli. Dünya tek boyutlu bir hat olduğu için
    /// konum sadece X ile tutulur; Y zeminden hesaplanır (fizik motoru kullanılmaz).
    /// </summary>
    public abstract class Actor : MonoBehaviour
    {
        protected SpriteRenderer sr;

        public float X;
        public float Height;                 // zeminden yükseklik (uçanlar, duvar üstü)
        protected float facing = 1f;
        protected float animTime;
        protected int frame;
        protected float bobPixels = 1f;
        protected float animStep = 0.3f;

        public SpriteRenderer Rend { get { return sr; } }
        public float Facing { get { return facing; } }

        protected GameManager GM { get { return GameManager.I; } }
        protected GameConfig Cfg { get { return GameManager.I != null ? GameManager.I.Config : null; } }

        protected virtual void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        }

        public void SetFacing(float dir)
        {
            if (Mathf.Abs(dir) > 0.01f) facing = dir > 0f ? 1f : -1f;
        }

        /// <summary>Verilen yönde yürür ve yürüyüş animasyonunu ilerletir.</summary>
        protected void Step(float dir, float speed)
        {
            if (Mathf.Abs(dir) > 0.01f)
            {
                float dx = (dir > 0f ? 1f : -1f) * speed * Time.deltaTime;
                X += dx;
                SetFacing(dir);
                animTime += Mathf.Abs(dx);
            }
            else
            {
                animTime = 0f;
            }
        }

        /// <summary>Hedefe doğru yürür; vardıysa true döner.</summary>
        protected bool StepTo(float targetX, float speed, float tolerance = 0.2f)
        {
            float d = targetX - X;
            if (Mathf.Abs(d) <= tolerance) { animTime = 0f; return true; }
            float dx = (d > 0f ? 1f : -1f) * speed * Time.deltaTime;
            if (Mathf.Abs(dx) >= Mathf.Abs(d)) X = targetX;
            else X += dx;
            SetFacing(d);
            animTime += Mathf.Abs(dx);
            return Mathf.Abs(targetX - X) <= tolerance;
        }

        /// <summary>Alt sınıflar o anki kareye uygun sprite'ı burada seçer.</summary>
        protected virtual void UpdateSprite() { }

        protected virtual void Apply()
        {
            if (sr == null) return;   // nesne yok edildiyse dokunma
            frame = animTime < 0.01f ? 0 : (int)(animTime / animStep) % 2;
            UpdateSprite();
            float bob = frame == 1 ? bobPixels / Pix.Ppu : 0f;
            transform.position = new Vector3(X, GroundLevel + Height + bob, 0f);
            if (sr != null) sr.flipX = facing < 0f;
        }

        public float GroundLevel { get { return Cfg != null ? Cfg.groundY : 0f; } }

        public float Dist(float x) { return Mathf.Abs(X - x); }

        public Vector2 Center
        {
            get
            {
                float h = sr != null && sr.sprite != null ? sr.sprite.bounds.size.y * 0.5f : 0.3f;
                return new Vector2(X, GroundLevel + Height + h);
            }
        }

        protected void ClampToWorld(float margin = 2f)
        {
            if (Cfg == null) return;
            X = Mathf.Clamp(X, -Cfg.worldHalfWidth + margin, Cfg.worldHalfWidth - margin);
        }
    }
}
