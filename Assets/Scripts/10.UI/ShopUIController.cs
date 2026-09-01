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

        private class InventorySlotRefs
        {
            public Image iconImage;
            public Button button;
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
        [SerializeField] private GameObject inventoryPanelGo;

        [Header("등급별 아이콘 색상 (임시)")]
        [SerializeField] private Color commonColor = new Color(0.6f, 0.6f, 0.65f, 1f);
        [SerializeField] private Color rareColor = new Color(0.3f, 0.55f, 0.9f, 1f);
        [SerializeField] private Color legendaryColor = new Color(0.9f, 0.65f, 0.15f, 1f);

        private readonly List<ShopCardRefs> _cards = new List<ShopCardRefs>();
        private readonly List<InventorySlotRefs> _inventorySlots = new List<InventorySlotRefs>();

        private InventoryManager myinventory = InventoryManager.Instance;
        private SlotReplaceConfirmPopup _slotReplacePopup;

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
            CacheInventorySlotRefs();
            _slotReplacePopup = new SlotReplaceConfirmPopup(transform.parent);
        }

        private void OnEnable()
        {
            // ShopPanel이 SetActive(true) 될 때마다(=상점 열릴 때마다) 자동으로 최신 상태로 갱신
            // InventoryPanel은 이제 ShopPanel의 자식이라 따로 켜고 끌 필요가 없다.
            if (gold != null && inventory != null)
            {
                RefreshAll();
            }
        }

        private void OnDisable()
        {
            // ShopPanel(자기 자신)이 꺼지는 시점 = 상점 세션 종료.
            // 이제 단일 패널이라, 여기서 바로 완료시키면 된다.
            _shopClosedTcs?.TrySetResult();
            _shopClosedTcs = null;
        }

        #endregion

        #region IShopOpener 구현

        private UniTaskCompletionSource _shopClosedTcs;

        /// <summary>
        /// SpecialTileService 등 게임 로직에서 호출한다.
        /// ShopPanel 전체(헤더+아이템목록+인벤토리)를 한 번에 연다.
        /// 반환된 UniTask는 이 패널이 닫힐 때(OnDisable) 완료된다.
        /// </summary>
        public UniTask OpenShopAsync(string playerId)
        {
            // TODO: 지금은 Init된 gold/inventory를 그대로 쓴다.
            // 실제 멀티플레이에서는 playerId로 해당 플레이어의 gold/inventory를 찾아서
            // Init을 다시 호출하는 등의 처리가 필요할 수 있다.
            Debug.Log($"[ShopUIController] {playerId}: 상점 오픈 (게임 로직에서 호출됨)");

            _shopClosedTcs = new UniTaskCompletionSource();
            gameObject.SetActive(true);

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

            if (currentGoldText == null)
            {
                // 이제 골드 표시가 ShopPanel 자신의 헤더 안에 있다.
                Transform found = transform.Find("ShopHeader/CurrentGoldPanel/CurrentGoldText");
                currentGoldText = found != null ? found.GetComponent<TMP_Text>() : null;
            }

            if (inventoryPanelGo == null)
            {
                // InventoryPanel도 이제 ShopPanel 자신의 자식이다.
                Transform found = transform.Find("InventoryPanel");
                inventoryPanelGo = found != null ? found.gameObject : null;
            }

            if (itemListArea == null)
            {
                Debug.LogWarning("[ShopUIController] ItemListArea를 찾지 못했습니다. StoreUIBuilder로 먼저 화면을 생성했는지 확인하세요.");
            }

            if (currentGoldText == null)
            {
                Debug.LogWarning("[ShopUIController] CurrentGoldText를 찾지 못했습니다. 인스펙터에서 직접 연결해주세요.");
            }

            if (inventoryPanelGo == null)
            {
                Debug.LogWarning("[ShopUIController] InventoryPanel을 찾지 못했습니다. 인스펙터에서 직접 연결해주세요.");
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

        private void CacheInventorySlotRefs()
        {
            _inventorySlots.Clear();
            if (inventoryPanelGo == null) return;

            Transform grid = inventoryPanelGo.transform.Find("InventorySlotGrid");
            if (grid == null) return;

            for (int i = 0; i < InventoryManager.MaxSlotCount; i++)
            {
                Transform slot = grid.Find($"InventorySlot_{i}");
                if (slot == null) continue;

                InventorySlotRefs refs = new InventorySlotRefs
                {
                    iconImage = slot.Find("IconImage")?.GetComponent<Image>(),
                    button = slot.GetComponent<Button>(),
                };

                _inventorySlots.Add(refs);

                if (refs.button != null)
                {
                    int capturedIndex = i; // 클로저 캡처 주의
                    refs.button.onClick.RemoveAllListeners();
                    refs.button.onClick.AddListener(() => OnInventorySlotClicked(capturedIndex));
                }
            }
        }

     

        #region 갱신

        private void RefreshAll()
        {
            RefreshShopList();
            RefreshGoldText();
            RefreshInventoryDisplay();
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

                    // 교체 대기 중에는 먼저 처리해야 하므로 구매 버튼을 잠근다.
                    card.buyButton.interactable = inventory == null || !inventory.IsWaitingReplace;
                }
            }
        }

        private void RefreshInventoryDisplay()
        {
            if (inventory == null) return;

            for (int i = 0; i < _inventorySlots.Count; i++)
            {
                Image iconImage = _inventorySlots[i].iconImage;
                if (iconImage == null) continue;

                EquipmentInstance instance = inventory.GetAt(i);

                if (instance == null)
                {
                    iconImage.sprite = null;
                    iconImage.color = new Color(1f, 1f, 1f, 0f); // 빈 칸 - 투명
                    continue;
                }

                EquipmentDef def = instance.Def;
                Sprite icon = string.IsNullOrEmpty(def.IconKey) ? null : Resources.Load<Sprite>(def.IconKey);

                if (icon != null)
                {
                    iconImage.sprite = icon;
                    iconImage.color = Color.white;
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.color = GetGradeColor(def.Grade);
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
            RefreshInventoryDisplay();
            RefreshShopList(); // 인벤토리가 꽉 찼으면 여기서 구매 버튼들이 잠긴다.

            if (inventory.IsWaitingReplace)
            {
                Debug.Log("[ShopUIController] 인벤토리가 가득 찼습니다. 인벤토리 칸을 클릭해서 교체할 장비를 선택하세요.");
            }
            else
            {
                Debug.Log("[ShopUIController] 구매 완료.");
            }
        }

        /// <summary>
        /// 인벤토리 1~5번 칸 클릭 처리.
        /// 교체 대기 중이 아니면 아무 일도 하지 않는다.
        /// 빈 칸이면 확인 없이 바로 배치하고, 차있는 칸이면 Yes/No 확인 팝업을 띄운다.
        /// </summary>
        private void OnInventorySlotClicked(int slotIndex)
        {
            if (inventory == null || !inventory.IsWaitingReplace) return;

            EquipmentInstance current = inventory.GetAt(slotIndex);
            EquipmentDef pendingDef = inventory.ReplaceDef;

            if (current == null)
            {
                // 빈 칸 - 확인 없이 바로 배치
                inventory.ReplaceAt(slotIndex);
                OnReplaceResolved();
            }
            else
            {
                // 차있는 칸 - 교체 확인 팝업
                _slotReplacePopup.Show(current.Def.Name, pendingDef.Name,
                    onYes: () =>
                    {
                        inventory.ReplaceAt(slotIndex);
                        _slotReplacePopup.Hide();
                        OnReplaceResolved();
                    },
                    onNo: () => _slotReplacePopup.Hide());
            }
        }

        private void OnReplaceResolved()
        {
            RefreshInventoryDisplay();
            RefreshShopList(); // 교체가 끝났으니 구매 버튼 다시 활성화
        }

        #endregion
    }
}