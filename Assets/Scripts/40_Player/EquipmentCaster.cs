using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using BlueMarble;
using Combat;
using Utility;

namespace Player
{
    public class EquipmentCaster : MonoBehaviour
    {
        [Header("References")]
        public Transform spawnPoint;
        public Transform firePoint; 
        public GameObject basicArrowPrefab;
        
        [Header("Basic Attack Settings")]
        public float basicAttackInterval = 1.3f;
        public int baseDamage = 1;
        public string attackerName = "Player";

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

                    if (canAttack)
                    {
                        int damage = Mathf.RoundToInt(baseDamage * CombatSystem.Instance.DamageMultiplier);
                        
                        basicAttackStrategy.Execute(spawnPoint, firePoint, damage, attackerName, basicArrowPrefab);
                        
                        await CustomUniTask.DelaySeconds(basicAttackInterval, cancellationToken: token);
                    }
                    else
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
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