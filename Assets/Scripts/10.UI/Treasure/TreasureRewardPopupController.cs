using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace BlueMarble
{
    /// <summary>
    /// 보물 보상 팝업. 등장 -> 자동 오픈 연출 -> VFX -> 보상 표시 -> (보상 클릭/확인 시 인벤토리 오픈, X는 그냥 닫기) 순서로 진행한다.
    /// </summary>
    public class TreasureRewardPopupController : MonoBehaviour, ITreasureRewardPresenter
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
        [SerializeField] private Button rewardIconButton; // rewardIconImage에 Button 컴포넌트 추가해서 연결 (클릭 시 인벤토리로 진입)
        [SerializeField] private TMP_Text rewardNameText;

        [Header("Confirm / Close")]
        [SerializeField] private Button confirmButton; // 클릭 시 인벤토리로 진입 (보상 아이콘과 동일 동작)
        [SerializeField] private Button closeButton;   // 클릭 시 인벤토리 안 보고 그냥 닫기 (스킵)

        [Header("Inventory")]
        [SerializeField] private InventoryUIController inventoryPresenter;

        public async UniTask ShowRewardAsync(EquipmentDef reward)
        {
            gameObject.SetActive(true);
            SetConfirmControlsActive(false);
            canvasGroup.alpha = 0f;

            // 1. 팝업 페이드인
            await canvasGroup.DOFade(1f, 0.25f).AsyncWaitForCompletion().AsUniTask();
            
            

            // 2. 상자 열림 연출 (자동)
            chestAnimator.SetTrigger(OpenTrigger);
            await UniTask.Delay(TimeSpan.FromSeconds(chestOpenDuration));

            // 3. 폭죽 VFX
            await vfxPlayer.PlayAsync(reward.Grade);

            // 4. 보상 정보 표시
            ApplyRewardInfo(reward);

            // 5. 보상 아이콘/확인(=인벤토리로 진입) 또는 X(=그냥 닫기) 대기
            SetConfirmControlsActive(true);
            bool wantsInventory = await WaitForProceedOrSkip();

            // 6. 인벤토리로 진입을 선택한 경우, 인벤토리가 닫힐 때까지 대기
            if (wantsInventory)
            {
                await inventoryPresenter.ShowAsync();
            }

            // 7. 페이드아웃 + 닫힘
            await canvasGroup.DOFade(0f, 0.25f).AsyncWaitForCompletion().AsUniTask();
            gameObject.SetActive(false);
        }

        private void SetConfirmControlsActive(bool active)
        {
            confirmButton.gameObject.SetActive(active);
            closeButton.gameObject.SetActive(active);
            rewardIconButton.interactable = active;
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
        /// 보상 아이콘 또는 확인 버튼 클릭 -> true(인벤토리로 진입).
        /// X(닫기) 버튼 클릭 -> false(그냥 닫기, 인벤토리 안 봄).
        /// </summary>
        private async UniTask<bool> WaitForProceedOrSkip()
        {
            var tcs = new UniTaskCompletionSource<bool>();
            void OnProceed() => tcs.TrySetResult(true);
            void OnSkip() => tcs.TrySetResult(false);

            confirmButton.onClick.AddListener(OnProceed);
            rewardIconButton.onClick.AddListener(OnProceed);
            closeButton.onClick.AddListener(OnSkip);

            bool result = await tcs.Task;

            confirmButton.onClick.RemoveListener(OnProceed);
            rewardIconButton.onClick.RemoveListener(OnProceed);
            closeButton.onClick.RemoveListener(OnSkip);

            return result;
        }
    }
}