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

            int id = (int)tileType;
            
            TileDef def = Data.GetTileDef(id);
            GameObject pref = def.Prefabs[tileDir];
            
            GameObject created = Instantiate(pref, root);
            created.name = $"Tile_{tileType}_{tileDir}";
        }
    }
}