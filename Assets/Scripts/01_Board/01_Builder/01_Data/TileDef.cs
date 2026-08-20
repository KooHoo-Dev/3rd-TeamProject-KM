using System.Collections.Generic;
using UnityEngine;

namespace BlueMarble.Board.Builder
{
    public class TileDef
    {
        public int ID { get; private set; }
        public string Name { get; private set; }

        public TileType Type { get; private set; }
        
        public Dictionary<TileDirection, GameObject> Prefabs { get; private set; }

        public TileDef(int id, string name, TileType type, Dictionary<TileDirection, GameObject> prefabs)
        {
            ID = id;
            Name = name;
            Type = type;
            Prefabs = prefabs;
        }
    }
}