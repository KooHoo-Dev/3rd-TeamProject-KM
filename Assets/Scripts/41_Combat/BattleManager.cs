using BlueMarble;
using Jay;
using UnityEngine;

namespace Combat
{
    public class BattleManager : SingletonBase<BattleManager>
    {
        // 공격을 하는 방식
        // 내가 이 장비를 사용한다 라는 정보를 장비Id와 발사했을 때의 위치를 BattleManager에게 보내면
        // BattleManager에서 그 정보를 받아서 그 정보에 맞는 장비 Id의 스킬 방식, 사거리, 데미지 등을 담은 프리팹을 플레이어의 위치에서 생성해 발사한다.

        public void SkillInfo(EquipmentDef equipmentId, Vector3 position)
        {
            int id = equipmentId.Id;
            
        }
    }
}