using BlueMarble.Server;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlueMarble
{
    public class Profile : MonoBehaviour
    {
        [Title("Components")]
        [SerializeField] private Image portraitRenderer;
        [SerializeField] private TMP_Text nicknameText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text totalGoldText;
        [SerializeField] private Image[] equipmentIcons;
        [SerializeField] private GameObject bubble;
        [SerializeField] private TMP_Text bubbleText;

        public string Id { get; private set; }

        public void Initialize(User user)
        {
            Id = user.Id;

            PlayerAppearance appearance = PlayerAppearanceMap.Instance
                .GetAppearance(user.PlayerNumber);

            portraitRenderer.sprite = appearance.Portrait;
            nicknameText.text = $"{user.Nickname}({user.Id})";

            HideBubble();
        }

        public void SetGold(int amount)
        {
            goldText.SetText($"자산: {amount:N0} 골드");
        }

        public void SetTotalGold(int amount)
        {
            totalGoldText.SetText($"총 자산: {amount:N0} 골드");
        }

        public void ShowBubble(string message)
        {
            bubbleText.SetText(message);
            bubble.SetActive(true);
        }

        public void HideBubble()
        {
            bubble.SetActive(false);
        }
    }
}
