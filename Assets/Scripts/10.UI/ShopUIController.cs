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
    [DisallowMultipleComponent] // 실수로 같은 오브젝트에 이 컴포넌트를 두 개 붙이는 것을 막는다 (참조가 꼬이기 쉬움).
    public class ShopUIController : MonoBehaviour, IShopOpener
    {
        #region 카드 참조 캐시용 구조체

        /// <summary>
        /// 상점 카드(ShopItemCard_0 ~ N) 하나에 필요한 UI 컴포넌트 참조를 모아두는 내부 클래스.
        /// 매 프레임 GetComponent/Find를 다시 하지 않도록, Awake 시점에 한 번만 찾아서 캐싱한다.
        /// </summary>
        private class ShopCardRefs
        {
            public GameObject root;          // 카드 전체 오브젝트 (활성/비활성 토글용)
            public Image iconImage;          // 장비 아이콘 이미지
            public TMP_Text nameText;        // 장비 이름
            public TMP_Text priceText;       // 가격 숫자
            public Button buyButton;         // BUY 버튼
            public GameObject tooltipPanel;  // 마우스 호버 시 뜨는 툴팁(효과/쿨타임/내구도) 전체
            public TMP_Text effectText;      // 툴팁 - 효과 설명
            public TMP_Text cooldownText;    // 툴팁 - 쿨타임
            public TMP_Text durabilityText;  // 툴팁 - 내구도
        }

        /// <summary>
        /// 인벤토리 슬롯(InventorySlot_0 ~ 4) 하나에 필요한 참조.
        /// 상점 카드보다 훨씬 단순해서(아이콘 + 클릭 버튼만) 필드가 적다.
        /// </summary>
        private class InventorySlotRefs
        {
            public Image iconImage; // 슬롯에 담긴 장비 아이콘. 비어있으면 투명 처리.
            public Button button;   // 슬롯 클릭 감지용 버튼 (교체 대기 중일 때만 의미 있는 클릭이 됨)
        }

        #endregion

        #region 필드

        [Header("의존성 (Init으로 주입됨)")]
        // 아래 세 개는 인스펙터에 노출하지 않는다 - 반드시 Init()을 통해서만 값이 채워져야 하기 때문.
        private ShopService shopService;   // 상점 목록 조회 / 구매 처리를 담당하는 순수 C# 서비스 클래스
        private IGold gold;                 // 이 상점을 이용 중인 플레이어의 골드 (인터페이스라 Mock으로도 테스트 가능)
        private InventoryManager inventory; // 이 상점을 이용 중인 플레이어의 인벤토리

        [Header("씬 참조 - 비워두면 이름으로 자동 탐색")]
        // 셋 다 StoreUIBuilder가 만든 하이어라키 이름을 기준으로 자동으로 찾아서 채워진다 (CacheSceneReferences 참고).
        // 자동 탐색이 실패하면 경고 로그가 뜨니, 그럴 땐 인스펙터에서 직접 드래그해서 연결하면 된다.
        [SerializeField] private Transform itemListArea;     // 상점 카드들이 나열되는 부모 컨테이너
        [SerializeField] private TMP_Text currentGoldText;   // 헤더에 있는 "Current Gold : n" 텍스트
        [SerializeField] private GameObject inventoryPanelGo; // 하단 인벤토리 영역 전체 (5칸 슬롯을 담고 있는 부모)

        [Header("등급별 아이콘 색상 (임시)")]
        // 실제 스프라이트(EquipmentDef.IconKey)를 못 찾았을 때 대신 보여주는 색상 블록.
        // 나중에 모든 장비에 아이콘이 준비되면 이 색상 로직 자체가 필요 없어질 수 있다.
        [SerializeField] private Color commonColor = new Color(0.6f, 0.6f, 0.65f, 1f);
        [SerializeField] private Color rareColor = new Color(0.3f, 0.55f, 0.9f, 1f);
        [SerializeField] private Color legendaryColor = new Color(0.9f, 0.65f, 0.15f, 1f);

        // 상점 카드 5개, 인벤토리 슬롯 5개의 캐싱된 참조 목록.
        // CacheCardRefs() / CacheInventorySlotRefs()에서 Awake 시점에 한 번만 채워진다.
        private readonly List<ShopCardRefs> _cards = new List<ShopCardRefs>();
        private readonly List<InventorySlotRefs> _inventorySlots = new List<InventorySlotRefs>();

        // 테스트용 더미 초기화(InitWithDummyData)에서 쓰는 인벤토리 싱글턴 참조.
        // 실제 게임 플레이에서는 Init()으로 주입되는 'inventory' 필드를 쓰고, 이 필드는 테스트 전용이다.
        private InventoryManager myinventory = InventoryManager.Instance;

        // 인벤토리 칸(차있는 칸)을 클릭했을 때 뜨는 "교체하시겠습니까?" 팝업.
        // 씬에 이미 있으면 그걸 재사용하고, 없으면 런타임에 직접 만든다 (SlotReplaceConfirmPopup 내부 로직).
        private SlotReplaceConfirmPopup _slotReplacePopup;

        #endregion

        #region 초기화

        /// <summary>
        /// 외부(턴/게임 매니저 등 실제 플레이어 상태를 들고 있는 쪽)에서
        /// 이 상점이 사용할 골드/인벤토리를 한 번 주입한다.
        /// 이미 상점이 열려있는 상태(activeInHierarchy)에서 호출되면, 화면도 바로 최신화한다.
        /// </summary>
        public void Init(IGold playerGold, InventoryManager playerInventory)
        {
            gold = playerGold;
            inventory = playerInventory;
            shopService ??= new ShopService(); // 아직 안 만들어졌으면 여기서 한 번만 생성 (널 병합 대입 연산자)

            if (gameObject.activeInHierarchy)
            {
                RefreshAll();
            }
        }

        /// <summary>
        /// 실제 게임 로직(턴 매니저 등) 없이 상점 UI만 단독으로 테스트하고 싶을 때 쓰는 진입점.
        /// 골드 500, 빈 인벤토리로 즉시 Init을 호출해준다.
        /// 컴포넌트 우클릭 → "테스트용 더미 데이터로 초기화"로 실행.
        /// </summary>
        [ContextMenu("테스트용 더미 데이터로 초기화")]
        private void InitWithDummyData()
        {
            Init(new Gold(500), myinventory);
        }

        /// <summary>
        /// 씬이 로드될 때 딱 한 번 실행된다 (오브젝트가 비활성 상태여도 실행됨).
        /// 여기서 필요한 참조들을 전부 미리 찾아 캐싱해둔다.
        /// </summary>
        private void Awake()
        {
            shopService ??= new ShopService();
            CacheSceneReferences();   // itemListArea / currentGoldText / inventoryPanelGo 자동 탐색
            CacheCardRefs();          // 상점 카드 5개 참조 캐싱 + 호버 툴팁 연결
            CacheInventorySlotRefs(); // 인벤토리 슬롯 5개 참조 캐싱 + 클릭 이벤트 연결
            _slotReplacePopup = new SlotReplaceConfirmPopup(transform.parent); // 교체 확인 팝업 준비
        }

        /// <summary>
        /// 이 ShopPanel이 SetActive(true)가 될 때마다(=상점이 열릴 때마다) 자동 호출된다.
        /// 인벤토리 패널도 이제 ShopPanel의 자식이라, 여기서 따로 켜줄 필요는 없다.
        /// </summary>
        private void OnEnable()
        {
            if (gold != null && inventory != null)
            {
                RefreshAll(); // 상점 목록/골드/인벤토리를 전부 최신 상태로 다시 그린다.
            }
        }

        /// <summary>
        /// 이 ShopPanel이 SetActive(false)가 될 때(=상점이 닫힐 때) 자동 호출된다.
        /// 지금은 패널이 하나로 통합돼있기 때문에, "패널이 꺼짐 = 상점 세션 완전히 종료"로 봐도 된다.
        /// OpenShopAsync()가 반환했던 UniTask를 여기서 완료시켜서, 보드 이동이 다시 진행되게 한다.
        /// </summary>
        private void OnDisable()
        {
            _shopClosedTcs?.TrySetResult(); // 대기 중인 태스크가 있으면 완료 처리 (없으면 null이라 안전하게 무시됨)
            _shopClosedTcs = null;
        }

        #endregion

        #region IShopOpener 구현

        // SpecialTileService가 상점을 열고 "닫힐 때까지" 기다리기 위해 쓰는 완료 신호.
        // OpenShopAsync에서 새로 만들고, OnDisable에서 완료시킨다.
        private UniTaskCompletionSource _shopClosedTcs;

        /// <summary>
        /// IShopOpener 인터페이스 구현체.
        /// SpecialTileService 같은 게임 로직 쪽에서 "플레이어가 상점 타일에 도착/통과했다"는
        /// 이벤트가 발생했을 때 이 메서드를 호출한다.
        ///
        /// 동작: ShopPanel 전체(헤더+아이템목록+인벤토리)를 한 번에 활성화하고,
        /// 반환하는 UniTask는 이 패널이 다시 비활성화될 때(OnDisable)까지 완료되지 않는다.
        /// 즉 호출하는 쪽에서 "await OpenShopAsync(...)"로 대기하면,
        /// 플레이어가 상점을 다 보고 닫을 때까지 보드 이동 로직이 자동으로 멈춰 있게 된다.
        /// </summary>
        public UniTask OpenShopAsync(string playerId)
        {
            // TODO: 지금은 Init된 gold/inventory를 그대로 쓴다 (플레이어가 한 명이라는 전제).
            // 실제 멀티플레이에서는 playerId로 해당 플레이어의 gold/inventory를 찾아서
            // Init을 다시 호출하는 등의 라우팅 처리가 추가로 필요할 수 있다.
            Debug.Log($"[ShopUIController] {playerId}: 상점 오픈 (게임 로직에서 호출됨)");

            _shopClosedTcs = new UniTaskCompletionSource(); // 새 "닫힘 대기" 신호 생성
            gameObject.SetActive(true);                     // ShopPanel 활성화 -> OnEnable에서 화면 갱신됨

            return _shopClosedTcs.Task; // 호출한 쪽은 이 Task를 await해서 "닫힐 때까지" 기다리게 된다.
        }

        #endregion

        #region 참조 캐싱

        /// <summary>
        /// itemListArea / currentGoldText / inventoryPanelGo 세 참조를 이름으로 자동 탐색한다.
        /// 인스펙터에서 이미 수동으로 연결해뒀으면(=null이 아니면) 그대로 두고 건너뛴다.
        /// 다 찾아도 안 되면 경고 로그를 남겨서, 어디가 문제인지 바로 알 수 있게 한다.
        /// </summary>
        private void CacheSceneReferences()
        {
            if (itemListArea == null)
            {
                // itemListArea는 ShopPanel(이 스크립트가 붙은 오브젝트) 바로 밑의 자식이다.
                Transform found = transform.Find("ItemListArea");
                itemListArea = found;
            }

            if (currentGoldText == null)
            {
                // 골드 텍스트는 ShopPanel/ShopHeader/CurrentGoldPanel/CurrentGoldText 경로에 있다.
                Transform found = transform.Find("ShopHeader/CurrentGoldPanel/CurrentGoldText");
                currentGoldText = found != null ? found.GetComponent<TMP_Text>() : null;
            }

            if (inventoryPanelGo == null)
            {
                // 인벤토리 패널도 이제 ShopPanel 바로 밑의 자식이다 (예전엔 별도 오브젝트였음).
                Transform found = transform.Find("InventoryPanel");
                inventoryPanelGo = found != null ? found.gameObject : null;
            }

            // 아래는 전부 "못 찾았을 때 알려주는" 방어 코드. 실제 동작에는 영향 없고 디버깅용.
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

        /// <summary>
        /// itemListArea의 자식들(ShopItemCard_0 ~ N)을 순회하면서,
        /// 각 카드 안의 UI 컴포넌트(아이콘/이름/가격/버튼/툴팁)를 미리 찾아 ShopCardRefs로 캐싱한다.
        /// StoreUIBuilder가 만든 하이어라키 이름(NameText, PriceRow/PriceText 등)에 맞춰 Find를 사용한다.
        /// </summary>
        private void CacheCardRefs()
        {
            _cards.Clear();
            if (itemListArea == null) return;

            for (int i = 0; i < itemListArea.childCount; i++)
            {
                Transform card = itemListArea.GetChild(i);

                // ?. (null 조건부 연산자)를 써서, 혹시 특정 자식 오브젝트가 없어도 예외 없이 null로 남긴다.
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
                EnsureCardHoverTooltip(refs); // 이 카드에 마우스 호버 이벤트를 연결
            }
        }

        /// <summary>
        /// 카드에 마우스를 올리면 툴팁이 뜨고, 벗어나면 사라지도록 EventTrigger를 연결한다.
        /// 에디터(StoreUIBuilder)에서 미리 연결해두는 대신 여기서(런타임) 직접 연결하는 이유:
        /// 에디터에서 AddListener로 등록한 콜백은 Play 모드 진입(도메인 리로드) 시 날아갈 수 있는데,
        /// 이렇게 Awake마다 다시 연결하면 그럴 걱정이 없다.
        /// </summary>
        private void EnsureCardHoverTooltip(ShopCardRefs card)
        {
            if (card.tooltipPanel == null) return;

            EventTrigger trigger = card.root.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = card.root.AddComponent<EventTrigger>();
            }

            trigger.triggers.Clear(); // 혹시 이전에 등록된 게 남아있으면 지우고 새로 등록 (중복 방지)

            GameObject tooltip = card.tooltipPanel;

            // 마우스가 카드 위에 들어오면 툴팁 표시
            EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(_ => tooltip.SetActive(true));
            trigger.triggers.Add(enterEntry);

            // 마우스가 카드에서 벗어나면 툴팁 숨김
            EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => tooltip.SetActive(false));
            trigger.triggers.Add(exitEntry);

            tooltip.SetActive(false); // 초기 상태는 항상 숨김으로 리셋
        }

        /// <summary>
        /// inventoryPanelGo 안의 InventorySlotGrid 밑에 있는 슬롯들(InventorySlot_0 ~ 4)을 찾아서
        /// 아이콘 Image와 클릭용 Button을 캐싱하고, 클릭 시 OnInventorySlotClicked가 호출되도록 연결한다.
        /// </summary>
        private void CacheInventorySlotRefs()
        {
            _inventorySlots.Clear();
            if (inventoryPanelGo == null) return;

            Transform grid = inventoryPanelGo.transform.Find("InventorySlotGrid");
            if (grid == null) return;

            for (int i = 0; i < InventoryManager.MaxSlotCount; i++)
            {
                Transform slot = grid.Find($"InventorySlot_{i}");
                if (slot == null) continue; // 슬롯 개수가 안 맞아도 나머지는 계속 처리되도록 continue

                InventorySlotRefs refs = new InventorySlotRefs
                {
                    iconImage = slot.Find("IconImage")?.GetComponent<Image>(),
                    button = slot.GetComponent<Button>(),
                };

                _inventorySlots.Add(refs);

                if (refs.button != null)
                {
                    // 람다 안에서 반복문 변수 i를 그대로 쓰면 클로저 캡처 문제가 생기므로,
                    // 지역 변수 capturedIndex에 복사해서 캡처한다 (C# 반복문 클로저 캡처 관련 고전적인 함정).
                    int capturedIndex = i;
                    refs.button.onClick.RemoveAllListeners(); // 혹시 이전에 등록된 게 있으면 정리
                    refs.button.onClick.AddListener(() => OnInventorySlotClicked(capturedIndex));
                }
            }
        }

        #endregion

        #region 갱신

        /// <summary>
        /// 상점 목록 + 골드 텍스트 + 인벤토리 표시, 세 가지를 한 번에 전부 최신화한다.
        /// 상점이 열릴 때(OnEnable), 구매 직후 등 "화면 전체를 다시 그려야 할 때" 호출한다.
        /// </summary>
        private void RefreshAll()
        {
            RefreshShopList();
            RefreshGoldText();
            RefreshInventoryDisplay();
        }

        /// <summary>
        /// ShopService에서 최신 판매 목록을 받아와서, 캐싱해둔 카드 5개에 데이터를 채운다.
        /// 목록 개수가 카드 개수보다 적으면 남는 카드는 비활성화한다.
        /// 인벤토리가 "교체 대기" 상태면, 먼저 그것부터 처리해야 하므로 모든 BUY 버튼을 잠근다.
        /// </summary>
        private void RefreshShopList()
        {
            IReadOnlyList<EquipmentDef> list = shopService.GetShopList();

            for (int i = 0; i < _cards.Count; i++)
            {
                ShopCardRefs card = _cards[i];

                if (i >= list.Count)
                {
                    card.root.SetActive(false); // 팔 물건이 없는 칸은 숨긴다
                    continue;
                }

                card.root.SetActive(true);
                EquipmentDef def = list[i];

                // 텍스트/아이콘 채우기. null 체크는 씬 구조가 예상과 다를 때 예외를 막기 위함.
                if (card.nameText != null) card.nameText.text = def.Name;
                if (card.priceText != null) card.priceText.text = EquipmentDisplayFormatter.GetPriceText(def);
                if (card.effectText != null) card.effectText.text = EquipmentDisplayFormatter.GetEffectText(def);
                if (card.cooldownText != null) card.cooldownText.text = EquipmentDisplayFormatter.GetCooldownText(def);
                if (card.durabilityText != null) card.durabilityText.text = EquipmentDisplayFormatter.GetDurabilityText(def);

                if (card.iconImage != null)
                {
                    // IconKey는 "Resources 폴더 기준 경로" 문자열이다 (예: "Icons/Equipment/rusty_dagger").
                    // 비어있거나 해당 경로에 파일이 없으면 Resources.Load가 null을 반환한다.
                    Sprite icon = string.IsNullOrEmpty(def.IconKey) ? null : Resources.Load<Sprite>(def.IconKey);

                    if (icon != null)
                    {
                        card.iconImage.sprite = icon;
                        card.iconImage.color = Color.white; // 실제 아이콘은 원래 색 그대로 보여준다 (틴트 없음)
                    }
                    else
                    {
                        // 아이콘이 아직 없거나 경로가 잘못됐으면, 등급별 색상 블록으로 안전하게 대체한다.
                        card.iconImage.color = GetGradeColor(def.Grade);
                    }
                }

                if (card.buyButton != null)
                {
                    int capturedIndex = i; // 클로저 캡처 주의 (위 인벤토리 슬롯과 같은 이유)
                    card.buyButton.onClick.RemoveAllListeners();
                    card.buyButton.onClick.AddListener(() => OnPurchaseClicked(capturedIndex));

                    // 인벤토리가 꽉 차서 교체를 기다리는 중이면, 새로 구매하지 못하도록 버튼을 잠근다.
                    // (교체를 먼저 마쳐야 다음 구매가 가능하다는 정책)
                    card.buyButton.interactable = inventory == null || !inventory.IsWaitingReplace;
                }
            }
        }

        /// <summary>
        /// 인벤토리 5칸의 아이콘을 현재 실제 인벤토리 내용(InventoryManager)에 맞춰 다시 그린다.
        /// 빈 슬롯은 완전히 투명하게 만들어서 "비어있음"을 표현한다.
        /// </summary>
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
                    // 빈 슬롯 - 스프라이트 제거하고 알파값 0으로 완전히 투명 처리
                    iconImage.sprite = null;
                    iconImage.color = new Color(1f, 1f, 1f, 0f);
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
                    // 아이콘이 없으면 상점 카드와 동일하게 등급 색상 블록으로 대체
                    iconImage.sprite = null;
                    iconImage.color = GetGradeColor(def.Grade);
                }
            }
        }

        /// <summary>헤더의 "Current Gold : n" 텍스트를 현재 골드 값으로 갱신한다.</summary>
        private void RefreshGoldText()
        {
            if (currentGoldText != null && gold != null)
            {
                currentGoldText.text = $"Current Gold : {gold.CurrentGold}";
            }
        }

        /// <summary>등급(EquipmentGrade)에 따라 임시로 보여줄 색상 블록 색을 반환한다.</summary>
        private Color GetGradeColor(EquipmentGrade grade)
        {
            switch (grade)
            {
                case EquipmentGrade.Rare: return rareColor;
                case EquipmentGrade.Legendary: return legendaryColor;
                default: return commonColor; // Common 및 그 외 알 수 없는 값은 기본 색
            }
        }

        #endregion

        #region 구매 흐름

        /// <summary>
        /// 상점 카드의 BUY 버튼 클릭 시 호출된다.
        /// 실제 구매 처리는 ShopService에 위임하고, 결과에 따라 화면을 갱신한다.
        /// </summary>
        /// <param name="shopIndex">클릭된 카드가 상점 목록에서 몇 번째 항목인지</param>
        private void OnPurchaseClicked(int shopIndex)
        {
            if (gold == null || inventory == null)
            {
                // Init()이 호출되기 전에 버튼을 누르는 경우 (테스트 중 실수 등)를 방어한다.
                Debug.LogWarning("[ShopUIController] Init(gold, inventory)이 호출되지 않았습니다.");
                return;
            }

            // 실제 구매 로직: 골드 차감 + 인벤토리에 추가 (또는 꽉 찼으면 "교체 대기" 상태로 전환)까지
            // ShopService.TryPurchase 내부에서 처리된다. 여기서는 결과만 받는다.
            bool purchased = shopService.TryPurchase(shopIndex, gold, inventory);

            if (!purchased)
            {
                // 자금 부족 등으로 실패한 경우 - 화면 갱신 없이 그냥 로그만 남기고 종료
                Debug.Log("[ShopUIController] 자금이 부족합니다.");
                return;
            }

            // 구매 성공 - 골드/인벤토리/상점목록(버튼 잠금 상태 포함) 전부 다시 그린다.
            RefreshGoldText();
            RefreshInventoryDisplay();
            RefreshShopList(); // 인벤토리가 꽉 찼으면 여기서 모든 BUY 버튼이 잠긴다.

            if (inventory.IsWaitingReplace)
            {
                // 인벤토리가 가득 차서 새 장비를 어느 칸에 넣을지 플레이어가 직접 골라야 하는 상태.
                Debug.Log("[ShopUIController] 인벤토리가 가득 찼습니다. 인벤토리 칸을 클릭해서 교체할 장비를 선택하세요.");
            }
            else
            {
                Debug.Log("[ShopUIController] 구매 완료.");
            }
        }

        /// <summary>
        /// 인벤토리 1~5번 칸을 클릭했을 때 호출된다.
        /// "교체 대기" 상태가 아니면 클릭해도 아무 반응 없음 (평소엔 그냥 보여주기만 하는 슬롯).
        /// 빈 칸이면 확인 없이 바로 새 장비를 배치하고, 이미 뭔가 들어있는 칸이면
        /// "이걸 이걸로 바꿀까요?" 확인 팝업(_slotReplacePopup)을 띄운다.
        /// </summary>
        /// <param name="slotIndex">클릭된 인벤토리 슬롯 번호 (0부터 시작)</param>
        private void OnInventorySlotClicked(int slotIndex)
        {
            if (inventory == null || !inventory.IsWaitingReplace) return; // 교체 대기 중이 아니면 무시

            EquipmentInstance current = inventory.GetAt(slotIndex); // 지금 그 칸에 있는 장비 (없으면 null)
            EquipmentDef pendingDef = inventory.ReplaceDef;         // 새로 구매해서 대기 중인 장비 정보

            if (current == null)
            {
                // 빈 칸을 클릭한 경우 - 굳이 확인할 필요 없이 바로 채워 넣는다.
                inventory.ReplaceAt(slotIndex);
                OnReplaceResolved();
            }
            else
            {
                // 이미 장비가 들어있는 칸 - "정말 바꿀 건지" 한 번 확인받는다.
                _slotReplacePopup.Show(current.Def.Name, pendingDef.Name,
                    onYes: () =>
                    {
                        // Yes를 누르면 실제로 교체를 확정하고 팝업을 닫는다.
                        inventory.ReplaceAt(slotIndex);
                        _slotReplacePopup.Hide();
                        OnReplaceResolved();
                    },
                    onNo: () => _slotReplacePopup.Hide()); // No는 그냥 팝업만 닫고 교체 대기 상태 유지
            }
        }

        /// <summary>
        /// 교체(빈 칸 즉시배치 또는 Yes 확인)가 끝난 직후 공통으로 실행되는 후처리.
        /// 인벤토리 화면을 갱신하고, 이제 교체 대기가 풀렸으니 상점 목록도 다시 그려서
        /// 잠겨있던 BUY 버튼들을 다시 눌러지게 만든다.
        /// </summary>
        private void OnReplaceResolved()
        {
            RefreshInventoryDisplay();
            RefreshShopList();
        }

        #endregion
    }
}