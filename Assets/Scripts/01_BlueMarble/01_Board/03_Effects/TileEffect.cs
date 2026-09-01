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
        public async UniTask Apply(string playerId, Tile tile)
        {
            _ = PropertyManager.Instance.RequestPurchase(playerId, tile);
            await UniTask.CompletedTask;
        }
    }

    public class SalaryEffect : ITileEffect
    {
        public async UniTask Apply(string playerId, Tile tile)
        {
            // 이건 EconomyManager 들어오면 Config값 찾아와서 그 만큼 Add처리 하면 될듯?
            await UniTask.CompletedTask;
        }   
    }

    public class PrisonEffect : ITileEffect
    {
        public async UniTask Apply(string playerId, Tile tile)
        {
            // 지금 구조로는...
            IPlayer target = PlayerRegistry.GetPlayer(playerId);
            target.ActionDisabledTurnCount = 1; // 나중엔 Config에 등록된 값을 써야할듯?
            
            await UniTask.CompletedTask;
        }  
    }

    public class TreasureEffect : ITileEffect 
    {
        public async UniTask Apply(string playerId, Tile tile)
        {
            await UniTask.CompletedTask;
        } 
    }

    public class GoldCardEffect : ITileEffect //
    {
        public async UniTask Apply(string playerId, Tile tile)
        {
            await UniTask.CompletedTask;
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
            ShopManager shop = ShopManager.Instance;
            await shop.OpenAsync(playerId);
        }
    }
}