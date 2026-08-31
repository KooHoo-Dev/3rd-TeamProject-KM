// using Study.Utilities;
// using UnityEngine;
// using UnityEngine.InputSystem;
//
// namespace BlueMarble
// {
//     /// <summary>
//     /// 인벤토리 매니저 동작 확인용 테스트 스크립트.
//     /// 0번 키: 현재 인벤토리 상태 조회
//     /// 1번 키: 장비 테이블(id 1~10)에서 랜덤 5개를 뽑아 인벤토리를 채움
//     /// 2번 키: 인벤토리를 전부 비움
//     /// 3번 키: 낡은 단검(id=1)만 5개 채움 (중복 장비 인스턴스 확인용)
//     /// 4번 키: 현재 상태에서 장비 1개 추가 시도 (가득 찼을 때 실패하는지 확인용)
//     /// 5번 키: 보유 장비 중 무작위 1개 즉시 파괴 (E8-T2-S2)
//     /// 6번 키: (교체 대기 모드 전용) 신규 장비 획득 포기
//     /// 7번 키: 보유 장비 전체 내구도 -1, 0되면 제거 (E8-T2-S1)
//     /// ※ 교체 대기 모드 중에는 1~5번이 "교체할 슬롯 선택"으로 동작이 바뀐다.
//     /// </summary>
//     public class EquipmentInventoryTest : MonoBehaviour
//     {
//         private const int MinEquipmentId = 1;
//         private const int MaxEquipmentId = 20;
//         private const int RustyDaggerId = 1; // 낡은 단검
//
//         private readonly InventoryManager inventory = new();
//         private bool hasLoggedReplacePrompt = false;
//         public InventoryManager Inventory { get; set; }
//
//         private void Update()
//         {
//             // 교체 대기 중이면, 평소 키(1~4)를 "교체 대상 슬롯 선택"으로 사용한다.
//             if (inventory.IsWaitingReplace)
//             {
//                 HandleReplaceInput();
//                 return;
//             }
//
//             if (SimpleInput.GetKeyDown(Key.Digit1))
//             {
//                 FillRandom();
//             }
//
//             if (SimpleInput.GetKeyDown(Key.Digit2))
//             {
//                 ClearInventory();
//             }
//
//             if (SimpleInput.GetKeyDown(Key.Digit3))
//             {
//                 FillWithRustyDaggers();
//             }
//
//             if (SimpleInput.GetKeyDown(Key.Digit4))
//             {
//                 TestOverflow();
//             }
//
//             if (SimpleInput.GetKeyDown(Key.Digit0))
//             {
//                 Debug.Log("===== [0] 현재 인벤토리 상태 =====");
//                 LogInventory();
//             }
//
//             if (SimpleInput.GetKeyDown(Key.Digit5))
//             {
//                 Debug.Log("===== [5] 무작위 장비 파괴 =====");
//                 inventory.DestroyRandomEquipment();
//                 LogInventory();
//             }
//
//             if (SimpleInput.GetKeyDown(Key.Digit7))
//             {
//                 Debug.Log("===== [7] 전체 내구도 -1 =====");
//                 inventory.LossDurabilityAll();
//                 LogInventory();
//             }
//         }
//
//         /// <summary>
//         /// 교체 대기 모드 동안의 입력 처리.
//         /// 1~5번: 해당 슬롯 번호와 교체 / 6번: 포기(취소)
//         /// 실제 게임에서는 이 부분이 UI 클릭으로 대체된다 (inventory.ReplaceAt / CancelReplace 호출은 동일).
//         /// </summary>
//         private void HandleReplaceInput()
//         {
//             if (hasLoggedReplacePrompt == false)
//             {
//                 Debug.Log(
//                     $"[교체 대기] 새 장비: {inventory.ReplaceDef.Name} — " +
//                     "교체할 슬롯 번호(1~5)를 누르거나, 6번을 눌러 포기하세요.");
//                 hasLoggedReplacePrompt = true;
//             }
//
//             if (SimpleInput.GetKeyDown(Key.Digit1)) { inventory.ReplaceAt(0); FinishReplace(); }
//             else if (SimpleInput.GetKeyDown(Key.Digit2)) { inventory.ReplaceAt(1); FinishReplace(); }
//             else if (SimpleInput.GetKeyDown(Key.Digit3)) { inventory.ReplaceAt(2); FinishReplace(); }
//             else if (SimpleInput.GetKeyDown(Key.Digit4)) { inventory.ReplaceAt(3); FinishReplace(); }
//             else if (SimpleInput.GetKeyDown(Key.Digit5)) { inventory.ReplaceAt(4); FinishReplace(); }
//             else if (SimpleInput.GetKeyDown(Key.Digit6))
//             {
//                 inventory.CancelReplace();
//                 hasLoggedReplacePrompt = false;
//                 Debug.Log("[교체 대기] 포기함. 인벤토리는 그대로 유지됩니다.");
//             }
//         }
//
//         private void FinishReplace()
//         {
//             hasLoggedReplacePrompt = false;
//             LogInventory();
//         }
//
//         /// <summary>
//         /// id 1~20 중 랜덤으로 5개를 뽑아 인벤토리에 채운다.
//         /// 존재하지 않는 id를 뽑으면 건너뛰고 다시 뽑는다. (현재 테이블은 1~10만 존재)
//         /// </summary>
//         private void FillRandom()
//         {
//             ClearInventory();
//
//             Debug.Log("===== [1] 랜덤 장비 5개 채우기 =====");
//
//             int filled = 0;
//             int safetyCount = 0; // 무한루프 방지용
//
//             while (filled < InventoryManager.MaxSlotCount && safetyCount < 1000)
//             {
//                 safetyCount++;
//
//                 int randomId = Random.Range(MinEquipmentId, MaxEquipmentId + 1);
//
//                 EquipmentDef def;
//                 try
//                 {
//                     def = DataManager.Instance.GetEquipmentDef(randomId);
//                 }
//                 catch (System.Collections.Generic.KeyNotFoundException)
//                 {
//                     continue; // 존재하지 않는 id면 다시 뽑기
//                 }
//
//                 bool added = inventory.TryAddEquipment(def);
//                 if (added == false) break; // 이미 가득 참
//
//                 filled++;
//             }
//
//             LogInventory();
//         }
//
//         /// <summary>
//         /// 낡은 단검(id=1)만 5개를 채운다.
//         /// 같은 EquipmentDef를 여러 번 추가했을 때 각 인스턴스가
//         /// 서로 다른 InstanceId/SlotIndex를 갖는지 확인하기 위한 테스트.
//         /// </summary>
//         private void FillWithRustyDaggers()
//         {
//             ClearInventory();
//
//             Debug.Log("===== [3] 낡은 단검 5개 채우기 =====");
//
//             EquipmentDef def;
//             try
//             {
//                 def = DataManager.Instance.GetEquipmentDef(RustyDaggerId);
//             }
//             catch (System.Collections.Generic.KeyNotFoundException)
//             {
//                 Debug.LogError($"[3] id={RustyDaggerId} 장비를 찾을 수 없습니다.");
//                 return;
//             }
//
//             for (int i = 0; i < InventoryManager.MaxSlotCount; i++)
//             {
//                 bool added = inventory.TryAddEquipment(def);
//                 if (added == false) break;
//             }
//
//             LogInventory();
//         }
//
//         /// <summary>
//         /// 인벤토리가 가득 찬 상태에서 추가 시도가 정확히 실패(false)하는지 확인한다.
//         /// 현재 인벤토리 상태를 먼저 채우지 않고, 지금 갖고 있는 상태 그대로 시도한다.
//         /// (미리 1번이나 3번을 눌러 5칸을 채운 뒤 4번을 눌러야 의미 있는 테스트가 됨)
//         /// </summary>
//         private void TestOverflow()
//         {
//             Debug.Log("===== [4] 초과(가득 참) 테스트 =====");
//             Debug.Log($"현재 가득 참 여부(IsFull): {inventory.IsFull}");
//
//             EquipmentDef def;
//             try
//             {
//                 def = DataManager.Instance.GetEquipmentDef(RustyDaggerId);
//             }
//             catch (System.Collections.Generic.KeyNotFoundException)
//             {
//                 Debug.LogError($"[4] id={RustyDaggerId} 장비를 찾을 수 없습니다.");
//                 return;
//             }
//
//             bool added = inventory.TryAddEquipment(def);
//             Debug.Log($"6번째 장비 추가 시도 → 성공 여부: {added} (가득 찬 상태라면 false가 정상)");
//             LogInventory();
//         }
//
//         /// <summary>
//         /// 현재 인벤토리 상태(빈 슬롯 포함 5칸 전체)를 표 형태로 출력한다.
//         /// </summary>
//         private void LogInventory()
//         {
//             var sb = new System.Text.StringBuilder();
//             sb.AppendLine("[인벤토리 상태]");
//             sb.AppendLine("슬롯 | ID | 이름          | 등급       | 내구도");
//             sb.AppendLine("--------------------------------------------");
//
//             for (int i = 0; i < InventoryManager.MaxSlotCount; i++)
//             {
//                 EquipmentInstance instance = inventory.GetAt(i);
//
//                 if (instance == null)
//                 {
//                     sb.AppendLine($"{i,4} | -- | (비어있음)");
//                     continue;
//                 }
//
//                 sb.AppendLine(
//                     $"{i,4} | {instance.Def.Id,2} | {instance.Def.Name,-12} | " +
//                     $"{instance.Def.Grade,-9} | {instance.CurrentDurability}/{instance.Def.MaxDurability}");
//             }
//
//             Debug.Log(sb.ToString());
//         }
//
//         /// <summary>
//         /// 인벤토리 5슬롯을 전부 비운다.
//         /// </summary>
//         private void ClearInventory()
//         {
//             for (int i = 0; i < InventoryManager.MaxSlotCount; i++)
//             {
//                 inventory.RemoveAt(i);
//             }
//
//             Debug.Log("===== [2] 인벤토리 비움 =====");
//             LogInventory();
//         }
//     }
// }