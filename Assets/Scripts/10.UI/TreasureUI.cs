using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace BlueMarble
{
    /// <summary>
    /// 화면 중앙에 뜨는 보상 상자 오픈 팝업.
    /// Open()을 호출하면 배경 페이드 -> 상자 흔들림 -> 뚜껑 열림 -> 빛줄기 확산
    /// -> 보상 아이콘(다이아몬드) 튀어오름 -> 반짝이 파티클 순서로 연출된다.
    ///
    /// 실제 아트가 준비되기 전까지, 모든 파츠는 uGUI Image + 런타임 생성 스프라이트로 표현한다.
    /// 나중에 아트가 나오면 각 Image의 sprite만 교체하면 된다.
    /// </summary>
    public class TreasureRewardPopupController : MonoBehaviour
    {
        #region 인스펙터 필드

        [Header("루트 (비워두면 자동 생성)")]
        [SerializeField] private RectTransform popupRoot;
        [SerializeField] private Image dimBackground;
        [SerializeField] private Image lightRayImage;
        [SerializeField] private RectTransform chestGroup;
        [SerializeField] private RectTransform lidPivot;
        [SerializeField] private CanvasGroup lidCanvasGroup;
        [SerializeField] private RectTransform rewardIcon;
        [SerializeField] private RectTransform sparkleContainer;
        [SerializeField] private Button tapToContinueButton;

        [Header("색상")]
        [SerializeField] private Color bodyColor = new Color(0.42f, 0.26f, 0.12f, 1f);
        [SerializeField] private Color lidColor = new Color(0.5f, 0.32f, 0.15f, 1f);
        [SerializeField] private Color trimColor = new Color(0.95f, 0.78f, 0.25f, 1f);
        [SerializeField] private Color rayColor = new Color(1f, 0.9f, 0.5f, 1f);
        [SerializeField] private Color gemColor = new Color(0.35f, 0.85f, 0.95f, 1f); // 범용 보석 색 (하늘색)

        [Header("타이밍 - 배경/흔들림")]
        [SerializeField] private float dimFadeDuration = 0.25f;
        [SerializeField] private float shakeDuration = 0.4f;
        [SerializeField] private float shakeStrength = 12f;

        [Header("타이밍 - 뚜껑/빛줄기")]
        [SerializeField] private float lidOpenDuration = 0.35f;
        [SerializeField] private float lidTiltAngle = 20f;      // 회전은 아주 살짝만 (튕겨나가 보이는 것 방지)
        [SerializeField] private float lidLiftDistance = 50f;   // 위로 띄우는 거리
        [SerializeField] private float rayRotateDuration = 8f;

        [Header("타이밍 - 보상/파티클")]
        [SerializeField] private float rewardPopDuration = 0.5f;
        [SerializeField] private float rewardPopHeight = 90f;
        [SerializeField] private int sparkleCount = 10;
        [SerializeField] private float sparkleTravelDistance = 160f;
        [SerializeField] private float sparkleDuration = 0.6f;

        #endregion

        private readonly List<RectTransform> _sparkles = new List<RectTransform>();
        private Tween _rayRotateTween;
        private Sequence _openSequence;

        #region 초기화

        private void Awake()
        {
            EnsurePlaceholderHierarchy();
            popupRoot.gameObject.SetActive(false);
        }

        [ContextMenu("플레이스홀더 생성 / 재생성")]
        private void EnsurePlaceholderHierarchy()
        {
            if (popupRoot == null)
            {
                popupRoot = CreateStretchPanel("TreasureRewardPopup", transform);
            }

            if (dimBackground == null)
            {
                dimBackground = CreateStretchImage("DimBackground", popupRoot, new Color(0f, 0f, 0f, 0.75f));
            }

            if (lightRayImage == null)
            {
                GameObject rayGo = NewRect("LightRayImage", popupRoot, new Vector2(700f, 700f));
                Image rayImg = rayGo.AddComponent<Image>();
                rayImg.sprite = GetOrCreateRaySprite();
                rayImg.color = rayColor;
                rayGo.transform.SetSiblingIndex(1); // 배경 위, 상자 아래
                lightRayImage = rayImg;
                CenterRect(rayImg.rectTransform);
                SetAlpha(lightRayImage, 0f); // 평소엔 숨김
            }

            if (chestGroup == null)
            {
                chestGroup = NewRect("ChestGroup", popupRoot, Vector2.zero).GetComponent<RectTransform>();
                CenterRect(chestGroup);

                CreateRectImage("ChestBody", chestGroup, new Vector2(260f, 160f), bodyColor, new Vector2(0f, -30f));
                CreateRectImage("BodyTrimTop", chestGroup, new Vector2(260f, 14f), trimColor, new Vector2(0f, 42f));
                CreateRectImage("BodyTrimBottom", chestGroup, new Vector2(260f, 14f), trimColor, new Vector2(0f, -108f));

                GameObject pivotGo = NewRect("LidPivot", chestGroup, Vector2.zero);
                RectTransform pivotRect = pivotGo.GetComponent<RectTransform>();
                pivotRect.anchoredPosition = new Vector2(0f, 45f); // 경첩 위치 (몸통 뒤쪽 상단)
                lidPivot = pivotRect;
                lidCanvasGroup = pivotGo.AddComponent<CanvasGroup>();

                CreateRectImage("LidBase", lidPivot, new Vector2(260f, 70f), lidColor, new Vector2(0f, 35f));
                CreateCircleImage("LidDome", lidPivot, 140f, lidColor, new Vector2(0f, 70f));
                CreateRectImage("LidTrim", lidPivot, new Vector2(260f, 10f), trimColor, new Vector2(0f, 4f));

                CreateRectImage("Lock", chestGroup, new Vector2(36f, 44f), trimColor, new Vector2(0f, 30f));
            }

            if (rewardIcon == null)
            {
                GameObject gemGo = NewRect("RewardIcon", popupRoot, new Vector2(90f, 90f));
                Image gemImg = gemGo.AddComponent<Image>();
                gemImg.sprite = GetOrCreateSquareSprite();
                gemImg.color = gemColor;
                rewardIcon = gemImg.rectTransform;
                CenterRect(rewardIcon);
                rewardIcon.anchoredPosition = new Vector2(0f, -10f);
                rewardIcon.localRotation = Quaternion.Euler(0f, 0f, 45f); // 다이아몬드 모양
                rewardIcon.localScale = Vector3.zero;
            }

            if (sparkleContainer == null)
            {
                sparkleContainer = NewRect("SparkleContainer", popupRoot, Vector2.zero).GetComponent<RectTransform>();
                CenterRect(sparkleContainer);

                for (int i = 0; i < sparkleCount; i++)
                {
                    GameObject dotGo = NewRect($"Sparkle_{i}", sparkleContainer, new Vector2(14f, 14f));
                    Image dotImg = dotGo.AddComponent<Image>();
                    dotImg.sprite = GetOrCreateSquareSprite();
                    dotImg.color = trimColor;
                    dotImg.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                    dotGo.SetActive(false);
                    _sparkles.Add(dotImg.rectTransform);
                }
            }

            if (tapToContinueButton == null)
            {
                GameObject btnGo = NewRect("TapToContinueButton", popupRoot, Vector2.zero);
                Image btnImg = btnGo.AddComponent<Image>();
                btnImg.color = new Color(0f, 0f, 0f, 0f); // 투명 - 화면 전체를 덮는 클릭 영역
                Stretch(btnImg.rectTransform);
                btnGo.transform.SetAsLastSibling(); // 최상단에서 클릭을 받는다

                tapToContinueButton = btnGo.AddComponent<Button>();
                tapToContinueButton.targetGraphic = btnImg;
                tapToContinueButton.onClick.AddListener(ClosePopup);
            }
        }

        #endregion

        #region 연출

        [ContextMenu("테스트 - Open 재생")]
        public void Open()
        {
            _openSequence?.Kill();
            _rayRotateTween?.Kill();

            // 초기 상태로 리셋
            popupRoot.gameObject.SetActive(true);
            SetAlpha(dimBackground, 0f);
            SetAlpha(lightRayImage, 0f);
            lightRayImage.rectTransform.localScale = Vector3.one * 0.5f;
            lidPivot.localRotation = Quaternion.identity;
            Vector2 lidStartPos = new Vector2(0f, 45f);
            lidPivot.anchoredPosition = lidStartPos;
            lidCanvasGroup.alpha = 1f;
            rewardIcon.localScale = Vector3.zero;
            Vector2 rewardStartPos = rewardIcon.anchoredPosition;

            _openSequence = DOTween.Sequence();

            // 1) 배경 페이드 인
            _openSequence.Append(dimBackground.DOFade(0.75f, dimFadeDuration));

            // 2) 상자 흔들림 (anticipation)
            _openSequence.Append(
                chestGroup.DOShakeAnchorPos(shakeDuration, shakeStrength, vibrato: 14)
            );

            // 3) 뚜껑이 살짝 기울며 위로 떠오르고, 동시에 투명해지며 사라짐 (빛에 날아가는 느낌)
            _openSequence.Append(
                lidPivot.DOLocalRotate(new Vector3(0f, 0f, lidTiltAngle), lidOpenDuration).SetEase(Ease.OutCubic)
            );
            _openSequence.Join(
                lidPivot.DOAnchorPos(lidStartPos + new Vector2(0f, lidLiftDistance), lidOpenDuration).SetEase(Ease.OutCubic)
            );
            _openSequence.Join(
                lidCanvasGroup.DOFade(0f, lidOpenDuration).SetDelay(lidOpenDuration * 0.3f)
            );
            _openSequence.Join(lightRayImage.DOFade(0.8f, lidOpenDuration));
            _openSequence.Join(lightRayImage.rectTransform.DOScale(1f, lidOpenDuration).SetEase(Ease.OutCubic));

            // 4) 보상 아이콘 튀어오름
            _openSequence.AppendCallback(() => rewardIcon.anchoredPosition = rewardStartPos);
            _openSequence.Append(rewardIcon.DOScale(1f, rewardPopDuration * 0.4f).SetEase(Ease.OutBack));
            _openSequence.Join(
                rewardIcon.DOAnchorPos(rewardStartPos + new Vector2(0f, rewardPopHeight), rewardPopDuration)
                    .SetEase(Ease.OutCubic)
            );

            // 5) 반짝이 파티클 터짐
            _openSequence.AppendCallback(PlaySparkleBurst);

            _openSequence.Play();

            // 빛줄기는 팝업이 열려있는 동안 계속 회전
            _rayRotateTween = lightRayImage.rectTransform
                .DORotate(new Vector3(0f, 0f, 360f), rayRotateDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        private void PlaySparkleBurst()
        {
            foreach (RectTransform sparkle in _sparkles)
            {
                sparkle.gameObject.SetActive(true);
                sparkle.anchoredPosition = rewardIcon.anchoredPosition;
                sparkle.localScale = Vector3.one;

                Image img = sparkle.GetComponent<Image>();
                SetAlpha(img, 1f);

                Vector2 direction = Random.insideUnitCircle.normalized;
                Vector2 target = sparkle.anchoredPosition + direction * sparkleTravelDistance * Random.Range(0.6f, 1f);

                Sequence sparkleSeq = DOTween.Sequence();
                sparkleSeq.Append(sparkle.DOAnchorPos(target, sparkleDuration).SetEase(Ease.OutCubic));
                sparkleSeq.Join(img.DOFade(0f, sparkleDuration).SetDelay(sparkleDuration * 0.3f));
                sparkleSeq.OnComplete(() => sparkle.gameObject.SetActive(false));
            }
        }

        public void ClosePopup()
        {
            _openSequence?.Kill();
            _rayRotateTween?.Kill();
            popupRoot.gameObject.SetActive(false);
        }

        #endregion

        #region 생성 도우미

        private RectTransform CreateStretchPanel(string name, Transform parent)
        {
            GameObject go = NewRect(name, parent, Vector2.zero);
            RectTransform rect = go.GetComponent<RectTransform>();
            Stretch(rect);
            return rect;
        }

        private Image CreateStretchImage(string name, Transform parent, Color color)
        {
            GameObject go = NewRect(name, parent, Vector2.zero);
            Image img = go.AddComponent<Image>();
            img.color = color;
            Stretch(img.rectTransform);
            return img;
        }

        private Image CreateRectImage(string name, Transform parent, Vector2 size, Color color, Vector2 anchoredPosition)
        {
            GameObject go = NewRect(name, parent, size);
            Image img = go.AddComponent<Image>();
            img.sprite = GetOrCreateSquareSprite();
            img.color = color;
            CenterRect(img.rectTransform);
            img.rectTransform.anchoredPosition = anchoredPosition;
            return img;
        }

        private Image CreateCircleImage(string name, Transform parent, float diameter, Color color, Vector2 anchoredPosition)
        {
            GameObject go = NewRect(name, parent, new Vector2(diameter, diameter));
            Image img = go.AddComponent<Image>();
            img.sprite = GetOrCreateCircleSprite();
            img.color = color;
            CenterRect(img.rectTransform);
            img.rectTransform.anchoredPosition = anchoredPosition;
            return img;
        }

        private GameObject NewRect(string name, Transform parent, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
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

        private void SetAlpha(Image image, float alpha)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }

        #endregion

        #region 플레이스홀더 스프라이트 생성

        private static Sprite _cachedSquareSprite;
        private static Sprite _cachedCircleSprite;
        private static Sprite _cachedRaySprite;

        private static Sprite GetOrCreateSquareSprite()
        {
            if (_cachedSquareSprite != null) return _cachedSquareSprite;

            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();

            _cachedSquareSprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return _cachedSquareSprite;
        }

        private static Sprite GetOrCreateCircleSprite()
        {
            if (_cachedCircleSprite != null) return _cachedCircleSprite;

            const int resolution = 64;
            Texture2D tex = new Texture2D(resolution, resolution);
            Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
            float radius = resolution / 2f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    tex.SetPixel(x, y, dist <= radius ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            }

            tex.Apply();
            _cachedCircleSprite = Sprite.Create(tex, new Rect(0f, 0f, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
            return _cachedCircleSprite;
        }

        // 방사형 빛줄기(라이트 버스트) 텍스처를 직접 그려서 생성한다.
        private static Sprite GetOrCreateRaySprite()
        {
            if (_cachedRaySprite != null) return _cachedRaySprite;

            const int resolution = 256;
            const int rayCount = 16;

            Texture2D tex = new Texture2D(resolution, resolution);
            Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
            float maxRadius = resolution / 2f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector2 pos = new Vector2(x + 0.5f, y + 0.5f) - center;
                    float dist = pos.magnitude;
                    float radial = Mathf.Clamp01(1f - dist / maxRadius);
                    float angle = Mathf.Atan2(pos.y, pos.x);
                    float spokes = Mathf.Pow(Mathf.Abs(Mathf.Sin(angle * rayCount * 0.5f)), 3f);
                    float alpha = radial * spokes;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            _cachedRaySprite = Sprite.Create(tex, new Rect(0f, 0f, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
            return _cachedRaySprite;
        }

        #endregion
    }
}