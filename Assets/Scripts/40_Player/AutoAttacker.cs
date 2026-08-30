using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utility;

namespace Combat
{
    // 기본 공격. 전투가 진행되는 동안 자동 발사된다.
    
    public class AutoAttacker : MonoBehaviour
    {
        [Header("Settings")]
        public float attackInterval = 1.3f;
        public int baseDamage = 1;
        public string attackerName = "Player";
        
        [Header("Ref")]
        public Transform spawnPoint;   // 화살 생성 위치
        public Transform firePoint;    // 화살 목표 지점
        public Arrow arrowPrefab;

        private CancellationTokenSource attackCts;
        
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
                    if (CombatSystem.Instance.IsCombatActive)
                    {
                        FireArrow();
                    }

                    await CustomUniTask.DelaySeconds(attackInterval);
                }
            }
            catch (System.OperationCanceledException)
            {
                
            }
        }

        private void FireArrow()
        {
            Arrow arrow = Instantiate(arrowPrefab, spawnPoint.position, Quaternion.identity);
            int damage = Mathf.RoundToInt(baseDamage * CombatSystem.Instance.DamageMultiplier);
            arrow.Launch(firePoint.position, damage, attackerName);
        }
    }
}
