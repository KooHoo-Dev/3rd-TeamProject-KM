using BlueMarble;
using Jay;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Combat
{
    public class SkillCastMessage
    {
        public string Type { get; set; } = "skillCast";
        public string CombatId { get; set; }
        public string CastId { get; set; }
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
        private readonly HashSet<string> executedCastIds = new();

        // 네트워크 담당 코드는 이 이벤트를 구독해 msg를 서버로 보내면 된다.
        public event Action<SkillCastMessage> OnSkillCastRequested;
        
        public GameObject basicArrowPrefab;
        private IAttackStrategy basicAttackStrategy = new BasicAttackStrategy();
        
        public EquipmentTable equipmentTable;

        protected override void Awake()
        {
            // 네트워크 수신으로 빈 싱글톤이 먼저 만들어졌다면,
            // 전투 씬에 직렬화된 프리팹 참조를 기존 인스턴스로 전달한다.
            if (instance != null && instance != this && basicArrowPrefab != null)
                instance.basicArrowPrefab = basicArrowPrefab;

            base.Awake();
        }

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

        public void CastLocal(SkillCastMessage msg)
        {
            if (msg == null) return;

            if (string.IsNullOrEmpty(msg.CombatId))
                msg.CombatId = CombatSystem.Instance.CombatId;

            if (string.IsNullOrEmpty(msg.CastId))
                msg.CastId = Guid.NewGuid().ToString("N");

            ExecuteSkill(msg);
            OnSkillCastRequested?.Invoke(msg);
        }

        public void ExecuteSkillFromNetwork(SkillCastMessage msg)
        {
            ExecuteSkill(msg);
        }

        private void ExecuteSkill(SkillCastMessage msg)
        {
            if (msg == null) return;

            string activeCombatId = CombatSystem.Instance.CombatId;
            if (!string.IsNullOrEmpty(activeCombatId) &&
                !string.IsNullOrEmpty(msg.CombatId) &&
                msg.CombatId != activeCombatId) return;

            // 로컬 생성 공격이 서버를 통해 되돌아와도 중복 생성하지 않는다.
            if (!string.IsNullOrEmpty(msg.CastId) && !executedCastIds.Add(msg.CastId))
                return;

            Vector3 spawnPoint = new Vector3(msg.SpawnX, msg.SpawnY, 0f);

            // 기본 공격인 경우
            if (msg.IsBasicAttack)
            {
                int finalDamage = Mathf.RoundToInt(5 * CombatSystem.Instance.DamageMultiplier);

                basicAttackStrategy.Execute(
                    spawnPoint,
                    msg.Direction,
                    finalDamage,
                    msg.CasterId,
                    msg.CastId,
                    basicArrowPrefab);
            }
            // 장비 스킬인 경우
            else
            {
                EquipmentDef def = equipmentTable.Get(msg.EquipmentId);
                if (def != null)
                {
                    GameObject prefab = Resources.Load<GameObject>(def.PrefabPath.Trim());
                    
                    if (prefab == null) Debug.Log($"[오류] {def.PrefabPath} 프리팹을 찾을 수 없습니다!");
                    
                    if (strategies.TryGetValue(def.AttackType, out IAttackStrategy strategy))
                    {
                        int finalDamage = Mathf.RoundToInt(def.BaseDamage * CombatSystem.Instance.DamageMultiplier);
                        strategy.Execute(
                            spawnPoint,
                            msg.Direction,
                            finalDamage,
                            msg.CasterId,
                            msg.CastId,
                            prefab,
                            def);
                    }
                }
            }
        }
    }
}
