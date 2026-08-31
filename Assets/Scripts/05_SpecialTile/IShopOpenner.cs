using Cysharp.Threading.Tasks;

namespace BlueMarble
{
    /// <summary>
    /// 상점 UI를 여는 트리거. 실제 구현은 E10(UI)의 ShopUIController가 담당할 예정.
    /// 반환하는 UniTask는 "상점이 닫힐 때" 완료되어야 한다.
    /// 이렇게 해야 특수 타일을 지나칠 때도, 상점이 닫힐 때까지 보드 이동이 실제로 멈춰서 기다릴 수 있다.
    /// </summary>
    public interface IShopOpener
    {
        UniTask OpenShopAsync(string playerId);
    }
}