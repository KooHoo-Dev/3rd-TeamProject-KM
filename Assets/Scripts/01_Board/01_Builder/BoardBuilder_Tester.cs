using System;
using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlueMarble.Board.Builder
{
    public class BoardBuilder_Tester : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private TileType tileType;
        [SerializeField] private TileDirection tileDir;

        [Header("Refs")]
        [SerializeField] private Transform root;
        
        private DataManager Data => DataManager.Instance;

        private void Update()
        {
            if (Key.Space.GetKeyDown())
                CreateTile();
        }

        private async void CreateTile()
        {
            await Data.EnsureDataLoaded();
            
            TileDef def = Data.GetTileDef((int)tileType);

            GameObject created = Instantiate(def.Prefab, root);
            created.name = $"Tile_{tileType}_{tileDir}";
        }
    }
}