namespace BlueMarble
{
    public interface IBoardTeleporter
    {
        /// <summary>
        /// 플레이어를 targetTileIndex 위치로 즉시 이동시킨다.
        /// 반환값: 이동 중 출발지를 지나가거나 정확히 도착했는지 여부.
        /// </summary>
        bool TeleportTo(string playerId, int targetTileIndex);

        int GetCurrentTileIndex(string playerId);
    }
}