namespace BlueMarble
{
    /// <summary>
    /// 상점 UI를 여는 트리거. 실제 구현은 E10(UI)의 ShopUIController가 담당할 예정.
    /// </summary>
    public interface IShopOpener
    {
        void OpenShop(string playerId);
    }
}