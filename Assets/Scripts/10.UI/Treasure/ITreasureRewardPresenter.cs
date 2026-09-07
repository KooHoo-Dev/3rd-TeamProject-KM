using Cysharp.Threading.Tasks;

namespace BlueMarble
{
    /// <summary>
    /// 보물 보상 팝업의 전체 시퀀스(등장→오픈 연출→VFX→보상표시→확인대기→닫힘)를 책임진다.
    /// TreasureManager는 이 인터페이스만 알고, 팝업의 내부 구현(DOTween, Animator 등)은 모른다.
    /// </summary>
    public interface ITreasureRewardPresenter
    {
        /// <summary>
        /// 팝업을 띄우고, 유저가 확인 버튼을 눌러 닫을 때까지 대기한다.
        /// 인벤토리 지급 성공/대기 여부와 무관하게, "무엇을 뽑았는지"만 표시한다.
        /// </summary>
        UniTask ShowRewardAsync(EquipmentDef reward);
    }

    /// <summary>
    /// 인벤토리 UI를 띄우고, 유저가 닫을 때까지 대기한다.
    /// 인벤토리가 가득 차 교체 대기 중이면, 슬롯 클릭 시 자동 교체까지 이 프리젠터 범위에서 처리한다.
    /// </summary>
    public interface IInventoryPresenter
    {
        UniTask ShowAsync();

        /// <summary>
        /// 등급에 맞는 폭죽 VFX 프리팹을 재생하고 정리하는 책임만 가진다.
        /// </summary>
        public interface ITreasureVfxPlayer
        {
            UniTask PlayAsync(EquipmentGrade grade);
        }
    }
}