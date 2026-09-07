using BlueMarble;
using Jay;
using UnityEngine;
using System.Collections.Generic;

namespace Combat
{
    public class SkillCastMessage
    {
        public string Type { get; set; } = "skillCast";
        public string CasterId { get; set; }
    
        public bool IsBasicAttack { get; set; } // 기본 공격인지 장비 공격인지 확인. true면 기본 공격
    
        public int EquipmentId { get; set; }
        public float SpawnX { get; set; }
        public float SpawnY { get; set; }
        public float Direction { get; set; } 
    }
    
    public class BattleManager : SingletonBase<BattleManager>
    {
        private Dictionary<AttackType, IAttackStrategy> strategies;
        
        public GameObject basicArrowPrefab;
        private IAttackStrategy basicAttackStrategy = new BasicAttackStrategy();
        
        public EquipmentTable equipmentTable;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            strategies = new Dictionary<AttackType, IAttackStrategy>
            {
                { AttackType.Straight, new StraightAttackStrategy() },
                { AttackType.Parabola, new ParabolaAttackStrategy() },
                { AttackType.Area, new AreaAttackStrategy() }
            };
            
            equipmentTable = new EquipmentTable();
            LoadTableAsync();
        }
        
        private async void LoadTableAsync()
        {
            await equipmentTable.LoadAsync();
        }

        public void ExecuteSkillFromNetwork(SkillCastMessage msg)
        {
            Vector3 spawnPoint = new Vector3(msg.SpawnX, msg.SpawnY, 0f);
            Vector3 targetPoint = Vector3.zero;

            // 기본 공격인 경우
            if (msg.IsBasicAttack)
            {
                float basicRange = 5f; // 기본 공격 사거리
                targetPoint = spawnPoint + (Vector3.right * msg.Direction * basicRange);
                
                int basicDamage = 10; 

                basicAttackStrategy.Execute(spawnPoint, targetPoint, basicDamage, msg.CasterId, basicArrowPrefab);
            }
            // 장비 스킬인 경우
            else
            {
                EquipmentDef def = equipmentTable.Get(msg.EquipmentId);
                if (def != null)
                {
                    targetPoint = spawnPoint + (Vector3.right * msg.Direction * def.Range);
                    GameObject prefab = Resources.Load<GameObject>(def.PrefabPath.Trim());
                    
                    if (prefab == null) Debug.Log($"[오류] {def.PrefabPath} 프리팹을 찾을 수 없습니다!");
                    
                    if (strategies.TryGetValue(def.AttackType, out IAttackStrategy strategy))
                    {
                        strategy.Execute(spawnPoint, targetPoint, (int)def.BaseDamage, msg.CasterId, prefab, def);
                    }
                }
            }
        }
    }
}