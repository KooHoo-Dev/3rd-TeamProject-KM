using BlueMarble.Server;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Epic3;

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
            PropertyManager property = PropertyManager.Instance;

            // Debug.Log($"[PropertyEffect] Applying effect to {playerId} on {tile.SpaceDef.Name}");
            // return property.RequestPurchase(playerId, tile);

            return UniTask.CompletedTask;
        }
    }

    public class SalaryEffect : ITileEffect
    {
        public UniTask Apply(string playerId, Tile _)
        {
            EconomyManager.Instance.GainGoldTo(playerId, 10000);
            return UniTask.CompletedTask;
        }   
    }

    public class PrisonEffect : ITileEffect
    {
        public UniTask Apply(string playerId, Tile tile)
        {
            RoomClient.Instance.RequestAddIncapacitationCount(3);
            
            Debug.Log($"[PrisonEffect] {playerId} is now in prison. " +
                      $"That player cannot take any action for 3 turn.");
            
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