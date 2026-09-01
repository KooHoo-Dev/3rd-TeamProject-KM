using System;
using BlueMarble.Server;
using Cysharp.Threading.Tasks;
using Jay;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace BlueMarble
{
    public class GameManager : SingletonBase<GameManager>
    {
        [field: Title("References")]
        [field: SerializeField] public PlayerManager Players { get; private set; }
        [field: SerializeField] public TurnManager Turn { get; private set; }

        private RoomClient Client => RoomClient.Instance;
        private BlueMarbleWorld World => BlueMarbleWorld.Instance;
        
        private async UniTaskVoid Start()
        {
            // 도중에 연결이 끊겼다거나, 기타 등등의 이유로
            // 게임이 시작되었는데 다른 플레이어가 없을 경우
            if (Client.Others.Count <= 0)
            {
                Debug.LogError("[GameManager] No users in the room.");
                // Room으로 다시 보내버린다? 뭐 일단 이건 후순위
                return;
            }
            
            Players.Initialize(Client.PlayerIds, Client.Me.Id);
            
            Client.OnTurnStarted += HandleTurnStarted;
            Client.OnDiceRolled += HandleDiceRolled;
            Client.OnGameEnded += HandleGameEnded;
            
            Turn.OnDiceRollRequested += HandleDiceRollDiceRequested;

            await UniTask.WaitUntil(
                this,
                self => World.IsBoardReady,
                cancellationToken: destroyCancellationToken);

            await Client.AnnounceBoardReady();
        }

        private void OnDisable()
        {
            Client.OnTurnStarted -= HandleTurnStarted;
            Client.OnDiceRolled -= HandleDiceRolled;
            Client.OnGameEnded -= HandleGameEnded;
            
            Turn.OnDiceRollRequested -= HandleDiceRollDiceRequested;
        }

        private void HandleTurnStarted(User user)
        {
            bool isMyTurn = user.Id == Client.Me.Id;
            Turn.SetInputEnabled(isMyTurn);
            
            Debug.Log("[GameManager] " +
                      $"Round {Client.RoundCount}, " +
                      $"Turn {Client.TurnId}, " +
                      $"Player {user.Nickname}({user.Id})");
        }

        private void HandleDiceRollDiceRequested()
        {
            User currentUser = Client.CurrentTurnUser;

            if (currentUser == null) return;
            if (currentUser.Id != Client.Me.Id) return;
            
            Client.RequestRollDice(Client.TurnId);
        }

        private void HandleDiceRolled(User user, int d1, int d2)
        {
            int turnId = Client.TurnId;
            PlayDicePresentationAsync(user.Id, d1, d2, turnId).Forget();
        }

        private async UniTask PlayDicePresentationAsync(string playerId, int d1, int d2, int turnId)
        {
            await Turn.ShowDiceResultAsync(playerId, d1, d2);
            await Client.AnnounceDiceAnimationEnded(turnId);
        }

        private void HandleGameEnded()
        {
            Turn.SetInputEnabled(false);
            Debug.Log($"[GameManager] Game Ended.");
        }
    }
}