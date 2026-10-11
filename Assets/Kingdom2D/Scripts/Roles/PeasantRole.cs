namespace Kingdom2D
{
    /// <summary>Boş köylü: altın toplar, üs çevresinde dolaşır, gece saklanır.</summary>
    public class PeasantRole : RoleBehavior
    {
        public override void Tick()
        {
            if (v.HasPendingTool)
            {
                if (v.GoTo(v.PendingStandX, v.RunSpeed, 0.4f))
                {
                    var role = v.PendingRole;
                    v.HasPendingTool = false;
                    v.SetRole(role);
                }
                return;
            }

            if (Dangerous)
            {
                if (FleeIfThreatened(9f)) return;
                if (DeliverCoinsIfCarrying()) return;
                Wander(GM.BaseX, 3.5f, v.WalkSpeed);
                return;
            }

            if (DeliverCoinsIfCarrying()) return;
            if (CollectLooseCoin(34f)) return;
            Wander(GM.BaseX, 7f, v.WalkSpeed);
        }
    }
}
