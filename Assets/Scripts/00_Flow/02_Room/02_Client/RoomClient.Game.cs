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

        public event Action<User, bool> OnTurnStarted;
        public event Action<User, int, int> OnDiceRolled;
        public event Action<User, int> OnGoldCardDrawn;

        public event Action OnGameEnded;

        public event Action<UserEconomy[]> OnEconomyUpdated;
        public event Action<UserMovedToMessage> OnUserMovedTo;
        public event Action<UserMoveFinishedMessage> OnUserMoveFinished;
        
        public IReadOnlyList<string> PlayerIds => playerIds;

        private void RegisterGameHandlers()
        {
            RegisterHandler<GameStartedMessage>(ProtocolHeader.GAME_STARTED, HandleGameStarted);
            RegisterHandler<TurnStartedMessage>(ProtocolHeader.TURN_STARTED, HandleTurnStarted);
            RegisterHandler<DiceRolledMessage>(ProtocolHeader.DICE_ROLLED, HandleDiceRolled);
            RegisterHandler<GoldCardDrawnMessage>(ProtocolHeader.GOLD_CARD_DRAWN, HandleGoldCardDrawn);
            RegisterHandler<GameEndedMessage>(ProtocolHeader.GAME_ENDED, HandleGameEnded);

            RegisterHandler<EconomyUpdatedMessage>(ProtocolHeader.ECONOMY_UPDATED, HandleEconomyUpdated);
            RegisterHandler<UserMovedToMessage>(ProtocolHeader.USER_MOVED_TO, HandleUserMovedTo);
            RegisterHandler<UserMoveFinishedMessage>(ProtocolHeader.USER_MOVE_FINISHED, HandleUserMoveFinished);
        }
        
        #region SEND_TO_SERVER

        public UniTask AnnounceBoardReady()
        {
            return SendAsync(new SetBoardReadyMessage());
        }

        public UniTask AnnounceTurnFinished(int turnId)
        {
            return SendAsync(new TurnFinishedMessage { TurnId = turnId });
        }

        public UniTask AnnounceUserMoveFinished(int turnId, long moveId, string requestId)
        {
            return SendAsync(new UserMoveFinishedMessage
            {
                TurnId = turnId,
                MoveId = moveId,
                RequestId = requestId
            });
        }

        public void RequestRollDice(int turnId)
        {
            SendAsync(new RollDiceMessage { TurnId = turnId }).Forget();
        }

        public void RequestDrawGoldCard(int turnId, int[] cardIds)
        {
            SendAsync(new DrawGoldCardMessage
            {
                TurnId = turnId,
                CardIds = cardIds
            }).Forget();
        }

        public void RequestUpdateEconomy(params EconomyUpdate[] updates)
        {
            SendAsync(new UpdateEconomyMessage { Updates = updates }).Forget();
        }

        public UniTask RequestAddIncapacitationCount(int count)
        {
            return SendAsync(new AddIncapacitationCountMessage { Count = count });
        }

        public UniTask RequestMovePlayerTo(string playerId, int tileId, int turnId, string requestId)
        {
            return SendAsync(new MoveUserToMessage
            {
                UserId = playerId, 
                TileId = tileId, 
                TurnId = turnId, 
                RequestId = requestId
            });
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
            
            OnTurnStarted?.Invoke(user, message.CanAct);
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

        private void HandleGoldCardDrawn(GoldCardDrawnMessage message)
        {
            if (message.TurnId != TurnId) return;
            
            User user = GetUser(message.PlayerId);
            
            OnGoldCardDrawn?.Invoke(user, message.CardId);
            Debug.Log($"[RoomClient] {user.Nickname}({user.Id}) drew gold card: {message.CardId}. " +
                      $"Turn Id: {message.TurnId}");
        }

        private void HandleGameEnded(GameEndedMessage message)
        {
            CurrentTurnUser = null;
            
            OnGameEnded?.Invoke();
            Debug.Log("[RoomClient] Game ended");
        }

        private void HandleEconomyUpdated(EconomyUpdatedMessage message)
        {
            OnEconomyUpdated?.Invoke(message.Economies);
            Debug.Log("[RoomClient] Economy updated");
        }

        private void HandleUserMovedTo(UserMovedToMessage message)
        {
            OnUserMovedTo?.Invoke(message);
            Debug.Log("[RoomClient] User moved to");
        }
        
        private void HandleUserMoveFinished(UserMoveFinishedMessage message)
        {
            OnUserMoveFinished?.Invoke(message);
            Debug.Log("[RoomClient] User move finished");
        }

        #endregion
    }
}
