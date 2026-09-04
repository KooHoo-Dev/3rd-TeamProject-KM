using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Utility.Data;

namespace BlueMarble
{
    public class DropProbTable : DataTable<DropProbDTO, DropProbDef>
    {
        protected override string TablePath => "Equipment/drop_probs";
        
        public override async UniTask LoadAsync()
        {
            defs.Clear();
            List<DropProbDTO> rows = await GetRows();

            for (int i = 0; i < rows.Count; i++)
            {
                DropProbDTO dto = rows[i];

                int id = dto.Id;
                Dictionary<EquipmentGrade, float> probs = new()
                {
                    { EquipmentGrade.Common, dto.CommonProb },
                    { EquipmentGrade.Rare, dto.RareProb },
                    { EquipmentGrade.Legendary, dto.LegendaryProb }
                };
                int gainCount = dto.GainCount;
                
                defs[id] = new DropProbDef(id, probs, gainCount);
            }
        }
    }
}