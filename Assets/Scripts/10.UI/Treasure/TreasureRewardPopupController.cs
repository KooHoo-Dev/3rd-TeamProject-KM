using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace BlueMarble
{
    /// <summary>
    /// 보물 보상 팝업. 등장 -> 자동 오픈 연출 -> VFX -> 보상 표시 -> 확인 대기 -> 닫힘 순서로 진행한다.
    /// </summary>
    public class TreasureRewardPopupController : MonoBehaviour
    {
        private static readonly int OpenTrigger = Animator.StringToHash("Open");

        [Header("Popup Root")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Chest")]
        [SerializeField] private Animator chestAnimator;
        [SerializeField] private float chestOpenDuration = 0.6f; // treasure 클립 재생시간(Speed 0.6 기준)

        [Header("VFX")]
        [SerializeField] private TreasureVfxPlayer vfxPlayer;

        [Header("Reward Display")]
        [SerializeField] private Image rewardIconImage;
        [SerializeField] private TMP_Text rewardNameText;

        [Header("Confirm / Close")]
        [SerializeField] private Button confirmButton; // TreasureUI/PopupPanel/ConfirmButton
        [SerializeField] private Button closeButton;   // TreasureUI/PopupPanel/CloseButton - 확인 버튼과 완전히 동일하게 동작

        public async UniTask ShowRewardAsync(EquipmentDef reward)
        {
            Debug.Log("[TreasureRewardPopupController] 필드 상태 점검 - " +
                      $"canvasGroup={(canvasGroup == null ? "NULL" : "OK")}, " +
                      $"chestAnimator={(chestAnimator == null ? "NULL" : "OK")}, " +
                      $"vfxPlayer={(vfxPlayer == null ? "NULL" : "OK")}, " +
                      $"rewardIconImage={(rewardIconImage == null ? "NULL" : "OK")}, " +
                      $"rewardNameText={(rewardNameText == null ? "NULL" : "OK")}, " +
                      $"confirmButton={(confirmButton == null ? "NULL" : "OK")}, " +
                      $"closeButton={(closeButton == null ? "NULL" : "OK")}");

            gameObject.SetActive(true);
            SetConfirmControlsActive(false);
            canvasGroup.alpha = 0f;

            // 1. 팝업 페이드인
            await canvasGroup.DOFade(1f, 0.25f).AsyncWaitForCompletion().AsUniTask();

            // 2. 상자 열림 연출 (자동) - chestAnimator는 ChestImage 오브젝트에 부착
            chestAnimator.SetTrigger(OpenTrigger);
            await UniTask.Delay(TimeSpan.FromSeconds(chestOpenDuration));

            // 3. 폭죽 VFX
            await vfxPlayer.PlayAsync(reward.Grade);

            // 4. 보상 정보 표시
            ApplyRewardInfo(reward);

            // 5. 확인/닫기 버튼 활성화 + 둘 중 하나라도 클릭될 때까지 대기 (X와 확인은 동일 동작)
            SetConfirmControlsActive(true);
            await WaitForConfirmClick();

            // 6. 페이드아웃 + 닫힘
            await canvasGroup.DOFade(0f, 0.25f).AsyncWaitForCompletion().AsUniTask();
            gameObject.SetActive(false);
        }

        private void SetConfirmControlsActive(bool active)
        {
            confirmButton.gameObject.SetActive(active);
            closeButton.gameObject.SetActive(active);
        }

        private void ApplyRewardInfo(EquipmentDef reward)
        {
            rewardNameText.text = $"{reward.Name} 획득!";

            rewardIconImage.gameObject.SetActive(false);

            if (string.IsNullOrEmpty(reward.IconKey)) return;

            Sprite icon = Resources.Load<Sprite>(reward.IconKey);
            if (icon == null)
            {
                Debug.Log($"[TreasureRewardPopupController] 아이콘을 찾을 수 없습니다: {reward.IconKey}");
                return;
            }

            rewardIconImage.sprite = icon;
            rewardIconImage.gameObject.SetActive(true);
        }

        /// <summary>
        /// 확인 버튼과 X(닫기) 버튼 중 어느 쪽을 눌러도 동일하게 진행되도록, 둘 다 같은 완료 신호에 연결한다.
        /// </summary>
        private async UniTask WaitForConfirmClick()
        {
            var tcs = new UniTaskCompletionSource();
            void OnClicked() => tcs.TrySetResult();

            confirmButton.onClick.AddListener(OnClicked);
            closeButton.onClick.AddListener(OnClicked);

            await tcs.Task;

            confirmButton.onClick.RemoveListener(OnClicked);
            closeButton.onClick.RemoveListener(OnClicked);
        }
    }
}