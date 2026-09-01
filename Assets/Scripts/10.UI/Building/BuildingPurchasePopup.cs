using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace BlueMarble
{
    /// <summary>
    /// "태엽 북돌이" 건물 구매 팝업 화면을 코드만으로 조립하는 에디터 툴.
    /// 빈 GameObject에 붙인 뒤 컴포넌트 우클릭 메뉴로 실행한다.
    /// 게임 로직은 전혀 연결하지 않으며, 화면(뼈대)만 생성한다.
    /// </summary>
    public class BuildingPurchasePopup : MonoBehaviour
    {
#if UNITY_EDITOR

        #region 필드 (인스펙터에서 교체할 스프라이트 슬롯)

        [Header("공통")]
        [SerializeField] private TMP_FontAsset font; // 모든 텍스트에 공통 적용할 한글 폰트

        [Header("아이콘 스프라이트 슬롯 (직접 채워 넣을 것)")]
        [SerializeField] private Sprite closeIconSprite;   // 닫기(X) 버튼 아이콘
        [SerializeField] private Sprite landIconSprite;     // 땅값 카드 아이콘
        [SerializeField] private Sprite floor1IconSprite;   // 1층 별장 카드 아이콘
        [SerializeField] private Sprite floor2IconSprite;   // 2층 별장 카드 아이콘
        [SerializeField] private Sprite floor3IconSprite;   // 3층 별장 카드 아이콘
        [SerializeField] private Sprite coinIconSprite;     // 구매 버튼 코인 아이콘

        private const string RootName = "BuildingPurchasePopup";

        // 기준 해상도
        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        #endregion


        #region 진입점 (Build / Clear)

        [ContextMenu("1. Build UI")]
        private void BuildUI()
        {
            // 이미 루트가 있으면 중복 생성 방지
            if (GameObject.Find(RootName) != null)
            {
                Debug.LogWarning($"[{RootName}] 이미 씬에 존재합니다. 중복 생성을 막기 위해 중단합니다. " +
                                  "먼저 '2. Clear UI'로 지운 뒤 다시 시도하세요.");
                return;
            }

            // 씬에 EventSystem이 없으면 경고만 남기고 계속 진행
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                Debug.LogWarning("씬에 EventSystem이 없습니다. 버튼 입력을 받으려면 EventSystem을 추가하세요.");
            }

            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create BuildingPurchasePopup");

            Transform canvasTransform = CreateCanvas(root.transform);

            CreateDimBackground(canvasTransform);
            Transform popupFrame = CreatePopupFrame(canvasTransform);

            CreateTitleBar(popupFrame);
            CreateCardArea(popupFrame);
            CreateCostInfoArea(popupFrame);
            CreatePurchaseButton(popupFrame);
            CreateTollRow(popupFrame);

            EditorSceneManager.MarkSceneDirty(gameObject.scene);
            Debug.Log($"[{RootName}] 생성 완료.");
        }

        [ContextMenu("2. Clear UI")]
        private void ClearUI()
        {
            GameObject root = GameObject.Find(RootName);
            if (root == null)
            {
                Debug.Log($"[{RootName}] 씬에 해당 오브젝트가 없습니다. 지울 것이 없습니다.");
                return;
            }

            Undo.DestroyObjectImmediate(root);
            Debug.Log($"[{RootName}] 오브젝트를 지웠습니다.");
        }

        #endregion


        #region 도우미 메서드

        private GameObject NewUI(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private GameObject NewPanel(string name, Transform parent, Vector2 size)
        {
            GameObject go = NewUI(name, parent);
            Image img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.15f); // 기본 임시 색상(용도별로 바깥에서 재설정)

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            return go;
        }

        private TMP_Text NewText(string name, Transform parent, string text, float size, TextAlignmentOptions align)
        {
            GameObject go = NewUI(name, parent);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = Color.black;
            if (font != null)
            {
                tmp.font = font;
            }
            return tmp;
        }

        private Button NewButton(string name, Transform parent, string label, out TMP_Text labelText)
        {
            GameObject go = NewUI(name, parent);
            Image img = go.AddComponent<Image>();
            img.color = Color.white;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = img;

            labelText = NewText($"{name}_Label", go.transform, label, 28f, TextAlignmentOptions.Center);
            Stretch(labelText.rectTransform, 0f);

            return button;
        }

        private void Stretch(RectTransform rect, float padding)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        #endregion


        #region Canvas 생성

        private Transform CreateCanvas(Transform root)
        {
            GameObject canvasGO = NewUI("Canvas", root);

            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
            Stretch(canvasRect, 0f);

            return canvasGO.transform;
        }

        #endregion


        #region 딤드 배경

        private void CreateDimBackground(Transform canvas)
        {
            GameObject dim = NewPanel("DimBackground", canvas, Vector2.zero);
            Image img = dim.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.6f);

            RectTransform rect = dim.GetComponent<RectTransform>();
            Stretch(rect, 0f);
        }

        #endregion


        #region 팝업 프레임

        private Transform CreatePopupFrame(Transform canvas)
        {
            Vector2 frameSize = new Vector2(900f, 800f);

            GameObject frame = NewPanel("PopupFrame", canvas, frameSize);
            Image img = frame.GetComponent<Image>();
            img.color = new Color32(0xF3, 0xE3, 0xC0, 0xFF); // 크림색 배경(임시, 나중에 프레임 스프라이트로 교체 가능)

            RectTransform rect = frame.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = frameSize;

            return frame.transform;
        }

        #endregion


        #region 타이틀 바

        private void CreateTitleBar(Transform popupFrame)
        {
            GameObject titleBar = NewPanel("TitleBar", popupFrame, new Vector2(0f, 110f));
            Image img = titleBar.GetComponent<Image>();
            img.color = new Color32(0xE8, 0xC0, 0x6A, 0xFF); // 금색 리본 톤

            RectTransform rect = titleBar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(-40f, 110f);

            // 타이틀 텍스트 (내용은 비워둠 - 직접 채울 것)
            TMP_Text titleText = NewText("TitleText", titleBar.transform, "", 40f, TextAlignmentOptions.Center);
            Stretch(titleText.rectTransform, 20f);

            // 닫기 버튼 (배경 색상 블록 + 스프라이트 자리)
            GameObject closeButtonGO = NewUI("CloseButton", popupFrame);
            Image closeBg = closeButtonGO.AddComponent<Image>();
            closeBg.color = new Color(1f, 1f, 1f, 0f); // 배경은 투명 처리(아이콘만 노출)
            Button closeButton = closeButtonGO.AddComponent<Button>();
            closeButton.targetGraphic = closeBg;

            RectTransform closeRect = closeButtonGO.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-20f, -20f);
            closeRect.sizeDelta = new Vector2(70f, 70f);

            GameObject closeIconGO = NewUI("CloseIcon", closeButtonGO.transform);
            Image closeIcon = closeIconGO.AddComponent<Image>();
            closeIcon.sprite = closeIconSprite; // 비어있으면 인스펙터에서 채울 것
            closeIcon.color = closeIconSprite == null ? new Color(0.7f, 0.2f, 0.2f, 1f) : Color.white;
            Stretch(closeIconGO.GetComponent<RectTransform>(), 10f);
        }

        #endregion


        #region 카드 영역

        private void CreateCardArea(Transform popupFrame)
        {
            GameObject cardArea = NewUI("CardArea", popupFrame);
            RectTransform areaRect = cardArea.GetComponent<RectTransform>();
            areaRect.anchorMin = new Vector2(0f, 1f);
            areaRect.anchorMax = new Vector2(1f, 1f);
            areaRect.pivot = new Vector2(0.5f, 1f);
            areaRect.anchoredPosition = new Vector2(0f, -130f);
            areaRect.sizeDelta = new Vector2(-40f, 300f);

            HorizontalLayoutGroup layout = cardArea.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(0, 0, 0, 0);

            CreateCard(cardArea.transform, "Card_Land", "땅값", "필수 건설비", landIconSprite);
            CreateCard(cardArea.transform, "Card_Floor1", "1층 별장", null, floor1IconSprite);
            CreateCard(cardArea.transform, "Card_Floor2", "2층 별장", null, floor2IconSprite);
            CreateCard(cardArea.transform, "Card_Floor3", "3층 별장", null, floor3IconSprite);
        }

        private void CreateCard(Transform parent, string name, string headerLabel, string subTag, Sprite iconSprite)
        {
            GameObject card = NewPanel(name, parent, new Vector2(195f, 300f));
            Image cardBg = card.GetComponent<Image>();
            cardBg.color = new Color32(0xFD, 0xF6, 0xE3, 0xFF);

            // 카드 자체는 레이아웃 그룹이 크기를 통제하므로 LayoutElement로 선호 크기만 지정
            LayoutElement layoutElement = card.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 195f;
            layoutElement.preferredHeight = 300f;

            // 헤더 라벨 (상단 고정)
            TMP_Text header = NewText($"{name}_HeaderText", card.transform, headerLabel, 24f, TextAlignmentOptions.Center);
            RectTransform headerRect = header.rectTransform;
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, 0f);
            headerRect.sizeDelta = new Vector2(0f, 40f);

            // 서브 태그 (땅값 카드에만 존재)
            float iconTopOffset = 45f;
            if (!string.IsNullOrEmpty(subTag))
            {
                TMP_Text subTagText = NewText($"{name}_SubTagText", card.transform, subTag, 16f, TextAlignmentOptions.Center);
                RectTransform subTagRect = subTagText.rectTransform;
                subTagRect.anchorMin = new Vector2(0f, 1f);
                subTagRect.anchorMax = new Vector2(1f, 1f);
                subTagRect.pivot = new Vector2(0.5f, 1f);
                subTagRect.anchoredPosition = new Vector2(0f, -40f);
                subTagRect.sizeDelta = new Vector2(0f, 24f);
                iconTopOffset = 70f;
            }

            // 아이콘 슬롯 (색상 블록 + 스프라이트 자리)
            GameObject iconSlotGO = NewUI($"{name}_IconSlot", card.transform);
            Image iconImg = iconSlotGO.AddComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.color = iconSprite == null ? new Color(0.6f, 0.8f, 0.6f, 1f) : Color.white;

            RectTransform iconRect = iconSlotGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.anchoredPosition = new Vector2(0f, -iconTopOffset);
            iconRect.sizeDelta = new Vector2(120f, 120f);

            // 가격 텍스트 (하단 고정, 값은 비워둠)
            TMP_Text priceText = NewText($"{name}_PriceText", card.transform, "", 22f, TextAlignmentOptions.Center);
            RectTransform priceRect = priceText.rectTransform;
            priceRect.anchorMin = new Vector2(0f, 0f);
            priceRect.anchorMax = new Vector2(1f, 0f);
            priceRect.pivot = new Vector2(0.5f, 0f);
            priceRect.anchoredPosition = new Vector2(0f, 15f);
            priceRect.sizeDelta = new Vector2(0f, 36f);
            priceText.color = new Color32(0xB5, 0x2A, 0x2A, 0xFF);
        }

        #endregion


        #region 비용 정보 영역

        private void CreateCostInfoArea(Transform popupFrame)
        {
            GameObject costArea = NewUI("CostInfoArea", popupFrame);
            RectTransform areaRect = costArea.GetComponent<RectTransform>();
            areaRect.anchorMin = new Vector2(0f, 1f);
            areaRect.anchorMax = new Vector2(1f, 1f);
            areaRect.pivot = new Vector2(0.5f, 1f);
            areaRect.anchoredPosition = new Vector2(0f, -450f);
            areaRect.sizeDelta = new Vector2(-40f, 100f);

            VerticalLayoutGroup layout = costArea.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            CreateInfoRow(costArea.transform, "BuildCostRow", "건설 비용");
            CreateInfoRow(costArea.transform, "DiscountRow", "건설비용할인");
        }

        private void CreateInfoRow(Transform parent, string name, string label)
        {
            GameObject row = NewUI(name, parent);
            LayoutElement le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 40f;

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleCenter;

            TMP_Text labelText = NewText($"{name}_LabelText", row.transform, label, 22f, TextAlignmentOptions.Right);
            labelText.rectTransform.sizeDelta = new Vector2(200f, 40f);
            LayoutElement labelLE = labelText.gameObject.AddComponent<LayoutElement>();
            labelLE.preferredWidth = 200f;

            TMP_Text arrowText = NewText($"{name}_ArrowText", row.transform, "->", 22f, TextAlignmentOptions.Center);
            LayoutElement arrowLE = arrowText.gameObject.AddComponent<LayoutElement>();
            arrowLE.preferredWidth = 40f;

            // 값 텍스트 (내용은 비워둠)
            TMP_Text valueText = NewText($"{name}_ValueText", row.transform, "", 22f, TextAlignmentOptions.Left);
            LayoutElement valueLE = valueText.gameObject.AddComponent<LayoutElement>();
            valueLE.preferredWidth = 300f;
        }

        #endregion


        #region 구매 버튼

        private void CreatePurchaseButton(Transform popupFrame)
        {
            GameObject buttonGO = NewUI("PurchaseButton", popupFrame);
            Image bg = buttonGO.AddComponent<Image>();
            bg.color = new Color32(0x2C, 0x3E, 0x50, 0xFF); // 네이비 톤 (임시)

            Button button = buttonGO.AddComponent<Button>();
            button.targetGraphic = bg;

            RectTransform rect = buttonGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -570f);
            rect.sizeDelta = new Vector2(-40f, 90f);

            // "구매" 라벨 (좌측)
            TMP_Text labelText = NewText("PurchaseButton_LabelText", buttonGO.transform, "구매", 28f, TextAlignmentOptions.Center);
            labelText.color = Color.white;
            RectTransform labelRect = labelText.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0f, 1f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(30f, 0f);
            labelRect.sizeDelta = new Vector2(100f, 0f);

            // 코인 아이콘 (중앙 우측)
            GameObject coinGO = NewUI("CoinIcon", buttonGO.transform);
            Image coinImg = coinGO.AddComponent<Image>();
            coinImg.sprite = coinIconSprite;
            coinImg.color = coinIconSprite == null ? new Color(0.9f, 0.75f, 0.2f, 1f) : Color.white;

            RectTransform coinRect = coinGO.GetComponent<RectTransform>();
            coinRect.anchorMin = new Vector2(1f, 0.5f);
            coinRect.anchorMax = new Vector2(1f, 0.5f);
            coinRect.pivot = new Vector2(1f, 0.5f);
            coinRect.anchoredPosition = new Vector2(-160f, 0f);
            coinRect.sizeDelta = new Vector2(44f, 44f);

            // 가격 텍스트 (우측, 값은 비워둠)
            TMP_Text priceText = NewText("PurchaseButton_PriceText", buttonGO.transform, "", 28f, TextAlignmentOptions.Right);
            priceText.color = Color.white;
            RectTransform priceRect = priceText.rectTransform;
            priceRect.anchorMin = new Vector2(1f, 0f);
            priceRect.anchorMax = new Vector2(1f, 1f);
            priceRect.pivot = new Vector2(1f, 0.5f);
            priceRect.anchoredPosition = new Vector2(-30f, 0f);
            priceRect.sizeDelta = new Vector2(200f, 0f);
        }

        #endregion


        #region 통행료 영역

        private void CreateTollRow(Transform popupFrame)
        {
            GameObject row = NewUI("TollRow", popupFrame);
            RectTransform rect = row.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -680f);
            rect.sizeDelta = new Vector2(-40f, 50f);

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            TMP_Text labelText = NewText("TollRow_LabelText", row.transform, "통행료", 22f, TextAlignmentOptions.Right);
            LayoutElement labelLE = labelText.gameObject.AddComponent<LayoutElement>();
            labelLE.preferredWidth = 200f;

            TMP_Text arrowText = NewText("TollRow_ArrowText", row.transform, "->", 22f, TextAlignmentOptions.Center);
            LayoutElement arrowLE = arrowText.gameObject.AddComponent<LayoutElement>();
            arrowLE.preferredWidth = 40f;

            // 값 텍스트 (내용은 비워둠)
            TMP_Text valueText = NewText("TollRow_ValueText", row.transform, "", 22f, TextAlignmentOptions.Left);
            LayoutElement valueLE = valueText.gameObject.AddComponent<LayoutElement>();
            valueLE.preferredWidth = 300f;
        }

        #endregion

#endif
    }
}