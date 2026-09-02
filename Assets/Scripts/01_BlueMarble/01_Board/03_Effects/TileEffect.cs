using Cysharp.Threading.Tasks;
using Epic3;
using UnityEngine;

namespace BlueMarble
{
    public interface ITileEffect
    {
        UniTask Apply(string playerId, Tile tile);
    }

    public class PropertyEffect : ITileEffect
    {
        public UniTask Apply(string playerId, Tile tile)
        {
            _ = PropertyManager.Instance.RequestPurchase(playerId, tile);
            await UniTask.CompletedTask;
        }
    }

    public class SalaryEffect : ITileEffect
    {
        public UniTask Apply(string playerId, Tile tile)
        {
            // 이건 EconomyManager 들어오면 Config값 찾아와서 그 만큼 Add처리 하면 될듯?
            Debug.Log($"[SalaryEffect] Adding 1000 to {playerId}'s salary");
            return UniTask.CompletedTask;
        }   
    }

    public class PrisonEffect : ITileEffect
    {
        public UniTask Apply(string playerId, Tile tile)
        {
            IPlayer target = PlayerRegistry.GetPlayer(playerId);
            target.ActionDisabledTurnCount = 1; // 나중엔 Config에 등록된 값을 써야할듯?
            
            Debug.Log($"[PrisonEffect] {playerId} is now in prison. " +
                      $"That player cannot take any action for {target.ActionDisabledTurnCount} turn.");
            
            return UniTask.CompletedTask;
        }  
    }

    public class TreasureEffect : ITileEffect 
    {
        public UniTask Apply(string playerId, Tile tile)
        {
            Debug.Log($"[TreasureEffect] Adding 1000 to {playerId}'s treasure");
            return UniTask.CompletedTask;
        } 
    }

    public class GoldCardEffect : ITileEffect //
    {
        public async UniTask Apply(string playerId, Tile tile)
        {
            GoldCardManager goldCard = GoldCardManager.Instance;
            await goldCard.DrawAndUseAsync(playerId);
        }
    }

    public class TeleportEffect : ITileEffect
    {
        public async UniTask Apply(string playerId, Tile _)
        {
            TeleportManager teleport = TeleportManager.Instance;
            await teleport.ExecuteAsync(playerId);
        }
    }

    public class ShopEffect : ITileEffect
    {
        public async UniTask Apply(string playerId, Tile _)
        {
            //ShopManager shop = ShopManager.Instance;
            //await shop.OpenAsync(playerId);
        }
    }
}