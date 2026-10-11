using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Altın atılabilen nokta (duvar, tezgah, serseri kampı, üs yükseltmesi...).
    /// Kral yaklaşıp ödeme tuşunu basılı tutarak doldurur.
    /// </summary>
    public class CoinSink : MonoBehaviour
    {
        public int Cost = 1;
        public int Paid;
        public bool Active = true;
        public bool Repeating;              // dolunca sıfırlanıp tekrar kullanılabilir mi
        public float PromptHeight = 1.4f;
        public System.Action OnComplete;

        SpriteRenderer icon;
        SpriteRenderer label;
        SpriteRenderer barBack;
        SpriteRenderer barFill;
        float bob;
        float visible;

        public float X { get { return transform.position.x; } }
        public bool Full { get { return Paid >= Cost; } }
        public int Remaining { get { return Mathf.Max(0, Cost - Paid); } }

        void Awake()
        {
            if (GameManager.I != null) GameManager.I.Sinks.Add(this);
        }

        void OnDestroy()
        {
            if (GameManager.I != null) GameManager.I.Sinks.Remove(this);
        }

        public void Setup(int cost, float promptHeight, System.Action onComplete, bool repeating = false)
        {
            Cost = Mathf.Max(1, cost);
            Paid = 0;
            PromptHeight = promptHeight;
            OnComplete = onComplete;
            Repeating = repeating;
            Active = true;
            BuildPrompt();
        }

        void BuildPrompt()
        {
            if (icon != null) return;
            icon = Util.Spawn(transform, Art.Coin, new Vector3(-0.22f, PromptHeight, 0f), Layer.Prompt, "prompt_coin");
            label = Util.Spawn(transform, PixelFont.Make("0", Pal.White, 1), new Vector3(0.08f, PromptHeight, 0f), Layer.Prompt, "prompt_label");
            barBack = Util.Spawn(transform, Art.Solid, new Vector3(0f, PromptHeight - 0.3f, 0f), Layer.Prompt, "bar_back");
            barFill = Util.Spawn(transform, Art.Solid, new Vector3(0f, PromptHeight - 0.3f, 0f), Layer.Prompt + 1, "bar_fill");
            barBack.color = new Color(0f, 0f, 0f, 0.5f);
            barFill.color = Pal.Gold;
            SetScale(barBack, 0.9f, 0.09f);
            SetScale(barFill, 0f, 0.07f);
        }

        static void SetScale(SpriteRenderer sr, float w, float h)
        {
            Vector2 s = sr.sprite.bounds.size;
            sr.transform.localScale = new Vector3(w / s.x, h / s.y, 1f);
        }

        /// <summary>Bir altın yatırır. Tamamlandıysa true döner.</summary>
        public bool Pay(int amount)
        {
            if (!Active || Full) return false;
            Paid = Mathf.Min(Cost, Paid + amount);
            if (Full)
            {
                if (OnComplete != null) OnComplete();
                if (Repeating) Paid = 0;
                else Active = false;
                return true;
            }
            return false;
        }

        public void Cancel()
        {
            Active = false;
            Paid = 0;
        }

        void Update()
        {
            if (icon == null) return;

            var king = GameManager.I != null ? GameManager.I.King : null;
            float dist = king != null ? Mathf.Abs(king.X - X) : 99f;
            bool show = Active && GameManager.I != null && GameManager.I.State == GameState.Playing && dist < 7f;
            visible = Mathf.MoveTowards(visible, show ? 1f : 0f, Time.deltaTime * 5f);

            bool near = dist < (GameManager.I != null ? GameManager.I.Config.payRange : 1.8f) + 0.2f;
            float alpha = visible * (near ? 1f : 0.55f);

            bob = Mathf.Sin(Time.time * 3f) * 0.05f;
            icon.transform.localPosition = new Vector3(-0.22f, PromptHeight + bob, 0f);
            label.transform.localPosition = new Vector3(0.06f, PromptHeight + bob, 0f);

            label.sprite = PixelFont.Make(Remaining.ToString(), Pal.White, 1);
            icon.color = new Color(1f, 1f, 1f, alpha);
            label.color = new Color(1f, 1f, 1f, alpha);

            float p = Cost > 0 ? Paid / (float)Cost : 0f;
            SetScale(barFill, 0.88f * p, 0.07f);
            barFill.transform.localPosition = new Vector3(-0.44f + 0.44f * p, PromptHeight - 0.3f + bob, 0f);
            barBack.transform.localPosition = new Vector3(0f, PromptHeight - 0.3f + bob, 0f);
            barBack.color = new Color(0f, 0f, 0f, 0.45f * alpha);
            barFill.color = new Color(Pal.Gold.r, Pal.Gold.g, Pal.Gold.b, alpha);
            barBack.enabled = Paid > 0 && visible > 0.01f;
            barFill.enabled = Paid > 0 && visible > 0.01f;
            icon.enabled = visible > 0.01f;
            label.enabled = visible > 0.01f;
        }
    }
}
