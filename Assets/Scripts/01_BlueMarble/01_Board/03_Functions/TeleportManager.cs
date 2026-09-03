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
            
            while (true)
            {
                if (SimpleInput.GetMouseButtonDown(0) == false)
                {
                    await UniTask.Yield();
                    continue;
                }
                
                Vector2 mousePos = SimpleInput.GetMousePosition();
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
                worldPos.z = 0;

                if (board.TryGetTile(worldPos, out Tile tile) == false)
                {
                    await UniTask.Yield();
                    continue;
                }
                
                Client.RequestMovePlayerTo(playerId, tile.SpaceDef.Id); 

                return;
            }
        }
    }
}