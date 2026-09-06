using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace BlueMarble
{
    /// <summary>
    /// 보물 보상 팝업. 등장 -> 자동 오픈 연출 -> VFX+보상 동시 표시 -> 잠깐 보여준 뒤 자동으로 인벤토리 오픈 -> 닫힘.
    /// 확인/X 버튼 없이 전부 자동으로 진행된다. 인벤토리는 자체 X 버튼으로 닫는다.
    /// </summary>
    public class TreasureRewardPopupController : MonoBehaviour
    {
        private static readonly int OpenTrigger = Animator.StringToHash("Open");

        [Header("Chest")]
        [SerializeField] private Animator chestAnimator;
        [SerializeField] private float chestOpenDuration = 0.6f; // treasure 클립 재생시간(Speed 0.6 기준)
        [SerializeField] private float chestShakeDuration = 1.2f;
        [SerializeField] private float chestShakeStrength = 20f;
        [SerializeField] private int chestBounceCount = 3;

        [Header("VFX")]
        [SerializeField] private TreasureVfxPlayer vfxPlayer;

        [Header("Reward Display")]
        [SerializeField] private Image rewardIconImage;
        [SerializeField] private TMP_Text rewardNameText;
        [SerializeField] private float rewardDisplayDuration = 1.5f; // 보상 보여준 뒤 인벤토리로 넘어가기까지 대기 시간

        [Header("Inventory")]
        [SerializeField] private InventoryUIController inventoryPresenter;

        private bool isShowing;

        public async UniTask ShowRewardAsync(EquipmentDef reward)
        {
            if (isShowing)
            {
                Debug.Log("[TreasureRewardPopupController] 이미 팝업이 진행 중이라 무시합니다.");
                return;
            }

            isShowing = true;
            try
            {
                await ShowRewardInternalAsync(reward);
            }
            finally
            {
                isShowing = false;
            }
        }

        private async UniTask ShowRewardInternalAsync(EquipmentDef reward)
        {
            gameObject.SetActive(true);

            // 1. 상자 흔들림 (오픈 전 기대감 연출) - 정확히 N번 통통 튀는 연출
            var chestRect = (RectTransform)chestAnimator.transform;
            Vector2 originalPos = chestRect.anchoredPosition;

            float singleBounceDuration = chestShakeDuration / (chestBounceCount * 2f);

            Sequence bounceSequence = DOTween.Sequence();
            for (int i = 0; i < chestBounceCount; i++)
            {
                bounceSequence.Append(chestRect.DOAnchorPosY(originalPos.y + chestShakeStrength, singleBounceDuration).SetEase(Ease.OutQuad));
                bounceSequence.Append(chestRect.DOAnchorPosY(originalPos.y, singleBounceDuration).SetEase(Ease.InQuad));
            }

            await UniTask.Delay(TimeSpan.FromSeconds(chestShakeDuration));

            // 2. 상자 열림 연출 (자동)
            chestAnimator.SetTrigger(OpenTrigger);
            await UniTask.Delay(TimeSpan.FromSeconds(chestOpenDuration));

            // 3. 폭죽 VFX + 보상 정보를 동시에 시작 (하나가 다른 하나를 기다리지 않도록)
            UniTask vfxTask = vfxPlayer.PlayAsync(reward.Grade);
            ApplyRewardInfo(reward);
            await vfxTask;

            // 4. 보상을 잠깐 보여준 뒤, 별도 확인 없이 자동으로 인벤토리로 진입
            await UniTask.Delay(TimeSpan.FromSeconds(rewardDisplayDuration));
            await inventoryPresenter.ShowAsync();

            // 5. 닫힘
            gameObject.SetActive(false);
        }

        private void ApplyRewardInfo(EquipmentDef reward)
        {
            rewardNameText.text = $"{reward.Name} 획득!";

            rewardIconImage.gameObject.SetActive(true);
            rewardIconImage.sprite = null;

            if (string.IsNullOrEmpty(reward.IconKey))
            {
                Debug.Log("[TreasureRewardPopupController] IconKey가 비어있어 아이콘 없이 표시합니다.");
                return;
            }

            Sprite icon = Resources.Load<Sprite>(reward.IconKey);
            if (icon == null)
            {
                Debug.Log($"[TreasureRewardPopupController] 아이콘을 찾을 수 없습니다: {reward.IconKey}");
                return;
            }

            rewardIconImage.sprite = icon;
        }
    }
}