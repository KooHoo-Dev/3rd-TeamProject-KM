using UnityEngine;

namespace BlueMarble
{
    public class MockShopOpener : IShopOpener
    {
        public void OpenShop(string playerId)
        {
            Debug.Log($"[MockShopOpener] {playerId}: 상점 UI 열림 (임시 로그)");
        }
    }
}