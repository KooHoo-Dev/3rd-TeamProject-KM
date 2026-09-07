using BlueMarble;
using UnityEngine;

namespace Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class ProjectileHit : MonoBehaviour
    {
        private int damage;
        private string attackerId;

        public void Init(int damageAmount, string attackerId)
        {
            damage = damageAmount;
            this.attackerId = attackerId;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent<Dummy>(out Dummy target))
            {
                HitRequestPacket packet = new HitRequestPacket
                {
                    AttackerId = attackerId,
                    TargetId = target.name,
                    Damage = damage
                };

                string jsonPayload = JsonUtility.ToJson(packet);
                
                Debug.Log($"[네트워크 전송] {target} : {jsonPayload}");
                
                // Destroy(transform.parent.gameObject);
            }
        }
    }
}