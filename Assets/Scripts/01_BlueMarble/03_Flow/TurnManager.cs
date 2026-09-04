using System;
using System.Threading;
using BlueMarble.Server;
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
        
        private RoomClient Client => RoomClient.Instance;
        
        private DiceManager Dice => DiceManager.Instance;

        private void Awake()
        {
            Dice.OnRollButtonClicked += RequestRollDice;
        }

        private void OnDisable()
        {
            Dice.OnRollButtonClicked -= RequestRollDice;
        }
        
        private void RequestRollDice()
        {
            if (CanRoll == false) return;

            CanRoll = false;
            Dice.SetRollButtonActive(false);
            
            OnDiceRollRequested?.Invoke();
        }

        public void SetInputEnabled(bool enabled)
        {
            CanRoll = enabled;
            Dice.SetRollButtonActive(enabled);
        }

        public void BeginTurn(bool isMyTurn)
        {
            Debug.Log($"[TurnManager] BeginTurn. IsMyTurn: {isMyTurn}");
            
            CanRoll = isMyTurn;
            Dice.ShowForTurn(isMyTurn);
        }

        public async UniTask ShowDiceResultAsync(int d1, int d2)
        {
            CanRoll = false;
            Dice.SetRollButtonActive(false);

            await Dice.RollAsync(d1, d2);
        }
    }
}