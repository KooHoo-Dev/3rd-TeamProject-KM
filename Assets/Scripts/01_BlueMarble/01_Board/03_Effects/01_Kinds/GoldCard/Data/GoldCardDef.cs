using UnityEngine;

namespace BlueMarble
{
    public enum GoldCardPatternType
    {
        GainGold,
        LoseGold,
        GainTreasure,
        LoseTreasure,
        GoTo,
    }

    public enum GoldCardScopeType
    {
        Self,
        All,
        ExceptSelf,
        Random,
    }
    
    public class GoldCardDef
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string FlavorText { get; private set; }
        public Sprite Icon { get; private set; }
        
        public GoldCardPatternType PatternType { get; private set; }
        public GoldCardScopeType ScopeType { get; private set; }
        
        public int GoldAmount { get; private set; }
        public int DropTableId { get; private set; } // 이건 나중에 수정
        public int SpaceId { get; private set; }

        public GoldCardDef(int id, string name, string description, string flavorText, Sprite icon, 
            GoldCardPatternType patternType, GoldCardScopeType scopeType, int goldAmount, int dropTableId, int spaceId)
        {
            Id = id;
            Name = name;
            Description = description;
            FlavorText = flavorText;
            Icon = icon;
            
            PatternType = patternType;
            ScopeType = scopeType;
            
            GoldAmount = goldAmount;
            DropTableId = dropTableId;
            SpaceId = spaceId;
        }
    }
}