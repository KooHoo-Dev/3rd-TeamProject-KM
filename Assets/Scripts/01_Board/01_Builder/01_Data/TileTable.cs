using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Jay.FileIO;
using UnityEngine;

namespace BlueMarble.Board.Builder
{
    public class TileTable
    {
        private const char DELIMITER = ',';
        private const string TABLE_NAME = "Board/tiles";

        private readonly Dictionary<int, TileDef> defs = new();

        public int Count => defs.Count;
        
        public TileDef Get(int id) => defs[id];
        public IReadOnlyCollection<TileDef> GetDefs() => defs.Values;
        
        public async UniTask LoadAsync()
        {
            defs.Clear();
            
            List<TileDTO> rows = await TSVReader.ReadTableAsync<TileDTO>(TABLE_NAME, true);

            for (int i = 0; i < rows.Count; i++)
            {
                TileDTO dto = rows[i];

                int id = dto.ID;
                string name = dto.Name;
                TileType type = Enum.Parse<TileType>(dto.TileType);
                TileDirection dir = Enum.Parse<TileDirection>(dto.TileDir);
                GameObject prefab = Resources.Load<GameObject>($"{dto.PrefabPath}/Tile_{type}_{dir}");
                
                defs[dto.ID] = new TileDef(id, name, type, dir, prefab);
            }
        }
    }
}