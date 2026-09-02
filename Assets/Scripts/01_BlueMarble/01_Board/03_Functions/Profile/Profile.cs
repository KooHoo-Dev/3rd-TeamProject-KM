using BlueMarble.Server;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlueMarble
{
    public class Profile : MonoBehaviour
    {
        [field: Title("Components")]
        [field: SerializeField] public Image PortraitRenderer { get; private set; }
        [field: SerializeField] public TMP_Text NicknameText { get; private set; }
        [field: SerializeField] public TMP_Text GoldText { get; private set; }

        public string Id { get; private set; }

        public void Initialize(User user)
        {
            Id = user.Id;
            NicknameText.text = $"{user.Nickname}({user.Id})";
        }

        public void SetGold(int gold)
        {
            GoldText.text = $"자산: {gold:N0} 골드";
        }
    }
}