using UnityEngine;

namespace BlueMarble
{
    public enum GoldCardType
    {
        GainGold,
        LoseGold,
        GainTreasure,
        LoseTreasure,
        GoTo,
    }

    public enum GoldCardScope
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
        
        public GoldCardType Type { get; private set; }
        public GoldCardScope Scope { get; private set; }
        
        public int GoldAmount { get; private set; }
        public int DropTableId { get; private set; } // 이건 나중에 수정
        public BoardSpaceDef SpaceDef { get; private set; }
        
        public GoldCardDef(int id, string name, string description, string flavorText, Sprite icon, 
            GoldCardType type, GoldCardScope scope, int goldAmount, int dropTableId, BoardSpaceDef spaceDef)
        {
            Id = id;
            Name = name;
            Description = description;
            FlavorText = flavorText;
            Icon = icon;
            
            Type = type;
            Scope = scope;
            
            GoldAmount = goldAmount;
            DropTableId = dropTableId;
            SpaceDef = spaceDef;
        }
    }
}