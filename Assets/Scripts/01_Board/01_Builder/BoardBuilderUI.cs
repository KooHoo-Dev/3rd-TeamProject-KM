using System;
using UnityEngine;
using UnityEngine.UI;

namespace BlueMarble.Board.Builder
{
    [RequireComponent(typeof(BoardBuilder))]
    public class BoardBuilderUI : MonoBehaviour
    {
        [SerializeField] private Toggle propertyToggle;
        [SerializeField] private Toggle specialToggle;
        [SerializeField] private Toggle cornerToggle;
        [SerializeField] private Button createButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private Button saveButton;
        
        private TileType currentType = TileType.Property;
        
        private BoardBuilder builder;

        private void Awake()
        {
            builder = GetComponent<BoardBuilder>();
        }

        private void Start()
        {
            OnToggleChanged(propertyToggle, TileType.Property);
            OnToggleChanged(specialToggle, TileType.Special);
            OnToggleChanged(cornerToggle, TileType.Corner);
            createButton.onClick.AddListener(OnCreateButtonClicked);
            removeButton.onClick.AddListener(OnRemoveButtonClicked);
            saveButton.onClick.AddListener(OnSaveButtonClicked);
        }

        private void OnToggleChanged(Toggle toggle, TileType type)
        {
            toggle.onValueChanged.AddListener(value =>
            {
                if (value == false) return;
                currentType = type;
            });
        }

        private void OnCreateButtonClicked() => builder.CreateTile(currentType);

        private void OnRemoveButtonClicked() => builder.RemoveLastTile();
        
        private void OnSaveButtonClicked() => builder.SaveBoard();
    }
}