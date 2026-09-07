using System.Collections.Generic;
using BlueMarble;
using Cysharp.Threading.Tasks;
using Jay;
using UnityEngine;
using Sirenix.OdinInspector;

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

        private UniTaskCompletionSource<bool> combatResult;
        
        // HitBox와 HurtBox가 충돌을 확인 하는 명부
        private Dictionary<Collider2D, HurtBox> HurtBoxDic { get; set; } = new Dictionary<Collider2D, HurtBox>();
        
        // 서버에서 ID를 받았을 때 체력을 깎을 본체를 찾는 명부
        private Dictionary<string, CombatEntity> entityDict =  new Dictionary<string, CombatEntity>();

        [Button]
        public void CombatStart()
        {
            IsCombatActive = true;
        }

        private void Update()
        {
            if (!IsCombatActive) return;
            
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= nextMultiplierIncreaseTime)
            {
                DamageMultiplier += multiplierIncreaseAmount;
                nextMultiplierIncreaseTime = elapsedTime + multiplierIncreaseInterval;
                Debug.Log($"[CombatSystem] 데미지 배율 증가: x{DamageMultiplier}");
            }
        }
        
        public async UniTask<bool> StartCombat(string attackerId, Tile tile)
        {
            this.attackerId = attackerId; 
            defenderId = tile.State.OwnerId;

            DamageMultiplier = 1.0f;
            elapsedTime = 0f;
            nextMultiplierIncreaseTime = multiplierIncreaseStartTime;

            combatResult = new UniTaskCompletionSource<bool>();
            IsCombatActive = true;
            
            // 타이머 시작
            if (combatTimer != null)
            {
                combatTimer.OnTimeUp += HandleTimeUp;
                combatTimer.StartTimer();
            }
            
            bool result = await combatResult.Task;
            
            IsCombatActive = false;
            if (combatTimer != null)
            {
                combatTimer.StopTimer();
                combatTimer.OnTimeUp -= HandleTimeUp;
            }

            return result;
        }
        
        // 누군가가 체력이 0이 되거나 타이머가 다 되면 호출
        private void EndCombat(string winnerId)
        {
            bool isAttackerWin = (winnerId == attackerId);
            combatResult?.TrySetResult(isAttackerWin);
        }

        private void HandleTimeUp()
        {
            // 명부에서 양측 본체를 찾아 체력 비교
            entityDict.TryGetValue(attackerId, out CombatEntity attacker);
            entityDict.TryGetValue(defenderId, out CombatEntity defender);

            float attackerRatio = attacker != null ? (float)attacker.CurrentHp / attacker.MaxHp : 0f;
            float defenderRatio = defender != null ? (float)defender.CurrentHp / defender.MaxHp : 0f;

            string winnerId;

            if (attackerRatio > defenderRatio)
            {
                winnerId = attackerId;
            }
            else if (defenderRatio > attackerRatio)
            {
                winnerId = defenderId;
            }
            else
            {
                Debug.Log("[CombatSystem] 동점으로 인한 점령자 방어 성공!");
                winnerId = defenderId; // 동점이면 방어자 승리 유리하게 판정
            }

            EndCombat(winnerId);
        }

        public void OnPlayerDied(string deadPlayerId)
        {
            if (!IsCombatActive) return;
            string winnerId;

            if (deadPlayerId == attackerId) winnerId = defenderId;
            else winnerId = attackerId;
            
            EndCombat(winnerId);
        }
        
        #region HurtBox 물리 충돌 명부 관리

        public void AddHurtBox(Collider2D collider, HurtBox hurtBox)
        {
            HurtBoxDic.TryAdd(collider, hurtBox);
        }

        public void RemoveHurtBox(Collider2D collider)
        {
            if (HurtBoxDic.ContainsKey(collider) == false) return;
            HurtBoxDic.Remove(collider);
        }

        public HurtBox GetHurtBoxOrNull(Collider2D collider)
        {
            HurtBoxDic.TryGetValue(collider, out HurtBox hurtBox);
            return hurtBox;
        }
        #endregion
        
        #region CombatEntity 본체 명부 관리

        public void RegisterEntity(CombatEntity entity)
        {
            entityDict.TryAdd(entity.OwnerId, entity);
        }

        public void UnregisterEntity(CombatEntity entity)
        {
            if (entityDict.ContainsKey(entity.OwnerId))
            {
                entityDict.Remove(entity.OwnerId);
            }
        }

        public void OnReceiveHitResponse(string targetId, int damage)
        {
            if (entityDict.TryGetValue(targetId, out CombatEntity targetEntity))
            {
                Debug.Log($"[CombatSystem] {targetId}에게 {damage} 피해");
                targetEntity.TakeDamage(damage);
            }
        }
        
        #endregion
        
        #region Test 함수
        // CombatSystem.cs 내부에 추가
        public async UniTask<bool> StartTestCombat(string testAttackerId, string testDefenderId)
        {
            this.attackerId = testAttackerId;
            this.defenderId = testDefenderId;

            DamageMultiplier = 1.0f;
            elapsedTime = 0f;
            nextMultiplierIncreaseTime = multiplierIncreaseStartTime;

            combatResult = new UniTaskCompletionSource<bool>();
            IsCombatActive = true;
    
            if (combatTimer != null)
            {
                combatTimer.OnTimeUp += HandleTimeUp;
                combatTimer.StartTimer();
            }
    
            bool result = await combatResult.Task;
    
            IsCombatActive = false;
            if (combatTimer != null)
            {
                combatTimer.StopTimer();
                combatTimer.OnTimeUp -= HandleTimeUp;
            }

            // 전투가 끝났을 때 누가 이겼는지 로그 출력
            Debug.Log($"[테스트 전투 종료] 공격자 승리 여부: {result}");

            return result;
        }
        #endregion
    }
}