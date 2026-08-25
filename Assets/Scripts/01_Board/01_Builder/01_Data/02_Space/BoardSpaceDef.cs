using UnityEngine;

namespace BlueMarble.Board.Builder
{
    public class BoardSpaceDef
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public int Price { get; private set; }

        public Sprite Icon { get; private set; }
        public Color Color { get; private set; }
        
        public BoardSpaceDef(int id, string name, int price, Sprite icon, Color color)
        {
            Id = id;
            Name = name;
            Price = price;
            Icon = icon;
            Color = color;
        }
    }
}