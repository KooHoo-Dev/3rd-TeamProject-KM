using System.Collections.Generic;
using UnityEngine;

namespace BlueMarble
{
    public class MockBoardTeleporter : IBoardTeleporter
    {
        private const int TILE_COUNT = 40;

        private readonly Dictionary<string, int> playerPosition = new();

        public int GetCurrentTileIndex(string playerId)
        {
            if (playerPosition.ContainsKey(playerId) == false)
                playerPosition[playerId] = 0;

            return playerPosition[playerId];
        }

        /// <summary>
        /// 목적지로 즉시 이동시키고, 이동 중 출발지를 지나갔는지(또는 정확히 도착했는지) 반환한다.
        /// 급여 지급 등 실제 효과 적용은 호출하는 쪽(SpecialTileService)의 책임이다.
        /// </summary>
        public bool TeleportTo(string playerId, int targetTileIndex)
        {
            int currentIndex = GetCurrentTileIndex(playerId);

            bool passesStart = DoesPassOrArriveStart(currentIndex, targetTileIndex);

            playerPosition[playerId] = targetTileIndex;
            Debug.Log($"[MockBoardTeleporter] {playerId}: {currentIndex} -> {targetTileIndex} 이동");

            return passesStart;
        }

        // 항상 정방향(인덱스 증가) 이동으로 가정.
        private bool DoesPassOrArriveStart(int currentIndex, int targetIndex)
        {
            if (targetIndex == 0) return true;
            if (targetIndex <= currentIndex) return true;
            return false;
        }
    }
}