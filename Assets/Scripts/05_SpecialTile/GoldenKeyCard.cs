namespace BlueMarble
{
    public enum GoldenKeyEffectType
    {
        GoldChange,   // 자금 획득/손실
        GrantItem,    // 장비 획득 (지금은 로그만)
        ForceMove     // 강제 이동
    }

    public class GoldenKeyCard
    {
        public GoldenKeyEffectType EffectType { get; }
        public int GoldAmount { get; }
        public int TargetTileIndex { get; }
        public string ItemName { get; }

        public GoldenKeyCard(GoldenKeyEffectType effectType, int goldAmount = 0, int targetTileIndex = -1, string itemName = null)
        {
            EffectType = effectType;
            GoldAmount = goldAmount;
            TargetTileIndex = targetTileIndex;
            ItemName = itemName;
        }
    }
}