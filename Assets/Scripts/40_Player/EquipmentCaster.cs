using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using BlueMarble;
using Combat;

namespace Player
{
    public class EquipmentCaster : MonoBehaviour
    {
        [Header("References")]
        public Transform spawnPoint;
        
        [Header("Basic Attack Settings")]
        public float basicAttackInterval = 1.3f;
        public int baseDamage = 1;
        public string attackerName = "Player";

        private float nextAttackTime = 0f;

        private CancellationTokenSource attackCts;
        private PlayerController playerController;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
            attackCts = new CancellationTokenSource();
            AutoAttackRoutine(attackCts.Token).Forget();
        }

        private void OnDisable()
        {
            attackCts?.Cancel();
            attackCts?.Dispose();
            attackCts = null;
        }

        private async UniTaskVoid AutoAttackRoutine(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    bool canAttack = CombatSystem.Instance.IsCombatActive 
                                     && !playerController.IsMoving 
                                     && !playerController.IsCastingSkill;

                    if (canAttack && Time.time >= nextAttackTime)
                    {
                        SkillCastMessage msg = new SkillCastMessage 
                        {
                            CasterId = attackerName,
                            IsBasicAttack = true,
                            EquipmentId = 0,
                            SpawnX = spawnPoint.position.x,
                            SpawnY = spawnPoint.position.y,
                            Direction = 1f // 오른쪽을 보고 있다면 1, 왼쪽이면 -1
                        };

                        BattleManager.Instance.ExecuteSkillFromNetwork(msg);
                        Debug.Log($"[네트워크 전송] 기본 공격 발사 요청!");

                        nextAttackTime = Time.time + basicAttackInterval;
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
                }
            }
            catch (System.OperationCanceledException) { }
        }

        public void ActivateEquipment(EquipmentDef def)
        {
            SkillCastMessage msg = new SkillCastMessage
            {
                CasterId = attackerName,
                IsBasicAttack = false,
                EquipmentId = def.Id,
                SpawnX = spawnPoint.position.x,
                SpawnY = spawnPoint.position.y,
                Direction = 1f
            };
            
            BattleManager.Instance.ExecuteSkillFromNetwork(msg);
            Debug.Log($"[로컬 전송] {def.Name} 스킬 발사 요청!");
        }
    }
}