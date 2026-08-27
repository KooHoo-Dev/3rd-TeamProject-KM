using System.Collections;
using UnityEngine;

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

        private Coroutine attackCoroutine;

        private void OnEnable()
        {
            attackCoroutine = StartCoroutine(AutoAttackRoutine());
        }

        private void OnDisable()
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        }

        private IEnumerator AutoAttackRoutine()
        {
            while (true)
            {
                if (CombatSystem.Instance != null && CombatSystem.Instance.IsCombatActive)
                {
                    FireArrow();
                }

                yield return new WaitForSeconds(attackInterval);
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
