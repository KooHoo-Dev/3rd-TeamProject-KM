using BlueMarble;
using Jay;
using UnityEngine;

namespace Combat
{
    public class CombatSystem : SingletonBase<CombatSystem>
    {
        [Header("References")]
        public Timer combatTimer;
        
        [Header("Damage Multiplier")]
        public float multiplierIncreaseStartTime = 10f;
        public float multiplierIncreaseInterval = 5f;
        public float multiplierIncreaseAmount = 1f;

        public float DamageMultiplier { get; private set; } = 1.0f;
        public bool IsCombatActive { get; private set; } = false;

        private float elapsedTime;
        private float nextMultiplierIncreaseTime;

        private string attackerId;
        private string defenderId;
        private Tile battleTile;

        private float attackerHp, attackerMaxHp;
        private float defenderHp, defenderMaxHp;

        private CombatFlow combatFlow;

        protected override void OnDispose()
        {
            base.OnDispose();
            if (combatTimer != null) combatTimer.OnTimeUp -= HandleTimeUp;
        }

        public void StartCombat(string attacker, string defender, Tile battleTile, float maxHp)
        {
            attackerId = attacker;
            defenderId = defender;
            this.battleTile = battleTile;
            
            
            
            attackerMaxHp = attackerHp = maxHp;
            defenderMaxHp = defenderHp = maxHp;

            elapsedTime = 0f;
            DamageMultiplier = 1.0f;
            nextMultiplierIncreaseTime = multiplierIncreaseStartTime;

            combatTimer.OnTimeUp += HandleTimeUp;
            combatTimer.StartTimer();
            IsCombatActive = true;
        }

        private void Update()
        {
            if (!IsCombatActive) return;

            elapsedTime += Time.deltaTime;

            if (elapsedTime >= nextMultiplierIncreaseTime)
            {
                DamageMultiplier += multiplierIncreaseAmount;
                nextMultiplierIncreaseTime += multiplierIncreaseInterval;
                Debug.Log($"[CombatSystem] 데미지 배율 증가: x{DamageMultiplier:F2}");
            }
        }
        
        public void ApplyDamage(string targetId, int amount)
        {
            if (!IsCombatActive) return;

            if (targetId == attackerId) attackerHp -= amount;
            else if (targetId == defenderId) defenderHp -= amount; 

            CheckInstantWin();
        }

        private void CheckInstantWin()
        {
            if (attackerHp <= 0) EndCombat(defenderId, attackerId);
            else if (defenderHp <= 0) EndCombat(attackerId, defenderId);
        }

        private void HandleTimeUp()
        {
            float attackerRatio = attackerHp / attackerMaxHp;
            float defenderRatio = defenderHp / defenderMaxHp;

            string winner, loser;

            if (attackerRatio > defenderRatio)
            {
                winner = attackerId;
                loser = defenderId;
            }
            else if (defenderRatio > attackerRatio)
            {
                winner = defenderId;
                loser = attackerId;
            }
            else
            {
                Debug.Log("[CombatSystem] 동점으로 인한 점령자 방어 성공!");
                winner = defenderId;
                loser = attackerId;
            }

            EndCombat(winner, loser);
        }

        private void EndCombat(string winnerId, string loserId)
        {
            if (!IsCombatActive) return;

            IsCombatActive = false;
            combatTimer.StopTimer();
            combatTimer.OnTimeUp -= HandleTimeUp;

            Debug.Log($"[CombatSystem] {winnerId}의 승리!");

            if (winnerId == attackerId)
            {
                //TerritoryManager.Instance.StolenTerritory(loserId, winnerId, battleTile);
            }
        }
    }
}