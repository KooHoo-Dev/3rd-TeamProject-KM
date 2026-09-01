namespace BlueMarble
{
    public interface IGold
    {
        int CurrentGold { get; }

        bool TrySpend(int amount);

        /// <summary>
        /// 골드를 지급한다. (출발지 급여, 황금 열쇠 자금 획득 등)
        /// </summary>
        void AddGold(int amount);
    }
}