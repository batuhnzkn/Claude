using UnityEngine;

namespace Kingdom2D
{
    /// <summary>İnşaatçı: ödenmiş yapıları kurar, hasarlı duvarları tamir eder.</summary>
    public class BuilderRole : RoleBehavior
    {
        public override void Tick()
        {
            bool night = GM.IsNight;

            if (Dangerous && FleeIfThreatened(6.5f)) return;
            if (DeliverCoinsIfCarrying()) return;

            var work = GM.FindWork(v.X, night);
            if (work != null)
            {
                float side = v.X < work.X ? -1f : 1f;
                float spot = work.X + side * work.WorkOffset;
                if (v.GoTo(spot, night ? v.RunSpeed : v.WalkSpeed, 0.3f))
                {
                    v.Idle();
                    v.Face(work.X - v.X);
                    work.Work(Cfg.builderWorkRate * Time.deltaTime);
                }
                return;
            }

            if (Dangerous) { Wander(GM.BaseX, 3.5f, v.WalkSpeed); return; }
            Wander(GM.BaseX, 6f, v.WalkSpeed);
        }
    }
}
