using System.Collections.Generic;
using UnityEngine;

namespace Epic2
{
    public class TurnManager : MonoBehaviour
    {
        public List<int> players = new List<int>();
        public int roundCount = 1;
    
        private int currentPlayerIndex = 0;

        // 게임 시작 시 서버에서 참가자 리스트를 받아 초기화할 때 호출
        public void InitializePlayers(List<int> joinedPlayers)
        {
            players = new List<int>(joinedPlayers);
        
            Debug.Log("바뀌기 전 순서");
            ShowSequence();
        
            Shuffle(players);
        
            Debug.Log("바뀐 후 순서");
            ShowSequence();
        }
    
        public int GetCurrentPlayerID()
        {
            if (players.Count == 0) return -1;
            return players[currentPlayerIndex];
        }

        public bool IsMyTurn(int myPlayerID)
        {
            return GetCurrentPlayerID() == myPlayerID;
        }

        // 누군가의 턴이 완전히 종료되었을 때 (이동, 정산 완료 후) 호출
        public void AdvanceTurn()
        {
            currentPlayerIndex++;
        
            // 모든 플레이어가 턴을 마쳤으면 라운드 증가
            if (currentPlayerIndex >= players.Count)
            {
                currentPlayerIndex = 0;
                roundCount++;
                Debug.Log($"라운드 증가: {roundCount} 라운드 시작");
            }
        }

        private static void Shuffle(List<int> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                int value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void ShowSequence()
        {
            foreach (int i in players)
            {
                Debug.Log($"Player ID: {i}");
            }
        }
    }
}
