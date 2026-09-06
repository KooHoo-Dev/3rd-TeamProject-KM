using BlueMarble.Server;
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

        private TreasureRewardPopupController presenter;

        private TreasureManager()
        {
        }

        /// <summary>
        /// 보물창고 UI 팝업(presenter)을 등록한다. 씬 진입 시 팝업 프리팹 쪽에서 1회 호출한다.
        /// </summary>
        public void SetPresenter(TreasureRewardPopupController presenter)
        {
            this.presenter = presenter;
        }

        /// <summary>
        /// 보물창고 드롭 테이블에서 장비 하나를 뽑아 플레이어 인벤토리에 지급한다.
        /// 인벤토리가 가득 찬 경우 InventoryManager가 자동으로 교체 대기 상태로 전환한다.
        /// 보상 팝업은 로컬 플레이어 본인의 턴일 때만 띄운다 (다른 플레이어 화면에는 안 뜸).
        /// </summary>
        /// <param name="playerId">지급 대상 플레이어</param>
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

            bool isLocalPlayer = playerId == RoomClient.Instance.Me.Id; // TODO: 실제 프로퍼티명 확인 필요

            if (isLocalPlayer && presenter != null)
            {
                await presenter.ShowRewardAsync(def);
            }
        }
    }
}
