using Cysharp.Threading.Tasks;
using Jay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BlueMarble
{
    public class ShopManager : SingletonBase<ShopManager>
    {
        [Title("References")]
        [SerializeField] private ShopUIController ui;

        public async UniTask OpenAsync(string playerId)
        {
            await ui.OpenShopAsync(playerId);
        }
    }
}