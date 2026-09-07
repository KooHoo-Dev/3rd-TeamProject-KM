using System;
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

        public string CombatId { get; private set; }
        public string AttackerId { get; private set; }
        public string DefenderId { get; private set; }
        public string LocalPlayerId { get; private set; }
        public int StartHp { get; private set; } = 50;
        public bool IsTestCombat { get; private set; }

        public bool HasCombatAssignment =>
            !string.IsNullOrEmpty(AttackerId) &&
            !string.IsNullOrEmpty(DefenderId) &&
            !string.IsNullOrEmpty(LocalPlayerId);

        // 네트워크 담당 코드는 이 이벤트를 구독해 서버로 전달하면 된다.
        public event Action<CombatRequestMessage> OnCombatRequested;
        public event Action<HitRequestPacket> OnHitRequested;
        public event Action<CombatResultMessage> OnCombatResultRequested;
        public event Action<CombatAssignmentMessage> OnCombatAssigned;
        public event Action<CombatResultMessage> OnCombatEnded;

        private UniTaskCompletionSource<bool> combatResult;
        private readonly HashSet<string> appliedHitIds = new();
        
        // HitBox와 HurtBox가 충돌을 확인 하는 명부
        private Dictionary<Collider2D, HurtBox> HurtBoxDic { get; set; } = new Dictionary<Collider2D, HurtBox>();
        
        // 서버에서 ID를 받았을 때 체력을 깎을 본체를 찾는 명부
        private Dictionary<string, CombatEntity> entityDict =  new Dictionary<string, CombatEntity>();

        protected override void Awake()
        {
            // 보드에서 Instance가 먼저 생성된 뒤 전투 씬의 인스턴스가 로드되는 경우,
            // 전투 씬에 연결된 Timer 참조와 설정을 기존 영속 인스턴스에 넘긴다.
            if (instance != null && instance != this)
            {
                instance.combatTimer = combatTimer;
                instance.multiplierIncreaseStartTime = multiplierIncreaseStartTime;
                instance.multiplierIncreaseInterval = multiplierIncreaseInterval;
                instance.multiplierIncreaseAmount = multiplierIncreaseAmount;

                if (instance.IsCombatActive)
                    instance.StartCombatTimer();
            }

            base.Awake();
        }

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
            if (tile == null || string.IsNullOrEmpty(tile.State.OwnerId))
            {
                Debug.LogError("[CombatSystem] 대전 상대 정보가 없습니다.");
                return false;
            }

            string defenderId = tile.State.OwnerId;

            combatResult = new UniTaskCompletionSource<bool>();
            OnCombatRequested?.Invoke(new CombatRequestMessage
            {
                AttackerId = attackerId,
                DefenderId = defenderId
            });

            Debug.Log($"[CombatSystem] 대전 요청: {attackerId} vs {defenderId}");

            // 서버가 배정을 확정하면 BeginCombatFromNetwork가 호출되고,
            // 승패가 결정되면 CompleteCombatFromNetwork가 이 대기를 끝낸다.
            return await combatResult.Task;
        }

        /// <summary>
        /// 서버가 대전 인원을 확정했을 때 네트워크 계층에서 양쪽 클라이언트 모두 호출한다.
        /// </summary>
        public void BeginCombatFromNetwork(CombatAssignmentMessage assignment)
        {
            if (assignment == null ||
                string.IsNullOrEmpty(assignment.AttackerId) ||
                string.IsNullOrEmpty(assignment.DefenderId) ||
                string.IsNullOrEmpty(assignment.LocalPlayerId))
            {
                Debug.LogError("[CombatSystem] 올바르지 않은 대전 배정 정보입니다.");
                return;
            }

            CombatId = assignment.CombatId;
            AttackerId = assignment.AttackerId;
            DefenderId = assignment.DefenderId;
            LocalPlayerId = assignment.LocalPlayerId;
            StartHp = assignment.StartHp > 0 ? assignment.StartHp : 50;
            IsTestCombat = assignment.IsTest;

            combatResult ??= new UniTaskCompletionSource<bool>();

            DamageMultiplier = 1.0f;
            elapsedTime = 0f;
            nextMultiplierIncreaseTime = multiplierIncreaseStartTime;
            appliedHitIds.Clear();
            IsCombatActive = true;

            StartCombatTimer();
            OnCombatAssigned?.Invoke(assignment);

            Debug.Log(
                $"[CombatSystem] 대전 배정 완료: " +
                $"Left={AttackerId}, Right={DefenderId}, Me={LocalPlayerId}");
        }

        /// <summary>
        /// 서버가 확정한 승자를 양쪽 클라이언트에 적용한다.
        /// </summary>
        public void CompleteCombatFromNetwork(CombatResultMessage result)
        {
            if (result == null || string.IsNullOrEmpty(result.WinnerId)) return;
            if (!string.IsNullOrEmpty(CombatId) &&
                !string.IsNullOrEmpty(result.CombatId) &&
                result.CombatId != CombatId) return;

            EndCombat(result.WinnerId);
        }
        
        // 누군가가 체력이 0이 되거나 타이머가 다 되면 호출
        private void EndCombat(string winnerId)
        {
            if (!IsCombatActive) return;

            IsCombatActive = false;
            StopCombatTimer();

            bool isAttackerWin = winnerId == AttackerId;
            combatResult?.TrySetResult(isAttackerWin);

            OnCombatEnded?.Invoke(new CombatResultMessage
            {
                CombatId = CombatId,
                WinnerId = winnerId
            });
        }

        private void HandleTimeUp()
        {
            // 명부에서 양측 본체를 찾아 체력 비교
            entityDict.TryGetValue(AttackerId, out CombatEntity attacker);
            entityDict.TryGetValue(DefenderId, out CombatEntity defender);

            float attackerRatio = attacker != null ? (float)attacker.CurrentHp / attacker.MaxHp : 0f;
            float defenderRatio = defender != null ? (float)defender.CurrentHp / defender.MaxHp : 0f;

            string winnerId;

            if (attackerRatio > defenderRatio)
            {
                winnerId = AttackerId;
            }
            else if (defenderRatio > attackerRatio)
            {
                winnerId = DefenderId;
            }
            else
            {
                Debug.Log("[CombatSystem] 동점으로 인한 점령자 방어 성공!");
                winnerId = DefenderId; // 동점이면 방어자 승리 유리하게 판정
            }

            RequestOrApplyCombatResult(winnerId);
        }

        public void OnPlayerDied(string deadPlayerId)
        {
            if (!IsCombatActive) return;
            string winnerId;

            if (deadPlayerId == AttackerId) winnerId = DefenderId;
            else winnerId = AttackerId;
            
            RequestOrApplyCombatResult(winnerId, deadPlayerId);
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

        public void ApplyHitFromNetwork(HitRequestPacket packet)
        {
            if (packet == null) return;
            if (!string.IsNullOrEmpty(CombatId) &&
                !string.IsNullOrEmpty(packet.CombatId) &&
                packet.CombatId != CombatId) return;

            string hitId = $"{packet.CastId}:{packet.TargetId}";
            if (!string.IsNullOrEmpty(packet.CastId) && !appliedHitIds.Add(hitId))
                return;

            OnReceiveHitResponse(packet.TargetId, packet.Damage);
        }

        public void RequestHit(HitRequestPacket packet)
        {
            if (packet == null) return;

            if (string.IsNullOrEmpty(packet.CombatId))
                packet.CombatId = CombatId;

            if (IsTestCombat)
            {
                ApplyHitFromNetwork(packet);
                return;
            }

            // 모든 클라이언트에서 충돌이 검출되므로 피격자 본인만 서버에 보고한다.
            if (packet.TargetId != LocalPlayerId) return;

            OnHitRequested?.Invoke(packet);
        }
        
        #endregion
        
        #region Test 함수
        // CombatSystem.cs 내부에 추가
        public async UniTask<bool> StartTestCombat(
            string testAttackerId,
            string testDefenderId,
            string testLocalPlayerId = null,
            int testStartHp = 50)
        {
            combatResult = new UniTaskCompletionSource<bool>();
            BeginCombatFromNetwork(new CombatAssignmentMessage
            {
                CombatId = "TestCombat",
                AttackerId = testAttackerId,
                DefenderId = testDefenderId,
                LocalPlayerId = string.IsNullOrEmpty(testLocalPlayerId)
                    ? testAttackerId
                    : testLocalPlayerId,
                StartHp = testStartHp,
                IsTest = true
            });
    
            bool result = await combatResult.Task;

            // 전투가 끝났을 때 누가 이겼는지 로그 출력
            Debug.Log($"[테스트 전투 종료] 공격자 승리 여부: {result}");

            return result;
        }
        #endregion

        private void StartCombatTimer()
        {
            if (combatTimer == null) return;

            combatTimer.OnTimeUp -= HandleTimeUp;
            combatTimer.OnTimeUp += HandleTimeUp;
            combatTimer.StartTimer();
        }

        private void StopCombatTimer()
        {
            if (combatTimer == null) return;

            combatTimer.StopTimer();
            combatTimer.OnTimeUp -= HandleTimeUp;
        }

        private void RequestOrApplyCombatResult(string winnerId, string deadPlayerId = null)
        {
            if (IsTestCombat)
            {
                EndCombat(winnerId);
                return;
            }

            // 사망 판정은 사망한 플레이어 본인이, 시간 종료 판정은 공격자가 한 번만 보고한다.
            bool shouldReport = string.IsNullOrEmpty(deadPlayerId)
                ? LocalPlayerId == AttackerId
                : LocalPlayerId == deadPlayerId;

            if (!shouldReport) return;

            OnCombatResultRequested?.Invoke(new CombatResultMessage
            {
                CombatId = CombatId,
                WinnerId = winnerId
            });
        }
    }
}
