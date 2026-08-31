using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Cysharp.Threading.Tasks;

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
    public class ShopUIController : MonoBehaviour, IShopOpener
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
        private InventoryManager myinventory = InventoryManager.Instance;

        [Header("씬 참조 - 비워두면 이름으로 자동 탐색")]
        [SerializeField] private Transform itemListArea;
        [SerializeField] private TMP_Text currentGoldText;
        [SerializeField] private GameObject shopEntryPanelGo;

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
            Init(new Gold(500), myinventory);
        }

        private void Awake()
        {
            shopService ??= new ShopService();
            CacheSceneReferences();
            CacheCardRefs();
            EnsureReplacePopup();
            WireShopEntryPanelCloseNotifier();
        }

        private void OnEnable()
        {
            // ShopPanel이 SetActive(true) 될 때마다(=상점 열릴 때마다) 자동으로 최신 상태로 갱신
            if (gold != null && inventory != null)
            {
                RefreshAll();
            }
        }

        private void WireShopEntryPanelCloseNotifier()
        {
            if (shopEntryPanelGo == null) return;

            ShopSessionCloseNotifier notifier = shopEntryPanelGo.GetComponent<ShopSessionCloseNotifier>();
            if (notifier == null)
            {
                notifier = shopEntryPanelGo.AddComponent<ShopSessionCloseNotifier>();
            }

            notifier.OnClosed = HandleShopSessionClosed;
        }

        private void HandleShopSessionClosed()
        {
            // "진짜 상점 세션 종료" = 왼쪽 진입 패널이 닫히는 시점.
            // 오른쪽 아이템 목록(ShopPanel)을 한 번도 안 열었어도 여기서 확실히 완료된다.
            _shopClosedTcs?.TrySetResult();
            _shopClosedTcs = null;
        }

        #endregion

        #region IShopOpener 구현

        private UniTaskCompletionSource _shopClosedTcs;

        /// <summary>
        /// SpecialTileService 등 게임 로직에서 호출한다.
        /// 왼쪽 진입 패널만 먼저 연다 - 사람이 "상점 열기"를 직접 눌렀을 때와 똑같이,
        /// 오른쪽 아이템 목록은 그 버튼을 눌러야 나오게 한다.
        /// 반환된 UniTask는 왼쪽 패널이 닫힐 때(=상점 세션 종료) 완료된다.
        /// </summary>
        public UniTask OpenShopAsync(string playerId)
        {
            // TODO: 지금은 Init된 gold/inventory를 그대로 쓴다.
            // 실제 멀티플레이에서는 playerId로 해당 플레이어의 gold/inventory를 찾아서
            // Init을 다시 호출하는 등의 처리가 필요할 수 있다.
            Debug.Log($"[ShopUIController] {playerId}: 상점 진입 패널 오픈 (게임 로직에서 호출됨)");

            _shopClosedTcs = new UniTaskCompletionSource();

            if (shopEntryPanelGo != null)
            {
                shopEntryPanelGo.SetActive(true);
            }
            else
            {
                // 진입 패널을 못 찾았으면, 최소한 아이템 목록이라도 바로 열어서 막히지 않게 한다.
                Debug.LogWarning("[ShopUIController] ShopEntryPanel을 못 찾아서 ShopPanel을 바로 엽니다.");
                gameObject.SetActive(true);
            }

            return _shopClosedTcs.Task;
        }

        #endregion

        private void CacheSceneReferences()
        {
            if (itemListArea == null)
            {
                Transform found = transform.Find("ItemListArea");
                itemListArea = found;
            }

            Transform canvasRootForSearch = transform.parent;

            if (shopEntryPanelGo == null)
            {
                Transform entryFound = canvasRootForSearch != null
                    ? canvasRootForSearch.Find("ShopEntryPanel")
                    : null;

                // 형제로 못 찾으면, 씬 전체에서 이름으로 한 번 더 찾아본다 (비활성 오브젝트도 포함).
                if (entryFound == null)
                {
                    GameObject globalFound = FindInactiveObjectByName("ShopEntryPanel");
                    entryFound = globalFound != null ? globalFound.transform : null;
                }

                shopEntryPanelGo = entryFound != null ? entryFound.gameObject : null;
            }

            if (currentGoldText == null)
            {
                // ShopPanel과 ShopEntryPanel은 같은 Canvas("Store UI")의 형제 오브젝트다.
                Transform found = canvasRootForSearch != null
                    ? canvasRootForSearch.Find("ShopEntryPanel/CurrentGoldPanel/CurrentGoldText")
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

            if (shopEntryPanelGo == null)
            {
                string parentName = canvasRootForSearch != null ? canvasRootForSearch.name : "(부모 없음)";
                Debug.LogWarning($"[ShopUIController] ShopEntryPanel을 찾지 못했습니다. 지금 이 오브젝트의 부모는 '{parentName}' 입니다. 하이어라키에서 ShopPanel과 ShopEntryPanel이 같은 부모 밑에 있는지 확인해주세요.");
            }
        }

        // 비활성 오브젝트까지 포함해서 씬 전체에서 이름으로 찾는다 (Resources.FindObjectsOfTypeAll 사용).
        private static GameObject FindInactiveObjectByName(string name)
        {
            Transform[] all = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (Transform t in all)
            {
                if (t.name == name && t.hideFlags == HideFlags.None && t.gameObject.scene.IsValid())
                {
                    return t.gameObject;
                }
            }

            return null;
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
                EnsureCardHoverTooltip(refs);
            }
        }

        // 호버 시 툴팁을 여닫는 연결을, 에디터가 아니라 여기(런타임)에서 직접 관리한다.
        // 실행할 때마다 새로 연결되므로, Play 모드 재진입이나 씬 재오픈에도 항상 동작한다.
        private void EnsureCardHoverTooltip(ShopCardRefs card)
        {
            if (card.tooltipPanel == null) return;

            EventTrigger trigger = card.root.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = card.root.AddComponent<EventTrigger>();
            }

            trigger.triggers.Clear(); // 예전에 남아있을 수 있는 항목 정리 후 새로 등록

            GameObject tooltip = card.tooltipPanel;

            EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(_ => tooltip.SetActive(true));
            trigger.triggers.Add(enterEntry);

            EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => tooltip.SetActive(false));
            trigger.triggers.Add(exitEntry);

            tooltip.SetActive(false); // 평소엔 숨김 상태로 리셋
        }
        
        

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
                if (card.iconImage != null)
                {
                    Sprite icon = string.IsNullOrEmpty(def.IconKey) ? null : Resources.Load<Sprite>(def.IconKey);

                    if (icon != null)
                    {
                        card.iconImage.sprite = icon;
                        card.iconImage.color = Color.white; // 실제 아이콘은 원래 색 그대로 보여준다
                    }
                    else
                    {
                        // 아이콘이 없거나 경로가 잘못됐으면, 기존처럼 등급 색상 블록으로 안전하게 대체
                        card.iconImage.color = GetGradeColor(def.Grade);
                    }
                }

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