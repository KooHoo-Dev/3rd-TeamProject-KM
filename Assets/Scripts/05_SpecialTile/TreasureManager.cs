using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BlueMarble
{
    /// <summary>
    /// E5 보물창고 효과를 처리하는 매니저.
    /// DropService(가중 랜덤 뽑기)와 InventoryManager(지급)를 연결한다.
    /// TreasureEffect에서 Instance로 호출한다.
    /// </summary>
    public class TreasureManager
    {
        public static TreasureManager Instance { get; } = new TreasureManager();

        private TreasureManager()
        {
        }

        /// <summary>
        /// 보물창고 드롭 테이블에서 장비 하나를 뽑아 플레이어 인벤토리에 지급한다.
        /// 인벤토리가 가득 찬 경우 InventoryManager가 자동으로 교체 대기 상태로 전환한다.
        /// </summary>
        /// <param name="playerId">지급 대상 플레이어 (현재는 로그용, 추후 플레이어별 인벤토리 분리 시 사용)</param>
        public async UniTask GrantAsync(string playerId)
        {
            EquipmentDef def = DropService.DrawEquipment(DropType.Treasure);

            bool added = InventoryManager.Instance.TryAddEquipment(def);

            if (added)
            {
                Debug.Log($"[TreasureManager] {playerId}: 보물창고에서 {def.Name}({def.Grade}) 획득");
            }
            else
            {
                Debug.Log($"[TreasureManager] {playerId}: 보물창고에서 {def.Name}({def.Grade}) 획득, " +
                          "인벤토리가 가득 차서 교체 대기 상태입니다.");
            }

            await UniTask.CompletedTask;
        }
    }
}