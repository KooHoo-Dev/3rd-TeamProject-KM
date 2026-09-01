using System.Collections.Generic;

namespace BlueMarble
{
    public static class GoldenKeyDeck
    {
        // 임시 카드 목록. 추후 TSV로 교체 예정.
        public static readonly List<GoldenKeyCard> Cards = new()
        {
            new GoldenKeyCard(GoldenKeyEffectType.GoldChange, goldAmount: 100),
            new GoldenKeyCard(GoldenKeyEffectType.GoldChange, goldAmount: -50),
            new GoldenKeyCard(GoldenKeyEffectType.GrantItem, itemName: "임시장비"),
            new GoldenKeyCard(GoldenKeyEffectType.ForceMove, targetTileIndex: 7),
            new GoldenKeyCard(GoldenKeyEffectType.ForceMove, targetTileIndex: 14),
        };
    }
}