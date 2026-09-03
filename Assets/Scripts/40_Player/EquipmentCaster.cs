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
        public Vector3 spawnPoint;
        public Vector3 firePoint; 
        public GameObject basicArrowPrefab;
        
        [Header("Basic Attack Settings")]
        public float basicAttackInterval = 1.3f;
        public int baseDamage = 1;
        public string attackerName = "Player";

        private float nextAttackTime = 0f;

        private Dictionary<AttackType, IAttackStrategy> strategies;
        private IAttackStrategy basicAttackStrategy;
        
        private CancellationTokenSource attackCts;
        private PlayerController playerController;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            
            strategies = new Dictionary<AttackType, IAttackStrategy>
            {
                { AttackType.Straight, new StraightAttackStrategy() },
                {AttackType.Parabola, new ParabolaAttackStrategy()},
                { AttackType.Area , new AreaAttackStrategy()}
            };
            
            basicAttackStrategy = new BasicAttackStrategy();
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
                        int damage = Mathf.RoundToInt(baseDamage * CombatSystem.Instance.DamageMultiplier);
    
                        SkillCastMessage msg = new SkillCastMessage 
                        {
                            CasterId = attackerName,
                            IsBasicAttack = true,
                            EquipmentId = 0,
                            SpawnX = spawnPoint.x,
                            SpawnY = spawnPoint.y,
                            Direction = 1f // 오른쪽을 보고 있다면 1, 왼쪽이면 -1
                        };
    
                        // TODO: 생성한 msg를 서버로 전송합니다. (예: NetworkManager.Send(msg))
                        Debug.Log($"[네트워크 전송] 기본 공격 발사 요청!");

                        nextAttackTime = Time.time + basicAttackInterval;
                    }
                }
            }
            catch (System.OperationCanceledException) { }
        }

        public void ActivateEquipment(EquipmentDef def)
        {
            GameObject prefab = Resources.Load<GameObject>(def.PrefabPath);

            if (prefab != null)
            {
                if (strategies.TryGetValue(def.AttackType, out IAttackStrategy strategy))
                {
                    int damage = Mathf.RoundToInt(def.BaseDamage * CombatSystem.Instance.DamageMultiplier);
                    
                    strategy.Execute(spawnPoint, firePoint, damage, attackerName, prefab, def);
                    Debug.Log($"[Caster] {def.Name} 스킬 발동!");
                }
            }
        }
    }
}