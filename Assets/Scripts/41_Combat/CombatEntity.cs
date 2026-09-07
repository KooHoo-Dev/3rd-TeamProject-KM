using UnityEngine;

namespace Combat
{
    public abstract class CombatEntity : MonoBehaviour
    {
        [field: SerializeField] public string OwnerId { get; private set;}
        
        public int CurrentHp { get; protected set; }
        public int MaxHp { get; protected set; }

        public virtual void Init(string playerId, int maxHp)
        {
            OwnerId = playerId;
            CurrentHp = maxHp;
            MaxHp = maxHp;

            if (CombatSystem.Instance != null)
            {
                CombatSystem.Instance.RegisterEntity(this);
            }
        }

        public void ApplyHealthSnapshot(int currentHp)
        {
            CurrentHp = Mathf.Clamp(currentHp, 0, MaxHp);
        }

        protected virtual void OnDisable()
        {
            if (CombatSystem.IsInitialized)
                CombatSystem.Instance.UnregisterEntity(this);
        }

        public abstract void TakeDamage(int damage);
    }
}
