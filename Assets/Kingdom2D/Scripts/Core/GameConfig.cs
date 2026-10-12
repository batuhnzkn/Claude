using UnityEngine;

namespace Kingdom2D
{
    /// <summary>
    /// Oyunun tüm dengeleme değerleri. GameManager üzerinde Inspector'dan
    /// değiştirilebilir (oyun çalışırken de).
    /// </summary>
    [System.Serializable]
    public class GameConfig
    {
        [Header("Dünya")]
        public float worldHalfWidth = 110f;
        public float groundY = 0f;

        [Header("Kral")]
        public float kingSpeed = 7f;
        public int purseBase = 10;
        public int purseBonusPerTier = 8;
        public float coinDropInterval = 0.2f;
        public float pickupRange = 1.2f;
        public float payRange = 1.8f;
        public int startCoins = 6;

        [Header("Gün döngüsü (saniye)")]
        public float dawnLength = 6f;
        public float dayLength = 50f;
        public float duskLength = 9f;
        public float nightLength = 38f;
        public int bloodMoonEvery = 5;

        [Header("Köylüler")]
        public float villagerSpeed = 3.2f;
        public float villagerRunSpeed = 5.2f;
        public float archerRange = 9f;
        public float archerFireInterval = 1.1f;
        public float arrowSpeed = 16f;
        public float arrowDamage = 2.5f;
        public float builderWorkRate = 1.2f;
        public float farmCycle = 16f;
        public int farmYield = 2;
        public float knightRange = 1.3f;
        public float knightDamage = 3f;
        public float knightAttackInterval = 0.8f;

        [Header("Maliyetler")]
        public int costRecruit = 1;
        public int costBow = 2;
        public int costHammer = 2;
        public int costScythe = 3;
        public int costSword = 5;
        public int[] costWall = { 4, 9, 18 };
        public int[] costCamp = { 14, 34 };
        public int costFarmPlot = 4;

        [Header("Greed (düşmanlar)")]
        public float greedBaseSpeed = 3.2f;
        public float waveBase = 2.5f;
        public float waveGrowth = 1.2f;
        public float bloodMoonMultiplier = 1.8f;
        public int greedCoinSteal = 3;
        public float greedAttackInterval = 0.85f;

        [Header("Doğa")]
        public int critterCount = 15;
        public int treeCount = 46;

        [Header("Kamera")]
        public float cameraSize = 4.9f;
        public float cameraHeight = 2.85f;
        public float cameraLerp = 4.5f;
        public float cameraLookAhead = 1.6f;

        public int CostWall(int tier)
        {
            if (costWall == null || costWall.Length == 0) return 999;
            return costWall[Mathf.Clamp(tier - 1, 0, costWall.Length - 1)];
        }

        public int CostCamp(int tier)
        {
            if (costCamp == null || costCamp.Length == 0) return 999;
            return costCamp[Mathf.Clamp(tier - 2, 0, costCamp.Length - 1)];
        }

        public int CostStand(StandKind kind)
        {
            switch (kind)
            {
                case StandKind.Bow: return costBow;
                case StandKind.Hammer: return costHammer;
                case StandKind.Scythe: return costScythe;
                default: return costSword;
            }
        }

        public float CycleLength { get { return dawnLength + dayLength + duskLength + nightLength; } }
    }
}
