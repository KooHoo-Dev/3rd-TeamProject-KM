using System.Collections.Generic;

namespace BlueMarble
{
    public class DropProbDef
    {
        public int Id { get; private set; }
        public Dictionary<EquipmentGrade, float> Probs { get; private set; }
        public int GainCount { get; private set; }
        
        public DropProbDef(int id, Dictionary<EquipmentGrade, float> probs, int gainCount)
        {
            Id = id;
            Probs = probs;
            GainCount = gainCount;
        }
    }
}