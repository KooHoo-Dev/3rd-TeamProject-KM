using Epic3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace BlueMarble
{
    public class Tile : MonoBehaviour
    {
        [field: Title("Components")]
        [field: SerializeField] public SpriteRenderer MainRenderer { get; private set; }
        [field: SerializeField] public TMP_Text AreaName { get; private set; }
        [field: SerializeField] public SpriteRenderer AreaIconRenderer { get; private set; }
        [field: SerializeField] public SpriteRenderer RegionRenderer { get; private set; }

        public TileDef Def { get; private set; }
        public BoardSpaceDef SpaceDef { get; private set; }
        
        public void Initialize(TileDef def, BoardSpaceDef spaceDef)
        {
            Def = def;
            SpaceDef = spaceDef;
        }

        public void UpdateVisuals()
        {
            AreaName.SetText(SpaceDef.Name);
            AreaIconRenderer.sprite = SpaceDef.Icon;
            if (RegionRenderer != null) RegionRenderer.color = SpaceDef.Color;
        }
    }
}