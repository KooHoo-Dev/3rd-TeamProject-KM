// using Cysharp.Threading.Tasks;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using Utility;
//
// namespace BlueMarble.TestCode
// {
//     public class BoardSpecialTileTest : MonoBehaviour
//     {
//         [SerializeField] private LocalPlayer player;
//         [SerializeField] private ShopUIController shopUIController; // 씬의 Store UI/ShopPanel에 붙어있는 컴포넌트를 드래그
//
//         private SpecialTileService specialTileService;
//         private InventoryManager myinventory = InventoryManager.Instance;
//         
//         private void Awake()
//         {
//             IGold gold = new Gold(startGold: 1000);
//             ITurnSkipRegistry turnSkipRegistry = new MockTurnSkipRegistry();
//             IBoardTeleporter boardTeleporter = new MockBoardTeleporter();
//             
//            
//             
//             
//             // ShopUIController를 인스펙터에 연결했으면 실제 상점 UI를 쓰고,
//             // 안 붙였으면 기존처럼 로그만 찍는 MockShopOpener로 대체한다.
//             IShopOpener shopOpener;
//             if (shopUIController != null)
//             {
//                 shopUIController.Init(gold, myinventory);
//                 shopOpener = shopUIController;
//             }
//             else
//             {
//                 shopOpener = new MockShopOpener();
//                 Debug.LogWarning("[BoardSpecialTileTest] ShopUIController가 연결되지 않아 MockShopOpener를 사용합니다.");
//             }
//
//             specialTileService = new SpecialTileService(gold, turnSkipRegistry, boardTeleporter, shopOpener);
//         }
//
//         [Button]
//         private void Test_PlayerMoveRandom()
//         {
//             int diceResult = Random.Range(1, 7);
//             _ = Test_PlayerMove(diceResult);
//         }
//
//         [Button]
//         private async UniTaskVoid Test_PlayerMove(int diceResult)
//         {
//             Debug.Log($"[BoardRouteTest] Player {player.Id} move {diceResult} spaces.");
//
//             for (int i = 0; i < diceResult; i++)
//             {
//                 Tile reached = await player.MoveNext();
//
//                 bool isLanding = i == diceResult - 1;
//
//                 if (isLanding) await ResolveTileLanding(player, reached);
//                 else await ResolveTilePassing(player, reached);
//
//                 Debug.Log("==================================");
//             }
//         }
//
//         private async UniTask ResolveTilePassing(LocalPlayer player, Tile tile)
//         {
//             Debug.Log($"[BoardRouteTest] Player {player.Id} passed {tile.SpaceDef.Name}.");
//
//             // HandleTilePassing이 이제 UniTask를 반환한다.
//             // 상점 칸이면 이 await 지점에서 실제로 멈춰서, 상점이 닫힐 때까지 기다린 뒤 다음 칸으로 넘어간다.
//             await specialTileService.HandleTilePassing(player.Id, tile.SpaceDef.Id);
//         }
//
//         private async UniTask ResolveTileLanding(LocalPlayer player, Tile tile)
//         {
//             Debug.Log($"[BoardRouteTest] Player {player.Id} landed on {tile.SpaceDef.Name}.");
//
//             await specialTileService.HandleTileLanding(player.Id, tile.SpaceDef.Id);
//         }
//     }
// }