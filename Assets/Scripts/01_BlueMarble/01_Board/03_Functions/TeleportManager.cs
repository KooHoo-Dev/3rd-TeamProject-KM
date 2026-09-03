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
                
                // 얻어온 Tile을 이용해서 해당 칸으로 플레이어를 이동시킴
                // 이건 서버랑 이케이케 해야할듯?
                return;
            }
        }
    }
}