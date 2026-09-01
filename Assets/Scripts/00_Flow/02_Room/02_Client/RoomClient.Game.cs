using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BlueMarble.Server
{
    public partial class RoomClient
    {
        [field: Title("Game States")]
        [field: ShowInInspector, ReadOnly] private string[] playerIds;

        [field: PropertySpace]
        [field: ShowInInspector, ReadOnly] public int RoundCount { get; private set; }
        [field: ShowInInspector, ReadOnly] public int TurnId { get; private set; }
        [field: ShowInInspector, ReadOnly] public User CurrentTurnUser { get; private set; }

        public event Action OnGameStarted;

        public event Action<User> OnTurnStarted;
        public event Action<User, int, int> OnDiceRolled;

        public event Action OnGameEnded;
        
        public IReadOnlyList<string> PlayerIds => playerIds;

        private void RegisterGameHandlers()
        {
            RegisterHandler<GameStartedMessage>(ProtocolHeader.GAME_STARTED, HandleGameStarted);
            RegisterHandler<TurnStartedMessage>(ProtocolHeader.TURN_STARTED, HandleTurnStarted);
            RegisterHandler<DiceRolledMessage>(ProtocolHeader.DICE_ROLLED, HandleDiceRolled);
            RegisterHandler<GameEndedMessage>(ProtocolHeader.GAME_ENDED, HandleGameEnded);
        }
        
        #region SEND_TO_SERVER

        public UniTask AnnounceBoardReady()
        {
            return SendAsync(new SetBoardReadyMessage());
        }

        public UniTask AnnounceDiceAnimationEnded(int turnId)
        {
            return SendAsync(new DiceAnimationFinishedMessage { TurnId = turnId });
        }

        public void RequestRollDice(int turnId)
        {
            SendAsync(new RollDiceMessage { TurnId = turnId }).Forget();
        }
        
        #endregion
        
        #region HANDLE_RECEIVED_MESSAGE
        
        private void HandleGameStarted(GameStartedMessage message)
        {
            playerIds = message.MemberIds;
            
            OnGameStarted?.Invoke();
            Debug.Log($"[RoomClient] Game started. Players: {playerIds.Length}");
        }

        private void HandleTurnStarted(TurnStartedMessage message)
        {
            User user = GetUser(message.PlayerId);
            
            RoundCount = message.RoundCount;
            TurnId = message.TurnId;
            CurrentTurnUser = user;
            
            OnTurnStarted?.Invoke(user);
            Debug.Log($"[RoomClient] {user.Nickname}({user.Id}) turn started. Round: {RoundCount}, Turn Id: {TurnId}");
        }

        private void HandleDiceRolled(DiceRolledMessage message)
        {
            if (message.TurnId != TurnId) return;
            
            User user = GetUser(message.PlayerId);
            
            OnDiceRolled?.Invoke(user, message.D1, message.D2);
            Debug.Log($"[RoomClient] {user.Nickname}({user.Id}) rolled dice: {message.D1}, {message.D2}. " +
                      $"Turn Id: {message.TurnId}");
        }

        private void HandleGameEnded(GameEndedMessage message)
        {
            CurrentTurnUser = null;
            
            OnGameEnded?.Invoke();
            Debug.Log("[RoomClient] Game ended");
        }

        #endregion
    }
}
