using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Jay;
using Sirenix.OdinInspector;
using UnityEngine;
using Utility;

namespace BlueMarble
{
    public class RoundManager : SingletonBase<RoundManager>
    {
        [Title("Components")]
        [SerializeField] private TurnManager turn;
        
        [Title("Settings")]
        [SerializeField] private int maxRoundCount = 20;

        [Title("Player List")]
        [ShowInInspector, ReadOnly] private List<string> players;
        
        [field: Title("States")]
        [field: ShowInInspector, ReadOnly] public int RoundCount { get; private set; }
        [field: ShowInInspector, ReadOnly] public int CurrentPlayerIndex { get; private set; }

        public event Action OnRoundEnded;
        
        public void Initialize(List<string> joinedPlayers)
        {
            RoundCount = 1;
            CurrentPlayerIndex = 0;
            
            players = joinedPlayers.Shuffle();
        }

        public async UniTask ExecuteRound()
        {
            while (true)
            {
                if (IsGameEnded()) break;

                for (int i = 0; i < players.Count; i++)
                {
                    CurrentPlayerIndex = i;
                    
                    string player = players[i];
                    await turn.ExecuteTurn(player);
                }

                CleanupEliminatedPlayers();
                AdvanceRound();
                
                OnRoundEnded?.Invoke();
            }
        }

        private void AdvanceRound()
        {
            RoundCount++;
        }

        private void CleanupEliminatedPlayers()
        {
            // 검사 함수를 넣어서, 서버한테 살아있는 플레이어 리스트를 받아와서
            // 해당 리스트에 없는 플레이어는 players에서 remove해버림
        }

        private bool IsGameEnded()
        {
            return RoundCount > maxRoundCount;
        }
    }
}