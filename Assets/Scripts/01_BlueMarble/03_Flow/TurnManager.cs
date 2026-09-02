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
        
        public async UniTask MovePlayerAsync(string playerId, int steps)
        {
            if (PlayerRegistry.TryGetPlayer(playerId, out IPlayer player) == false)
            {
                Debug.LogError($"[TurnManager] Player not found: {playerId}");
                return;
            }
            
            bool isMe = Client.Me.Id == playerId;
            
            for (int i = 0; i < steps; i++)
            {
                Tile reached = await player.MoveNext();
                
                if (isMe == false) continue; // 타일 효과는 '자신'일 경우만 발동

                bool isActiveEffect = 
                    i == steps - 1 ||                // 도착 타일이거나
                    reached.SpaceDef.IsActiveOnPass; // 통과 시에도 효과를 발동해야하거나

                if (isActiveEffect) await reached.ExecuteEffect(playerId);
            }
        }
    }
}