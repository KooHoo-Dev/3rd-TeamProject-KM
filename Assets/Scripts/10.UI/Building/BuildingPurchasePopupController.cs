using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Epic3;
using Study.Utilities;
using UnityEngine.InputSystem;

namespace BlueMarble
{
    /// <summary>
    /// BuildingPurchasePopup이 생성한 화면을 PropertyManager(Epic3)와 연결하는 런타임 컨트롤러.
    ///
    /// - PropertyManager.OnShowPurchaseUI(TileState, int tilePrice) 구독 → 팝업 표시 + 가격 반영
    /// - 구매 버튼 클릭 → SubmitPurchaseDecision(true, buyBuilding, buyLandmark) 호출
    /// - 닫기(X) 버튼 클릭 → SubmitPurchaseDecision(false, false, false) 호출 (전부 포기 처리)
    ///
    /// 타 팀원 소유 파일(PropertyManager.cs)은 직접 수정하지 않고, 이 컨트롤러만 새로 추가해서 연결한다.
    /// 씬에 배치 후 인스펙터에서 아래 필드를 BuildingPurchasePopup이 생성한 오브젝트에 직접 연결할 것.
    /// </summary>
    public class BuildingPurchasePopupController : MonoBehaviour
    {
        [Header("팝업 표시/숨김 대상 (예: PopupFrame의 상위 Canvas 또는 PopupFrame 자체)")]
        [SerializeField] private GameObject popupRoot;

        [Header("가격 표시 텍스트 (PurchaseButton_PriceText 연결)")]
        [SerializeField] private TMP_Text priceText;

        [Header("체크박스 (Card_Building_CheckboxToggle / Card_Landmark_CheckboxToggle 연결)")]
        [SerializeField] private Toggle buildingToggle;
        [SerializeField] private Toggle landmarkToggle;

        [Header("버튼 (PurchaseButton / CloseButton 연결)")]
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button closeButton;
        


        private void OnEnable()
        {
            if (PropertyManager.Instance != null)
            {
                PropertyManager.Instance.OnShowPurchaseUI += HandleShowPurchaseUI;
            }

            if (purchaseButton != null) purchaseButton.onClick.AddListener(OnPurchaseClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);

            HidePopup();
        }

        private void OnDisable()
        {
            if (PropertyManager.Instance != null)
            {
                PropertyManager.Instance.OnShowPurchaseUI -= HandleShowPurchaseUI;
            }

            if (purchaseButton != null) purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
            if (closeButton != null) closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        /// <summary>
        /// PropertyManager가 팝업을 띄우라고 알려올 때 호출된다.
        /// TileState 자체는 이번 화면에서 별도로 쓰지 않고, 가격만 표시한다.
        /// </summary>
        private void HandleShowPurchaseUI(TileState state, int tilePrice)
        {
            ResetToggles();

            if (priceText != null)
            {
                priceText.text = $"{tilePrice:N0}";
            }

            ShowPopup();
        }

        private void OnPurchaseClicked()
        {
            bool buyBuilding = buildingToggle != null && buildingToggle.isOn;
            bool buyLandmark = landmarkToggle != null && landmarkToggle.isOn;

            // 영토(buyTile)는 항상 필수이므로 true 고정
            PropertyManager.Instance?.SubmitPurchaseDecision(true, buyBuilding, buyLandmark);
            HidePopup();
        }

        private void OnCloseClicked()
        {
            // 전부 포기 처리
            PropertyManager.Instance?.SubmitPurchaseDecision(false, false, false);
            HidePopup();
        }

        private void ResetToggles()
        {
            if (buildingToggle != null) buildingToggle.SetIsOnWithoutNotify(false);
            if (landmarkToggle != null) landmarkToggle.SetIsOnWithoutNotify(false);
        }

        private void ShowPopup()
        {
            if (popupRoot != null) popupRoot.SetActive(true);
        }

        private void HidePopup()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
        }
    }
}