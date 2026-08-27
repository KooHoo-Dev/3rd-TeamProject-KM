using System;
using UnityEngine;

namespace Combat
{
    // 테스트용 임시 더미 컴포넌트.
    // 다른 스크립트에서 더미를 참조하고 있는 부분이 많아 실제 플레이어와 전투할 때를 고려해 리팩토링해야 함.
    
    [RequireComponent(typeof(Collider2D))]
    public class Dummy : MonoBehaviour
    {
        [Header("Settings")]
        public int maxHp = 10;

        public int CurrentHp { get; private set; }
        public float HpRatio => (float)CurrentHp / maxHp;
        public bool IsDead { get; private set; } = false;

        public event Action<int> OnDamaged;
        public event Action      OnDied;

        private void Awake()
        {
            CurrentHp = maxHp;
        }

        public void TakeDamage(int damege)
        {
            if (IsDead) return;

            CurrentHp = Mathf.Max(0, CurrentHp - damege);
            OnDamaged?.Invoke(damege);

            if (CurrentHp <= 0)
            {
                IsDead = true;
                OnDied?.Invoke();
            }
        }
    }
}
