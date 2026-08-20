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

                (int id, string name, TileType type, 
                    Dictionary<TileDirection, GameObject> prefabs) = Interpret(dto);
                
                defs[dto.ID] = new TileDef(id, name, type, prefabs);
            }
        }
        
        private (int, string, TileType, Dictionary<TileDirection, GameObject>) Interpret(TileDTO dto)
        {
            int id = dto.ID;
            string name = dto.Name;
            TileType type = Enum.Parse<TileType>(dto.TileType);
            Dictionary<TileDirection, GameObject> prefabs = new();
            
            string basePath = $"{dto.PrefabPath}/Tile_{type}";
            
            foreach (string rawDir in dto.ValidDir.Split(DELIMITER))
            {
                TileDirection dir = Enum.Parse<TileDirection>(
                    rawDir.Trim(),
                    ignoreCase: true
                );

                string path = $"{basePath}_{dir}";
                prefabs[dir] = Resources.Load<GameObject>(path);
            }
            
            return (id, name, type, prefabs);
        }
    }
}