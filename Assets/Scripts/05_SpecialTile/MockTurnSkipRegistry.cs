using System.Collections.Generic;
using UnityEngine;

namespace BlueMarble
{
    public class MockTurnSkipRegistry : ITurnSkipRegistry
    {
        private readonly HashSet<string> skipTargets = new();

        public void MarkSkipNextTurn(string playerId)
        {
            skipTargets.Add(playerId);
            Debug.Log($"[MockTurnSkipRegistry] {playerId}: 다음 턴 스킵 예약");
        }

        public bool ConsumeSkipIfNeeded(string playerId)
        {
            if (skipTargets.Contains(playerId) == false)
                return false;

            skipTargets.Remove(playerId);
            Debug.Log($"[MockTurnSkipRegistry] {playerId}: 턴 스킵 소모 (이번 턴 건너뜀)");
            return true;
        }
    }
}