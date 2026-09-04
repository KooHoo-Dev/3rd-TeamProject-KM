using BlueMarble;
using UnityEngine;

namespace Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class ParabolaProjectile : MonoBehaviour
    {
        [Header("포물선 설정")] public float arcHeight = 2.5f;
        
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
            
            float gravity = Mathf.Abs(Physics2D.gravity.y*rBody.gravityScale);
            float apex = Mathf.Max(arcHeight, displacement.y+0.5f);
            
            float timeToApex = Mathf.Sqrt(2f*apex/gravity);
            float timeFromApexToTarget = Mathf.Sqrt(2f * (apex - displacement.y) / gravity);
            float totalTime = timeToApex + timeFromApexToTarget;

            float vx = displacement.x / totalTime;
            float vy = gravity * timeToApex;

            rBody.linearVelocity = new Vector2(vx, vy);
            
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