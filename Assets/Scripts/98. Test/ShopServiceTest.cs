// using Study.Utilities;
// using UnityEngine;
// using UnityEngine.InputSystem;
//
// namespace BlueMarble
// {
//     /// <summary>
//     /// ShopService 동작 확인용 테스트 스크립트.
//     /// 0번 키: 상점 열기/닫기 (교체 대기 중엔 0=교체 포기)
//     /// 1~5번 키: 구매 모드 - 상점 목록 번호 / 교체 대기 모드 - 교체할 슬롯 번호
//     /// I 키: 현재 인벤토리 확인
//     /// </summary>
//     public class ShopServiceTest : MonoBehaviour
//     {
//         [SerializeField, Tooltip("비워두면 상점만 단독 테스트. EquipmentInventoryTest를 연결하면 그 인벤토리를 공유해서 같이 테스트.")]
//         private EquipmentInventoryTest sharedInventorySource;
//
//         private readonly ShopService shopService = new();
//         private readonly IGold gold = new Gold(1000);
//         private InventoryManager inventory;
//
//         private bool isShopOpen;
//
//         private void Awake()
//         {
//             inventory = sharedInventorySource != null
//                 ? sharedInventorySource.Inventory
//                 : new InventoryManager();
//         }
//
//         private void Update()
//         {
//             // 장비 데이터가 아직 로드되기 전이면 아무 입력도 처리하지 않는다.
//             if (DataManager.Instance.IsDataLoaded == false) return;
//
//             if (SimpleInput.GetKeyDown(Key.I))
//             {
//                 LogInventory();
//             }
//
//             if (isShopOpen == false)
//             {
//                 if (SimpleInput.GetKeyDown(Key.Digit0))
//                 {
//                     isShopOpen = true;
//                     Debug.Log("[Shop] 상점을 엽니다.");
//                     LogShopList();
//                 }
//
//                 return;
//             }
//
//             // 여기부터는 상점이 열려있는 상태
//             if (inventory.IsWaitingReplace)
//             {
//                 HandleReplaceInput();
//             }
//             else
//             {
//                 HandlePurchaseInput();
//             }
//         }
//
//         private void HandlePurchaseInput()
//         {
//             int shopIndex = GetDigitKeyDown();
//
//             if (shopIndex >= 0)
//             {
//                 shopService.TryPurchase(shopIndex, gold, inventory);
//                 return;
//             }
//
//             if (SimpleInput.GetKeyDown(Key.Digit0))
//             {
//                 isShopOpen = false;
//                 Debug.Log("[Shop] 상점을 닫습니다.");
//             }
//         }
//
//         private void HandleReplaceInput()
//         {
//             int slotIndex = GetDigitKeyDown();
//
//             if (slotIndex >= 0)
//             {
//                 inventory.ReplaceAt(slotIndex);
//                 Debug.Log($"[Shop] {slotIndex + 1}번 슬롯과 교체했습니다.");
//                 LogInventory();
//                 return;
//             }
//
//             if (SimpleInput.GetKeyDown(Key.Digit0))
//             {
//                 inventory.CancelReplace();
//                 Debug.Log("[Shop] 교체를 포기했습니다. (지불한 돈은 환불되지 않습니다)");
//             }
//         }
//
//         /// <summary>
//         /// 1~5 중 눌린 키가 있으면 0~4(슬롯/목록 인덱스)를, 없으면 -1을 반환한다.
//         /// </summary>
//         private int GetDigitKeyDown()
//         {
//             if (SimpleInput.GetKeyDown(Key.Digit1)) return 0;
//             if (SimpleInput.GetKeyDown(Key.Digit2)) return 1;
//             if (SimpleInput.GetKeyDown(Key.Digit3)) return 2;
//             if (SimpleInput.GetKeyDown(Key.Digit4)) return 3;
//             if (SimpleInput.GetKeyDown(Key.Digit5)) return 4;
//
//             return -1;
//         }
//
//         private void LogShopList()
//         {
//             var list = shopService.GetShopList();
//
//             Debug.Log("===== 상점 목록 =====");
//             for (int i = 0; i < list.Count; i++)
//             {
//                 EquipmentDef def = list[i];
//                 Debug.Log($"{i + 1}. {def.Name} - {def.Price}G (내구도 {def.MaxDurability}, 쿨타임 {def.Cooldown}s)");
//             }
//             Debug.Log("0. 상점 종료");
//         }
//
//         private void LogInventory()
//         {
//             Debug.Log("===== 인벤토리 =====");
//
//             for (int i = 0; i < InventoryManager.MaxSlotCount; i++)
//             {
//                 EquipmentInstance instance = inventory.GetAt(i);
//
//                 if (instance == null)
//                 {
//                     Debug.Log($"{i + 1}. (비어있음)");
//                     continue;
//                 }
//
//                 Debug.Log($"{i + 1}. {instance.Def.Name} - 내구도 {instance.CurrentDurability}/{instance.Def.MaxDurability}");
//             }
//         }
//     }
// }