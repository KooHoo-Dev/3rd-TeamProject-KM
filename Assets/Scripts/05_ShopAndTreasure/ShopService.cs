using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BlueMarble
{
    public class ShopService
    {

        /// <summary>
        /// 상점에서 Common 등급 불러오기
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<EquipmentDef> GetShopList()
        {
            return DataManager.Instance.Equipments.GetDefs().Where(def => def.Grade == EquipmentGrade.Common).ToList();
        }

        /// <summary>
        /// 상점 목록 중 하나(shopIndex, 0부터 시작)를 구매 시도한다.
        /// 자금은 서버가 관리하는 EconomyManager를 직접 조회/차감한다.
        /// 자금 확인/차감 -> 인벤토리 추가 순서로 진행하며,
        /// 인벤토리가 가득 차 있으면 InventoryManager가 자동으로 교체 대기 상태로 전환한다.
        /// 구매(자금 차감)가 성공했으면 true, 자금 부족이나 잘못된 번호면 false
        /// </summary>
        /// <returns></returns>
        public bool TryPurchase(int shopIndex, string playerId, InventoryManager inventory)
        {
            IReadOnlyList<EquipmentDef> list = GetShopList();

            if (shopIndex < 0 || shopIndex >= list.Count)
            {
                Debug.Log("[ShopService] 잘못된 번호입니다.");
                return false;
            }

            EquipmentDef def = list[shopIndex];

            if (!EconomyManager.Instance.TryGetGold(playerId, out int currentGold) || currentGold < def.Price)
            {
                Debug.Log("[ShopService] 자금이 부족합니다.");
                return false;
            }

            // 서버에 차감 요청 (fire-and-forget) - 실제 최신 골드값은 이후 EconomyManager.OnUpdated로 반영된다.
            EconomyManager.Instance.SpendGold(playerId, def.Price);

            bool added = inventory.TryAddEquipment(def);

            if (added)
            {
                Debug.Log($"[ShopService] {def.Name} 구매 완료 (인벤토리에 즉시 장착)");
            }
            else
            {
                Debug.Log($"[ShopService] {def.Name} 구매 완료, 인벤토리가 가득 차서 교체 대기 상태입니다. (교체할 슬롯을 선택하거나 포기하세요)");
            }

            return true;
        }



    }
}