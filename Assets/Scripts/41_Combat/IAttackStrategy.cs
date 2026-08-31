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
            Debug.Log($"[전략 패턴] 기본 공격 실행! (대미지: {damage})");
            // GameObject instance = Object.Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        }
    }

    public class StraightAttackStrategy : IAttackStrategy
    {
        public void Execute(Transform spawnPoint, Transform targetPoint, int damage, string attackerName, GameObject prefab, EquipmentDef def = null)
        {
            Debug.Log($"[전략 패턴 - 직선] {def?.Name} 사용됨! (대미지: {damage})");
            // GameObject instance = Object.Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        }
    }
    
    public class ParabolaAttackStrategy : IAttackStrategy
    {
        public void Execute(Transform spawnPoint, Transform targetPoint, int damage, string attackerName, GameObject prefab, EquipmentDef def = null)
        {
            Debug.Log($"[전략 패턴 - 포물선] {def?.Name} 사용됨! (대미지: {damage})");
        }
    }
    
    public class AreaAttackStrategy : IAttackStrategy
    {
        public void Execute(Transform spawnPoint, Transform targetPoint, int damage, string attackerName, GameObject prefab, EquipmentDef def = null)
        {
            Debug.Log($"[전략 패턴 - 범위] {def?.Name} 사용됨! (대미지: {damage})");
        }
    }
}