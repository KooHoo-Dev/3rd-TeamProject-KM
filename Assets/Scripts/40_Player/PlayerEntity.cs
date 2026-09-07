using Combat;
using UnityEngine;

namespace Player
{
    public class PlayerEntity : CombatEntity
    {
        public override void TakeDamage(int damage)
        {
            CurrentHp -= damage;
            
            Debug.Log($"[PlayerEntity] {OwnerId} 맞음.");

            if (CurrentHp <= 0)
            {
                CurrentHp = 0;
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"[PlayerEntity] {OwnerId} 사망");

            CombatSystem.Instance.OnPlayerDied(OwnerId);
        }
    }
}