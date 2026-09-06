using BlueMarble;

namespace Combat
{
    public class CombatFlow
    {
        private TerritoryManager territoryManager => TerritoryManager.Instance;
        
        
        // 통행료를 내려면,
        public bool CanBattleRequest(string attackerId, string defenderId, Tile tile)
        {
            //territoryManager.PayToll(attackerId, tile);
            
            
            return true;
        }
    }
}