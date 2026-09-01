using UnityEngine;

namespace BlueMarble
{
  
    public class TreasureService
    {
        public static TreasureService Instance { get; } = new TreasureService();

        private TreasureService()
        {
        }

        /// <summary>
        /// 보물창고 드롭 테이블에서 장비 하나를 뽑아 플레이어 인벤토리에 지급한다.
        /// 인벤토리가 가득 찬 경우 InventoryManager가 자동으로 교체 대기 상태로 전환한다.
        /// </summary>
        /// <param name="playerId">지급 대상 플레이어 (현재는 로그용, 추후 플레이어별 인벤토리 분리 시 사용)</param>
        /// <returns>지급을 시도한 장비 정의</returns>
        public EquipmentDef Grant(string playerId)
        {
            EquipmentDef def = DropService.DrawEquipment(DropType.Treasure);

            bool added = InventoryManager.Instance.TryAddEquipment(def);

            if (added)
            {
                Debug.Log($"[TreasureService] {playerId}: 보물창고에서 {def.Name}({def.Grade}) 획득");
            }
            else
            {
                Debug.Log($"[TreasureService] {playerId}: 보물창고에서 {def.Name}({def.Grade}) 획득, " +
                          "인벤토리가 가득 차서 교체 대기 상태입니다.");
            }

            return def;
        }
    }
}