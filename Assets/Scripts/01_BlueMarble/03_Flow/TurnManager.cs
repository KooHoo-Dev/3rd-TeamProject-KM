using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

namespace BlueMarble
{
    public class TurnManager : MonoBehaviour
    {
        [field: Title("States")]
        [field: ShowInInspector, ReadOnly] public bool CanRoll { get; private set; }

        public event Action OnDiceRollRequested;
        
        public void SetInputEnabled(bool enabled) => CanRoll = enabled;

        private void Update()
        {
            if (Key.Digit1.GetKeyDown())
                RequestRollDice(); // 테스트용. 멀티플레이 씬에서는 인스펙터 접근이 안되서...
        }

        [Button]
        public void RequestRollDice()
        {
            if (CanRoll == false) return;

            CanRoll = false;
            OnDiceRollRequested?.Invoke();
        }

        public async UniTask ShowDiceResultAsync(string playerId, int d1, int d2)
        {
            CanRoll = false;
            
            IPlayer player = PlayerRegistry.GetPlayer(playerId);
            if (player == null) throw new Exception($"Player not found: {playerId}");
            
            // 여기에서 주사위 연출 넣으면 됨
            // EX) await DiceUI.ShowAsync(d1, d2);

            int totalSteps = d1 + d2;
            
            for (int i = 0; i < totalSteps; i++)
                await player.MoveNext();
        }
    }
}