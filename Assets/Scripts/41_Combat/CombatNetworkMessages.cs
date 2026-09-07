namespace Combat
{
    public class CombatRequestMessage
    {
        public string AttackerId { get; set; }
        public string DefenderId { get; set; }
    }

    public class CombatAssignmentMessage
    {
        public string CombatId { get; set; }
        public string AttackerId { get; set; }
        public string DefenderId { get; set; }
        public string LocalPlayerId { get; set; }
        public int StartHp { get; set; } = 50;
        public bool IsTest { get; set; }
    }

    public class CombatResultMessage
    {
        public string CombatId { get; set; }
        public string WinnerId { get; set; }
    }
}
