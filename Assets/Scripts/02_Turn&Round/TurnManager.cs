using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Epic2
{
    public class TurnManager : MonoBehaviour
    {
        public List<int> players = new List<int>();

        public const int MAX_ROUND = 20;
        public int roundCount = 1;
        public int currentPlayerIndex = 0;
        
        private bool isGameEnded = false;
        // 테스트를 위해 public
        public float waitSeconds = 30f;
        
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
            // 1. 기존 타이머가 돌고 있다면 안전하게 취소하고 폐기해!
            if (token != null)
            {
                token.Cancel();
                token.Dispose();
                token = null; // 폐기 후 비워둠
            }

            currentPlayerIndex++;
        
            if (currentPlayerIndex >= players.Count)
            {
                currentPlayerIndex = 0;
                roundCount++;
                IsGameEnded();
                Debug.Log($"라운드 증가: {roundCount} 라운드 시작");
            }

            Debug.Log($"현재 순서: {players[currentPlayerIndex]}");
            
            // 2. 다음 사람 턴이 되었으니 새 타이머를 시작해!
            TurnDeadline().Forget();
        }

        // 이 함수는 30초의 제한시간이 있고, 30초가 지나면 
        public async UniTaskVoid TurnDeadline()
        {
            token =  new CancellationTokenSource();

            Debug.Log($"{waitSeconds}초 만큼 대기");
            try
            {
                await UniTask.Delay(Mathf.RoundToInt(waitSeconds * 1000f),
                    DelayType.DeltaTime,
                    cancellationToken: token.Token);
            }
            catch (Exception e)
            {
                return;
            }
            
            Debug.Log("대기 끝");
            AdvanceTurn();
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
