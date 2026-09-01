using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BlueMarble
{
    /// <summary>
    /// 테스트/개발용 Mock. 실제 ShopUIController가 준비되기 전까지 사용한다.
    /// 지금은 로그만 남기고 즉시 완료 처리하지만,
    /// 실제 구현(ShopUIController)에서는 "상점 닫기" 버튼을 눌렀을 때
    /// 이 UniTask가 완료되도록 만들어야 한다 (예: UniTaskCompletionSource 사용).
    /// </summary>
    public class MockShopOpener : IShopOpener
    {
        public async UniTask OpenShopAsync(string playerId)
        {
            Debug.Log($"[MockShopOpener] {playerId}: 상점 UI 열림 (임시로 즉시 닫힘 처리)");
            await UniTask.CompletedTask;
        }
    }
}