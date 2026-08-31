using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Utility;

namespace BlueMarble.TestCode
{
    public class BoardSpecialTileTest : MonoBehaviour
    {
        [SerializeField] private LocalPlayer player;

        private SpecialTileService specialTileService;

        private void Awake()
        {
            IGold gold = new Gold(startGold: 1000);
            ITurnSkipRegistry turnSkipRegistry = new MockTurnSkipRegistry();
            IBoardTeleporter boardTeleporter = new MockBoardTeleporter();
            IShopOpener shopOpener = new MockShopOpener();

            specialTileService = new SpecialTileService(gold, turnSkipRegistry, boardTeleporter, shopOpener);
        }

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

            specialTileService.HandleTilePassing(player.Id, tile.SpaceDef.Id);

            await UniTask.CompletedTask;
        }

        private async UniTask ResolveTileLanding(LocalPlayer player, Tile tile)
        {
            Debug.Log($"[BoardRouteTest] Player {player.Id} landed on {tile.SpaceDef.Name}.");

            specialTileService.HandleTileLanding(player.Id, tile.SpaceDef.Id);

            await UniTask.CompletedTask;
        }
    }
}