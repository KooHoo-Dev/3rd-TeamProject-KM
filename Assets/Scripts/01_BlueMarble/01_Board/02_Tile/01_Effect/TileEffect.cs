using Cysharp.Threading.Tasks;
using Epic3;

namespace BlueMarble
{
    public interface ITileEffect
    {
        UniTask Apply(string playerId, Tile tile);
    }

    public class PropertyEffect : ITileEffect //
    {
        public async UniTask Apply(string playerId, Tile tile)
        {
            PropertyManager.Instance.RequestPurchaseTile(playerId, tile);
            await UniTask.CompletedTask;
        }
    }

    public class SalaryEffect : ITileEffect //
    {
        public async UniTask Apply(string playerId, Tile tile)
        {
            await UniTask.CompletedTask;
        }   
    }

    public class PrisonEffect : ITileEffect //
    {
        public async UniTask Apply(string playerId, Tile tile)
        {
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
        public async UniTask Apply(string playerId, Tile tile)
        {
            await UniTask.CompletedTask;
        }
    }

    public class ShopEffect : ITileEffect //
    {
        public async UniTask Apply(string playerId, Tile tile)
        {
            // ShopManager shop = ShopManager.Instance;
            // await shop.OpenAsync();
            await UniTask.CompletedTask;
        }
    }
}