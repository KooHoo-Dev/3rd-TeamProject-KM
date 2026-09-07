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

        private bool hasHit = false;
        public void Init(int damageAmount, string attackerId)
        {
            this.damage = damageAmount;
            this.attackerId = attackerId;
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
                AttackerId = attackerId,
                TargetId = targetHurtBox.Owner.OwnerId,
                Damage = damage,
            };
            
            string jsonPayload = JsonUtility.ToJson(packet);
            Debug.Log(jsonPayload);

            GetComponent<Collider2D>().enabled = false;
            

            // 서버를 거치지 않고 바로 CombatSystem에 데미지 적용을 명령 (테스트용)
            if (CombatSystem.Instance != null)
            {
                CombatSystem.Instance.OnReceiveHitResponse(packet.TargetId, packet.Damage);
            }
        }
    }
}