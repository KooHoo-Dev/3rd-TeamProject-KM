using Jay;
using UnityEngine;

namespace Combat
{
    public class CombatSystem : SingletonBase<CombatSystem>
    {
        [Header("Ref")]
        public Dummy dummy;
        public Timer combatTimer;

        [Header("Damage Multiplier")]
        // 대미지 배율 증가 시점(초)
        public float multiplierIncreaseStartTime = 10f;
        // 대미지 배율 증가 주기
        public float multiplierIncreaseInterval = 5f;
        // 대미지 배율 증가 폭
        public float multiplierIncreaseAmount = 1f;

        public float DamageMultiplier { get; private set; } = 1.0f;
        public bool  IsCombatActive   { get; private set; } = false;

        private float elapsedTime;
        private float nextMultiplierIncreaseTime;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();

            nextMultiplierIncreaseTime = multiplierIncreaseStartTime;

            dummy.OnDied += HandleDummyDied;
            combatTimer.OnTimeUp += HandleTimeUp;
            combatTimer.StartTimer();

            IsCombatActive = true;
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            if (dummy != null) dummy.OnDied -= HandleDummyDied;
            if (combatTimer != null) combatTimer.OnTimeUp -= HandleTimeUp;
        }

        private void Update()
        {
            if (IsCombatActive == false) return;

            elapsedTime += Time.deltaTime;

            if (elapsedTime >= nextMultiplierIncreaseTime)
            {
                DamageMultiplier += multiplierIncreaseAmount;
                nextMultiplierIncreaseTime += multiplierIncreaseInterval;

                Debug.Log($"[CombatSystem] 데미지 배율이 증가했다. 현재 배율: x{DamageMultiplier:F2}");
            }
        }
        
        public void ReportDamage(string attackePlayer, Dummy targetPlayer, int amount)
        {
            if (IsCombatActive == false) return;

            Debug.Log($"[CombatSystem] {attackePlayer}가 {targetPlayer.name}에게 {amount}의 대미지를 주었다.");
            targetPlayer.TakeDamage(amount);
        }

        private void HandleDummyDied()
        {
            EndCombat("Player");
        }

        private void HandleTimeUp()
        {
            float playerRatio = 1.0f;
            float dummyRatio  = dummy.HpRatio;

            string winner;
            if (playerRatio > dummyRatio) winner = "Player";
            else if (dummyRatio > playerRatio) winner = "Dummy";
            else winner = "Dummy";

            EndCombat(winner);
        }

        private void EndCombat(string winner)
        {
            if (IsCombatActive == false) return;

            IsCombatActive = false;
            combatTimer.StopTimer();

            Debug.Log($"[CombatSystem] {winner}의 승리!");
        }
    }
}
