using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Epic2
{
    // 이거는 방장만..?
    public class TurnManager : MonoBehaviour
    {
        public List<int> players = new List<int>();

        public const int MAX_ROUND = 20;
        public int roundCount = 1;
        public int currentPlayerIndex = 0;
        
        private bool isGameEnded = false;
        private float waitSeconds = 30f;
        
        public CancellationTokenSource token;
        
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
                IsGameEnded();
                Debug.Log($"라운드 증가: {roundCount} 라운드 시작");
            }
            
            Debug.Log($"현재 순서: {players[currentPlayerIndex]}");
        }
        
        

        private bool IsGameEnded()
        {
            return roundCount > MAX_ROUND ? isGameEnded =  true : isGameEnded = false;
        }

        // 생각해보니까 이거는 같은 결과 값을 공유해야 함.
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
