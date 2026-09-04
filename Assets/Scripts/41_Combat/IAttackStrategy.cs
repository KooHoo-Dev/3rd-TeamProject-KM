using UnityEngine;
using BlueMarble;

namespace Combat
{
    public interface IAttackStrategy
    {
        void Execute(Vector3 spawnPoint, Vector3 targetPoint, int damage, string attackerName, GameObject prefab, EquipmentDef def = null);
    }
    
    // 기본 공격 (포물선 발사)
    public class BasicAttackStrategy : IAttackStrategy
    {
        public void Execute(Vector3 spawnPoint, Vector3 targetPoint, int damage, string attackerName, GameObject prefab, EquipmentDef def = null)
        {
            if (prefab != null)
            {
                GameObject instance = Object.Instantiate(prefab, spawnPoint, Quaternion.identity);
                if (instance.TryGetComponent<ParabolaProjectile>(out ParabolaProjectile projectile))
                {
                    projectile.Launch(targetPoint, damage, attackerName);
                }
            }
        }
    }

    // 장비 스킬들 (발동 로그만 출력)
    public class StraightAttackStrategy : IAttackStrategy
    {
        public void Execute(Vector3 spawnPoint, Vector3 targetPoint, int damage, string attackerName, GameObject prefab, EquipmentDef def = null)
        {
            Debug.Log($"[전략 패턴 - 직선] {def?.Name} 장비 사용 완료 (대미지: {damage})");
        }
    }
    
    public class ParabolaAttackStrategy : IAttackStrategy
    {
        public void Execute(Vector3 spawnPoint, Vector3 targetPoint, int damage, string attackerName, GameObject prefab, EquipmentDef def = null)
        {
            Debug.Log($"[전략 패턴 - 포물선] {def?.Name} 장비 사용 완료 (대미지: {damage})");
        }
    }
    
    public class AreaAttackStrategy : IAttackStrategy
    {
        public void Execute(Vector3 spawnPoint, Vector3 targetPoint, int damage, string attackerName, GameObject prefab, EquipmentDef def = null)
        {
            Debug.Log($"[전략 패턴 - 범위] {def?.Name} 장비 사용 완료 (대미지: {damage})");
        }
    }
}