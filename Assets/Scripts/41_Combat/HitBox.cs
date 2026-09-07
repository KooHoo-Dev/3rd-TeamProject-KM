using System.Collections.Generic;
using BlueMarble;
using UnityEngine;

namespace Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class HitBox : MonoBehaviour
    {
        private int damage;
        private string attackerId;
        private string castId;

        private bool hasHit = false;
        public void Init(int damageAmount, string attackerId, string castId)
        {
            this.damage = damageAmount;
            this.attackerId = attackerId;
            this.castId = castId;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log($"[HitBox] 충돌 발생: {collision.gameObject.name}");
            // 단일 타수
            if (hasHit) return;

            HurtBox targetHurtBox = CombatSystem.Instance.GetHurtBoxOrNull(collision);
            if (targetHurtBox == null) return;

            // 본인 공격에는 타격 금지
            if (targetHurtBox.Owner.OwnerId == attackerId) return;

            hasHit = true;

            HitRequestPacket packet = new HitRequestPacket
            {
                CombatId = CombatSystem.Instance.CombatId,
                CastId = castId,
                AttackerId = attackerId,
                TargetId = targetHurtBox.Owner.OwnerId,
                Damage = damage,
            };
            
            string jsonPayload = JsonUtility.ToJson(packet);
            Debug.Log(jsonPayload);

            GetComponent<Collider2D>().enabled = false;
            

            // 테스트에서는 즉시 적용하고, 실제 대전에서는 네트워크 이벤트로 전달한다.
            CombatSystem.Instance.RequestHit(packet);
        }
    }
}
