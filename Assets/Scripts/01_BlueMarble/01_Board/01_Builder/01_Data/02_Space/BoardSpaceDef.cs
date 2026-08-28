using UnityEngine;

namespace BlueMarble
{
    public class BoardSpaceDef
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public int Price { get; private set; }
        public bool IsActiveOnPass { get; set; }

        public Sprite Icon { get; private set; }
        public Color Color { get; private set; }
        
        public BoardSpaceDef(int id, string name, int price, bool isActiveOnPass, Sprite icon, Color color)
        {
            Id = id;
            Name = name;
            Price = price;
            IsActiveOnPass = isActiveOnPass;
            Icon = icon;
            Color = color;
        }
    }
}