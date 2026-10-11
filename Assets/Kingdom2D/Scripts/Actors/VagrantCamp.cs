using System.Collections.Generic;
using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Çadırlı gezgin kampı: altın atıldıkça buradan yeni köylü gelir.</summary>
    public class VagrantCamp : MonoBehaviour
    {
        public float X;
        public int MaxVagrants = 6;

        readonly List<Vagrant> vagrants = new List<Vagrant>();
        CoinSink sink;
        SpriteRenderer fire;
        float fireAnim;

        public int Count { get { return vagrants.Count; } }

        public static VagrantCamp Create(Transform parent, float x, int startCount)
        {
            var go = Util.Node(parent, "VagrantCamp", new Vector3(x, 0f, 0f));
            var camp = go.AddComponent<VagrantCamp>();
            camp.X = x;

            Util.Spawn(go.transform, Art.Tent, Vector3.zero, Layer.StructureBack, "tent");
            camp.fire = Util.Spawn(go.transform, Art.Flame(0), new Vector3(1.4f, 0f, 0f), Layer.Decor, "fire");

            camp.sink = go.AddComponent<CoinSink>();
            camp.sink.Setup(GameManager.I.Config.costRecruit, 1.9f, camp.OnPaid, true);

            for (int i = 0; i < startCount; i++) camp.AddVagrant();
            if (GameManager.I != null) GameManager.I.VagrantCamps.Add(camp);
            return camp;
        }

        void AddVagrant()
        {
            if (vagrants.Count >= MaxVagrants) return;
            var v = Vagrant.Spawn(transform.parent, this, X + Util.Rand(-3f, 3f));
            vagrants.Add(v);
        }

        /// <summary>Her yeni günde kamp biraz kalabalıklaşır.</summary>
        public void Replenish()
        {
            Cleanup();
            AddVagrant();                                   // her sabah bir gezgin
            if (Util.Chance(0.6f)) AddVagrant();            // bazen ikinci
        }

        void Cleanup()
        {
            for (int i = vagrants.Count - 1; i >= 0; i--)
                if (vagrants[i] == null) vagrants.RemoveAt(i);
        }

        void OnPaid()
        {
            Cleanup();
            if (vagrants.Count == 0)
            {
                CoinPickup.Spawn(GameManager.I.WorldRoot, X, 1, 3f);
                return;
            }

            Vagrant best = vagrants[0];
            float bd = float.MaxValue;
            var king = GameManager.I.King;
            float refX = king != null ? king.X : X;
            for (int i = 0; i < vagrants.Count; i++)
            {
                float d = Mathf.Abs(vagrants[i].X - refX);
                if (d < bd) { bd = d; best = vagrants[i]; }
            }
            vagrants.Remove(best);
            best.Recruit();
        }

        void Update()
        {
            fireAnim += Time.deltaTime * 9f;
            if (fire != null) fire.sprite = Art.Flame(((int)fireAnim) % 3);
            Cleanup();
            if (sink != null) sink.Active = vagrants.Count > 0;
        }

        void OnDestroy()
        {
            if (GameManager.I != null) GameManager.I.VagrantCamps.Remove(this);
        }
    }
}
