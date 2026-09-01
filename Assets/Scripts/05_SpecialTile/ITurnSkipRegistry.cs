namespace BlueMarble
{
    public interface ITurnSkipRegistry
    {
        /// <summary>
        /// 해당 플레이어의 다음 턴을 스킵하도록 표시한다.
        /// </summary>
        void MarkSkipNextTurn(string playerId);

        /// <summary>
        /// 해당 플레이어가 이번 턴을 스킵해야 하는지 확인하고,
        /// 스킵 대상이면 상태를 소모(초기화)한다.
        /// </summary>
        bool ConsumeSkipIfNeeded(string playerId);
    }
}