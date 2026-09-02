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
            defs.Clear();
            List<GoldCardDTO> rows = await GetRows();

            for (int i = 0; i < rows.Count; i++)
            {
                GoldCardDTO dto = rows[i];
                
                int id = dto.Id;
                string name = dto.Name;
                string description = dto.Description;
                string flavorText = dto.FlavorText;
                Sprite icon = Resources.Load<Sprite>(dto.IconKey);

                GoldCardPatternType patternType = Enum.Parse<GoldCardPatternType>(dto.Pattern);
                GoldCardScopeType scopeType = Enum.Parse<GoldCardScopeType>(dto.Scope);

                int goldAmount = dto.GoldAmount;
                int dropTableId = dto.DropTableId;
                int spaceId = dto.SpaceId;

                defs[dto.Id] = new GoldCardDef(
                    id, name, description, flavorText, icon,
                    patternType, scopeType, 
                    goldAmount, dropTableId, spaceId);
            }
        }
    }
}