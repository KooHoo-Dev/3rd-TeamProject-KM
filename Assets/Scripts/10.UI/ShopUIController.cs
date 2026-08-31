using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BlueMarble
{
    /// <summary>
    /// StoreUIBuilder가 만든 상점 화면(ShopPanel)에 부착해서 사용한다.
    /// ShopService / IGold / InventoryManager와 실제로 연결해서
    /// 목록 표시, 구매, 인벤토리 초과 시 교체 팝업까지 처리한다.
    ///
    /// 사용 순서:
    /// 1. StoreUIBuilder로 "Store UI"를 먼저 빌드한다.
    /// 2. "ShopPanel" 오브젝트에 이 컴포넌트를 붙인다.
    /// 3. 턴/게임 매니저 쪽에서 Init(gold, inventory)를 한 번 호출해서 의존성을 주입한다.
    ///    (또는 테스트용으로 컴포넌트 우클릭 → "테스트용 더미 데이터로 초기화")
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopUIController : MonoBehaviour
    {
        #region 카드 참조 캐시용 구조체

        private class ShopCardRefs
        {
            public GameObject root;
            public Image iconImage;
            public TMP_Text nameText;
            public TMP_Text priceText;
            public Button buyButton;
            public GameObject tooltipPanel;
            public TMP_Text effectText;
            public TMP_Text cooldownText;
            public TMP_Text durabilityText;
        }

        #endregion

        #region 필드

        [Header("의존성 (Init으로 주입됨)")]
        private ShopService shopService;
        private IGold gold;
        private InventoryManager inventory;

        [Header("씬 참조 - 비워두면 이름으로 자동 탐색")]
        [SerializeField] private Transform itemListArea;
        [SerializeField] private TMP_Text currentGoldText;

        [Header("등급별 아이콘 색상 (임시)")]
        [SerializeField] private Color commonColor = new Color(0.6f, 0.6f, 0.65f, 1f);
        [SerializeField] private Color rareColor = new Color(0.3f, 0.55f, 0.9f, 1f);
        [SerializeField] private Color legendaryColor = new Color(0.9f, 0.65f, 0.15f, 1f);

        private readonly List<ShopCardRefs> _cards = new List<ShopCardRefs>();

        // 교체 팝업 (씬에 없으면 런타임에 직접 만든다)
        private GameObject _replacePopupRoot;
        private TMP_Text _newItemInfoText;
        private readonly List<Button> _replaceSlotButtons = new List<Button>();
        private readonly List<TMP_Text> _replaceSlotLabels = new List<TMP_Text>();
        private Button _replaceCancelButton;

        #endregion

        #region 초기화

        /// <summary>
        /// 외부(턴/게임 매니저)에서 플레이어의 골드/인벤토리를 주입한다.
        /// </summary>
        public void Init(IGold playerGold, InventoryManager playerInventory)
        {
            gold = playerGold;
            inventory = playerInventory;
            shopService ??= new ShopService();

            if (gameObject.activeInHierarchy)
            {
                RefreshAll();
            }
        }

        [ContextMenu("테스트용 더미 데이터로 초기화")]
        private void InitWithDummyData()
        {
            Init(new Gold(500), new InventoryManager());
        }

        private void Awake()
        {
            shopService ??= new ShopService();
            CacheSceneReferences();
            CacheCardRefs();
            EnsureReplacePopup();
        }

        private void OnEnable()
        {
            // ShopPanel이 SetActive(true) 될 때마다(=상점 열릴 때마다) 자동으로 최신 상태로 갱신
            if (gold != null && inventory != null)
            {
                RefreshAll();
            }
        }

        private void CacheSceneReferences()
        {
            if (itemListArea == null)
            {
                Transform found = transform.Find("ItemListArea");
                itemListArea = found;
            }

            if (currentGoldText == null)
            {
                // ShopPanel과 ShopEntryPanel은 같은 Canvas("Store UI")의 형제 오브젝트다.
                Transform canvasRoot = transform.parent;
                Transform found = canvasRoot != null
                    ? canvasRoot.Find("ShopEntryPanel/CurrentGoldPanel/CurrentGoldText")
                    : null;
                currentGoldText = found != null ? found.GetComponent<TMP_Text>() : null;
            }

            if (itemListArea == null)
            {
                Debug.LogWarning("[ShopUIController] ItemListArea를 찾지 못했습니다. StoreUIBuilder로 먼저 화면을 생성했는지 확인하세요.");
            }

            if (currentGoldText == null)
            {
                Debug.LogWarning("[ShopUIController] CurrentGoldText를 찾지 못했습니다. 인스펙터에서 직접 연결해주세요.");
            }
        }

        private void CacheCardRefs()
        {
            _cards.Clear();
            if (itemListArea == null) return;

            for (int i = 0; i < itemListArea.childCount; i++)
            {
                Transform card = itemListArea.GetChild(i);

                ShopCardRefs refs = new ShopCardRefs
                {
                    root = card.gameObject,
                    iconImage = card.Find("IconImage")?.GetComponent<Image>(),
                    nameText = card.Find("NameText")?.GetComponent<TMP_Text>(),
                    priceText = card.Find("PriceRow/PriceText")?.GetComponent<TMP_Text>(),
                    buyButton = card.Find("BuyButton")?.GetComponent<Button>(),
                    tooltipPanel = card.Find("TooltipPanel")?.gameObject,
                    effectText = card.Find("TooltipPanel/EffectText")?.GetComponent<TMP_Text>(),
                    cooldownText = card.Find("TooltipPanel/CooldownText")?.GetComponent<TMP_Text>(),
                    durabilityText = card.Find("TooltipPanel/DurabilityText")?.GetComponent<TMP_Text>(),
                };

                _cards.Add(refs);
            }
        }

        #endregion

        #region 갱신

        private void RefreshAll()
        {
            RefreshShopList();
            RefreshGoldText();
        }

        private void RefreshShopList()
        {
            IReadOnlyList<EquipmentDef> list = shopService.GetShopList();

            for (int i = 0; i < _cards.Count; i++)
            {
                ShopCardRefs card = _cards[i];

                if (i >= list.Count)
                {
                    card.root.SetActive(false);
                    continue;
                }

                card.root.SetActive(true);
                EquipmentDef def = list[i];

                if (card.nameText != null) card.nameText.text = def.Name;
                if (card.priceText != null) card.priceText.text = EquipmentDisplayFormatter.GetPriceText(def);
                if (card.effectText != null) card.effectText.text = EquipmentDisplayFormatter.GetEffectText(def);
                if (card.cooldownText != null) card.cooldownText.text = EquipmentDisplayFormatter.GetCooldownText(def);
                if (card.durabilityText != null) card.durabilityText.text = EquipmentDisplayFormatter.GetDurabilityText(def);
                if (card.iconImage != null) card.iconImage.color = GetGradeColor(def.Grade);

                if (card.buyButton != null)
                {
                    int capturedIndex = i; // 클로저 캡처 주의
                    card.buyButton.onClick.RemoveAllListeners();
                    card.buyButton.onClick.AddListener(() => OnPurchaseClicked(capturedIndex));
                }
            }
        }

        private void RefreshGoldText()
        {
            if (currentGoldText != null && gold != null)
            {
                currentGoldText.text = $"Current Gold : {gold.CurrentGold}";
            }
        }

        private Color GetGradeColor(EquipmentGrade grade)
        {
            switch (grade)
            {
                case EquipmentGrade.Rare: return rareColor;
                case EquipmentGrade.Legendary: return legendaryColor;
                default: return commonColor;
            }
        }

        #endregion

        #region 구매 흐름

        private void OnPurchaseClicked(int shopIndex)
        {
            if (gold == null || inventory == null)
            {
                Debug.LogWarning("[ShopUIController] Init(gold, inventory)이 호출되지 않았습니다.");
                return;
            }

            bool purchased = shopService.TryPurchase(shopIndex, gold, inventory);

            if (!purchased)
            {
                Debug.Log("[ShopUIController] 자금이 부족합니다.");
                return;
            }

            RefreshGoldText();

            if (inventory.IsWaitingReplace)
            {
                ShowReplacePopup(inventory.ReplaceDef);
            }
            else
            {
                Debug.Log("[ShopUIController] 구매 완료.");
            }
        }

        #endregion

        #region 교체 팝업 (씬에 없으면 런타임에 생성)

        private void EnsureReplacePopup()
        {
            Transform canvasRoot = transform.parent;
            if (canvasRoot == null) return;

            Transform existing = canvasRoot.Find("ReplacePopup");
            if (existing != null)
            {
                _replacePopupRoot = existing.gameObject;
                CacheExistingReplacePopupRefs(existing);
                _replacePopupRoot.SetActive(false);
                return;
            }

            BuildReplacePopup(canvasRoot);
        }

        private void CacheExistingReplacePopupRefs(Transform popup)
        {
            _newItemInfoText = popup.Find("NewItemInfoText")?.GetComponent<TMP_Text>();
            _replaceCancelButton = popup.Find("CancelButton")?.GetComponent<Button>();

            _replaceSlotButtons.Clear();
            _replaceSlotLabels.Clear();

            for (int i = 0; i < InventoryManager.MaxSlotCount; i++)
            {
                Transform slotBtn = popup.Find($"SlotButton_{i}");
                if (slotBtn == null) continue;

                _replaceSlotButtons.Add(slotBtn.GetComponent<Button>());
                _replaceSlotLabels.Add(slotBtn.GetComponentInChildren<TMP_Text>());
            }
        }

        // StoreUIBuilder가 ReplacePopup을 만들지 않았을 경우, 여기서 최소 구성으로 직접 만든다.
        private void BuildReplacePopup(Transform canvasRoot)
        {
            GameObject rootGo = NewRect("ReplacePopup", canvasRoot, Vector2.zero);
            RectTransform rootRect = rootGo.GetComponent<RectTransform>();
            Stretch(rootRect);
            Image dim = rootGo.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.6f);

            GameObject boxGo = NewRect("PopupBox", rootGo.transform, new Vector2(560f, 420f));
            RectTransform boxRect = boxGo.GetComponent<RectTransform>();
            CenterRect(boxRect);
            Image boxImg = boxGo.AddComponent<Image>();
            boxImg.color = new Color(0.17f, 0.18f, 0.21f, 1f);

            _newItemInfoText = CreateText("NewItemInfoText", boxGo.transform, "새 장비 정보", 26f,
                new Vector2(0f, 150f), new Vector2(500f, 60f));

            _replaceSlotButtons.Clear();
            _replaceSlotLabels.Clear();

            for (int i = 0; i < InventoryManager.MaxSlotCount; i++)
            {
                float y = 90f - i * 60f;
                GameObject slotGo = NewRect($"SlotButton_{i}", boxGo.transform, new Vector2(440f, 50f));
                RectTransform slotRect = slotGo.GetComponent<RectTransform>();
                CenterRect(slotRect);
                slotRect.anchoredPosition = new Vector2(0f, y);

                Image slotImg = slotGo.AddComponent<Image>();
                slotImg.color = new Color(0.3f, 0.32f, 0.36f, 1f);
                Button slotBtn = slotGo.AddComponent<Button>();
                slotBtn.targetGraphic = slotImg;

                TMP_Text label = CreateText($"SlotLabel_{i}", slotGo.transform, $"{i + 1}번 슬롯", 22f, Vector2.zero, new Vector2(420f, 50f));

                _replaceSlotButtons.Add(slotBtn);
                _replaceSlotLabels.Add(label);
            }

            GameObject cancelGo = NewRect("CancelButton", boxGo.transform, new Vector2(200f, 60f));
            RectTransform cancelRect = cancelGo.GetComponent<RectTransform>();
            CenterRect(cancelRect);
            cancelRect.anchoredPosition = new Vector2(0f, -180f);
            Image cancelImg = cancelGo.AddComponent<Image>();
            cancelImg.color = new Color(0.45f, 0.3f, 0.3f, 1f);
            _replaceCancelButton = cancelGo.AddComponent<Button>();
            _replaceCancelButton.targetGraphic = cancelImg;
            CreateText("Label", cancelGo.transform, "포기", 24f, Vector2.zero, new Vector2(200f, 60f));

            _replacePopupRoot = rootGo;
            _replacePopupRoot.SetActive(false);
        }

        private void ShowReplacePopup(EquipmentDef newDef)
        {
            if (_newItemInfoText != null)
            {
                _newItemInfoText.text = $"{newDef.Name} 획득!\n{EquipmentDisplayFormatter.GetEffectText(newDef)}";
            }

            for (int i = 0; i < _replaceSlotButtons.Count; i++)
            {
                int capturedIndex = i;
                EquipmentInstance current = inventory.GetAt(i);

                if (i < _replaceSlotLabels.Count && _replaceSlotLabels[i] != null)
                {
                    _replaceSlotLabels[i].text = current != null
                        ? $"{i + 1}번 슬롯 - {current.Def.Name} (교체)"
                        : $"{i + 1}번 슬롯 - 비어있음";
                }

                _replaceSlotButtons[i].onClick.RemoveAllListeners();
                _replaceSlotButtons[i].onClick.AddListener(() => OnReplaceSlotSelected(capturedIndex));
            }

            if (_replaceCancelButton != null)
            {
                _replaceCancelButton.onClick.RemoveAllListeners();
                _replaceCancelButton.onClick.AddListener(OnReplaceCancelClicked);
            }

            _replacePopupRoot.SetActive(true);
        }

        private void OnReplaceSlotSelected(int slotIndex)
        {
            inventory.ReplaceAt(slotIndex);
            _replacePopupRoot.SetActive(false);
        }

        private void OnReplaceCancelClicked()
        {
            inventory.CancelReplace();
            _replacePopupRoot.SetActive(false);
        }

        #endregion

        #region UI 생성 도우미 (교체 팝업 전용 최소 버전)

        private GameObject NewRect(string name, Transform parent, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = size;
            return go;
        }

        private void CenterRect(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private TMP_Text CreateText(string name, Transform parent, string text, float fontSize, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject go = NewRect(name, parent, size);
            RectTransform rect = go.GetComponent<RectTransform>();
            CenterRect(rect);
            rect.anchoredPosition = anchoredPosition;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return tmp;
        }

        #endregion
    }
}