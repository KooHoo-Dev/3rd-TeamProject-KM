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

    public class PropertyEffect : ITileEffect // 오늘 해야함
    {
        public UniTask Apply(string playerId, Tile tile)
        {
            PropertyManager property = PropertyManager.Instance;

            // Debug.Log($"[PropertyEffect] Applying effect to {playerId} on {tile.SpaceDef.Name}");
            // return property.RequestPurchase(playerId, tile);

            return UniTask.CompletedTask;
        }
    }

    public class SalaryEffect : ITileEffect // DONE
    {
        public UniTask Apply(string playerId, Tile _)
        {
            return EconomyManager.Instance.GainGoldTo(playerId, 10000);
        }   
    }

    public class PrisonEffect : ITileEffect // DONE
    {
        public UniTask Apply(string playerId, Tile tile)
        {
            return RoomClient.Instance.RequestAddIncapacitationCount(3);
        }  
    }

    public class TreasureEffect : ITileEffect // 서버랑 연동해야함
    {
        public async UniTask Apply(string playerId, Tile tile)
        {
            Debug.Log($"[TreasureEffect] Adding 1000 to {playerId}'s treasure");

            TreasureManager treasure = TreasureManager.Instance;
            await treasure.GrantAsync(playerId);
        }
    }

    public class GoldCardEffect : ITileEffect // 살짝 수정해야함 (장비 획득 관련)
    {
        public async UniTask Apply(string playerId, Tile tile)
        {
            GoldCardManager goldCard = GoldCardManager.Instance;
            await goldCard.DrawAndUseAsync(playerId);
        }
    }

    public class TeleportEffect : ITileEffect // DONE
    {
        public async UniTask Apply(string playerId, Tile _)
        {
            TeleportManager teleport = TeleportManager.Instance;
            await teleport.MoveToSelected(playerId);
        }
    }

    public class ShopEffect : ITileEffect // 살짝 수정해야함
    {
        public async UniTask Apply(string playerId, Tile _)
        {
            ShopManager shop = ShopManager.Instance;
            await shop.OpenAsync(playerId);
        }
    }
}