using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Utility;

namespace BlueMarble.TestCode
{
    public class BoardRouteTest : MonoBehaviour
    {
        [SerializeField] private LocalPlayer player;

        [Button]
        private void Test_PlayerMoveRandom()
        {
            int diceResult = Random.Range(1, 7);
            _ = Test_PlayerMove(diceResult);
        }
        
        [Button]
        private async UniTaskVoid Test_PlayerMove(int diceResult)
        {
            Debug.Log($"[BoardRouteTest] Player {player.Id} move {diceResult} spaces.");
            
            for (int i = 0; i < diceResult; i++)
            {
                Tile reached = await player.MoveNext();
                
                bool isLanding = i == diceResult - 1;
                
                if (isLanding) await ResolveTileLanding(player, reached);
                else await ResolveTilePassing(player, reached);
                
                Debug.Log("==================================");
            }
        }

        private async UniTask ResolveTilePassing(LocalPlayer player, Tile tile)
        {
            Debug.Log($"[BoardRouteTest] Player {player.Id} passed {tile.SpaceDef.Name}.");
            Debug.Log($"[BoardRouteTest] {tile.SpaceDef.Name} pass effect: " +
                      $"{(tile.SpaceDef.IsActiveOnPass ? "Triggered" : "Skipped")}.");

            if (tile.SpaceDef.IsActiveOnPass)
            {
                await CustomUniTask.DelaySeconds(5f);
                Debug.Log($"[BoardRouteTest] {tile.SpaceDef.Name}'s effect is done.");
            }
        }

        private async UniTask ResolveTileLanding(LocalPlayer player, Tile tile)
        {
            Debug.Log($"[BoardRouteTest] Player {player.Id} landed on {tile.SpaceDef.Name}.");
            Debug.Log($"[BoardRouteTest] {tile.SpaceDef.Name} landing effect: Triggered");
            
            await UniTask.CompletedTask;
        }
    }
}
