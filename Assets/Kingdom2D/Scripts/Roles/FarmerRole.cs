using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Çiftçi: tarlada çalışır, hasadı altın olarak üsse taşır.</summary>
    public class FarmerRole : RoleBehavior
    {
        public override void Tick()
        {
            if (Dangerous)
            {
                if (FleeIfThreatened(8f)) return;
                if (DeliverCoinsIfCarrying()) return;
                Wander(GM.BaseX, 3.5f, v.WalkSpeed);
                return;
            }

            if (DeliverCoinsIfCarrying()) return;

            if (v.WorkFarm == null)
            {
                var farm = GM.FindFarmWithSlot(v.X);
                if (farm != null) farm.AssignWorker(v);
            }

            if (v.WorkFarm == null)
            {
                if (CollectLooseCoin(20f)) return;
                Wander(GM.BaseX, 6f, v.WalkSpeed);
                return;
            }

            float spot = v.WorkFarm.WorkSpot(v);
            if (v.GoTo(spot, v.WalkSpeed, 0.3f))
            {
                v.Idle();
                v.Face(v.WorkFarm.X - v.X);
                v.WorkFarm.Work(Time.deltaTime, v);
            }
        }
    }
}
