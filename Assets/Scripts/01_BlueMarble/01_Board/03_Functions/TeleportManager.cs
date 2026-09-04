using System;
using BlueMarble.Server;
using Cysharp.Threading.Tasks;
using Jay;
using Sirenix.OdinInspector;
using Study.Utilities;
using UnityEngine;

namespace BlueMarble
{
    public class TeleportManager : SingletonBase<TeleportManager>
    {
        [Title("Components")]
        [SerializeField] private Camera mainCamera;

        private RoomClient Client => RoomClient.Instance;
        private BlueMarbleWorld World => BlueMarbleWorld.Instance;
        
        public async UniTask ExecuteAsync(string playerId)
        {
            Board board = World.Board;

            if (PlayerRegistry.TryGetPlayer(playerId, out IPlayer player) == false)
            {
                Debug.Log($"[TeleportManager] Cannot find player: {playerId}");
                return;
            }
            
            while (true)
            {
                await UniTask.Yield();
                
                if (SimpleInput.GetMouseButtonDown(0) == false) continue;
                
                Vector2 mousePos = SimpleInput.GetMousePosition();
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
                worldPos.z = 0;

                if (board.TryGetTile(worldPos, out Tile tile) == false) continue;
                if (board.GetTile(player.Position) == tile) continue;
                
                int tileIdx = World.Board.GetTileIndex(tile);

                string requestId = Guid.NewGuid().ToString("N");
                UniTaskCompletionSource<bool> completionSource = new();

                Client.OnUserMoveFinished += HandleMoveFinished;

                try
                {
                    await Client.RequestMovePlayerTo(playerId, tileIdx, Client.TurnId, requestId);
                    await completionSource.Task;
                }
                finally
                {
                    Client.OnUserMoveFinished -= HandleMoveFinished;
                }
                
                return;
                
                void HandleMoveFinished(UserMoveFinishedMessage msg)
                {
                    if (msg.RequestId != requestId) return;
                    completionSource.TrySetResult(true);
                }
            }
        }
    }
}