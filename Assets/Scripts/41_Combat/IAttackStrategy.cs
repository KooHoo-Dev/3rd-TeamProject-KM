using UnityEngine;
using BlueMarble;

namespace Combat
{
    public interface IAttackStrategy
    {
        void Execute(Transform spawnPoint, Transform targetPoint, int damage, string attackerName, GameObject prefab, EquipmentDef def = null);
    }
    
    public class BasicAttackStrategy : IAttackStrategy
    {
        public void Execute(Transform spawnPoint, Transform targetPoint, int damage, string attackerName, GameObject prefab, EquipmentDef def = null)
        {
            GameObject instance = Object.Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            if (instance.TryGetComponent<Arrow>(out Arrow arrow))
            {
                arrow.Launch(targetPoint.position, damage, attackerName);
            }
        }
    }

    public class StraightAttackStrategy : IAttackStrategy
    {
        public void Execute(Transform spawnPoint, Transform targetPoint, int damage, string attackerName, GameObject prefab, EquipmentDef def = null)
        {
            GameObject instance = Object.Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            
            // TODO: def.ProjectileSpeed와 def.Range를 활용하여 직선으로 날아가는 컴포넌트 연결
        }
    }
    
}