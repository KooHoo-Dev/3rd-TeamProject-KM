using BlueMarble.Board.Builder;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace BlueMarble.Board
{
    public class Tile : MonoBehaviour
    {
        [field: Title("Components")]
        [field: SerializeField] public SpriteRenderer MainRenderer { get; private set; }
        [field: SerializeField] public TMP_Text AreaName { get; private set; }
        [field: SerializeField] public SpriteRenderer AreaIconRenderer { get; private set; }
        [field: SerializeField] public SpriteRenderer RegionRenderer { get; private set; }

        public TileDef Def { get; private set; }

        public void Initialize(TileDef def)
        {
            Def = def;
        }
    }
}