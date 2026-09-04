using BlueMarble;
using UnityEngine;

namespace Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class StraightProjectile : MonoBehaviour
    {
        [Header("투사체 속도 설정")] public float speed = 2.5f;
        
        private Rigidbody2D rBody;
        private int damage;
        private string attackerId;

        private void Awake()
        {
            rBody = GetComponent<Rigidbody2D>();
        }

        public void Launch(Vector2 targetPosition, int damageAmount, string attackerId)
        {
            damage = damageAmount;
            this.attackerId = attackerId;

            Vector2 start = transform.position;
            Vector2 displacement = targetPosition - start;

            float distance = displacement.magnitude;
            
            float totalTime = distance / speed;
            
            Vector2 direction = displacement.normalized;
            rBody.linearVelocity = direction * speed;
            
            Destroy(gameObject, totalTime);
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Dummy 대신 실제 상대방 플레이어의 스크립트로 교체하여 사용
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
                
                Destroy(gameObject);
            }
        }
    }
}