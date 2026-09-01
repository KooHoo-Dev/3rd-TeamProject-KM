using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utility.Data;

namespace BlueMarble
{
    public class GoldCardTable : DataTable<GoldCardDTO, GoldCardDef>
    {
        protected override string TablePath => "gold_cards";
        
        public override async UniTask LoadAsync()
        {
            DataManager data = DataManager.Instance;
            
            defs.Clear();
            List<GoldCardDTO> rows = await GetRows();

            for (int i = 0; i < rows.Count; i++)
            {
                GoldCardDTO dto = rows[i];
                
                int id = dto.Id;
                string name = dto.Name;
                string desc = dto.Description;
                string flavorText = dto.FlavorText;
                Sprite icon = Resources.Load<Sprite>(dto.IconKey);

                GoldCardType type = Enum.Parse<GoldCardType>(dto.Type);
                GoldCardScope scope = Enum.Parse<GoldCardScope>(dto.Scope);

                int goldAmount = dto.GoldAmount;
                int dropTableId = dto.DropTableId;
                BoardSpaceDef spaceDef = data.GetBoardSpaceDef(dto.SpaceId);

                defs[dto.Id] = new GoldCardDef(
                    id, name, desc, flavorText, icon,
                    type, scope, 
                    goldAmount, dropTableId, spaceDef);
            }
        }
    }
}