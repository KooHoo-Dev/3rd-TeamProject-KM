using System.Collections.Generic;
using UnityEngine;

namespace BlueMarble.Board.Builder
{
    public class TileDef
    {
        public int ID { get; private set; }
        public string Name { get; private set; }

        public TileType Type { get; private set; }
        public TileDirection Dir { get; private set; }

        public GameObject Prefab { get; private set; }

        public TileDef(int id, string name, TileType type, TileDirection dir, GameObject prefab)
        {
            ID = id;
            Name = name;
            Type = type;
            Dir = dir;
            Prefab = prefab;
        }
    }
}