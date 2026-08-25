using System.Collections.Generic;
using BlueMarble.Utility;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utility.Data;

namespace BlueMarble.Board.Builder
{
    public class BoardSpaceTable : DataTable<BoardSpaceDTO, BoardSpaceDef>
    {
        protected override string TablePath => "Board/spaces";

        public override async UniTask LoadAsync()
        {
            BoardSpaceIconMap iconMap = BoardSpaceIconMap.Instance;
            
            defs.Clear();
            List<BoardSpaceDTO> rows = await GetRows();

            for (int i = 0; i < rows.Count; i++)
            {
                BoardSpaceDTO dto = rows[i];

                int id = dto.Id;
                string name = dto.Name;
                int price = dto.Price;
                Sprite icon = iconMap.GetIcon(dto.IconIndex);
                Color color = dto.ColorHex.HexToColor();

                defs[dto.Id] = new BoardSpaceDef(id, name, price, icon, color);
            }
        }
    }
}