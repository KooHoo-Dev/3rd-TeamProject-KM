using UnityEngine;

namespace Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Arrow : MonoBehaviour
    {
        public float arcHeight = 3f;  // 포물선 높이 조절용 변수

        private Rigidbody2D rBody;
        private int damage;
        private string attackPlayer;

        private void Awake()
        {
            rBody = GetComponent<Rigidbody2D>();
        }
        
        public void Launch(Vector2 targetPosition, int damageAmount, string shooter)
        {
            damage = damageAmount;
            attackPlayer = shooter;

            Vector2 start = transform.position;
            Vector2 displacement = targetPosition - start;
            float gravity = Mathf.Abs(Physics2D.gravity.y * rBody.gravityScale);
            
            float apex = Mathf.Max(arcHeight, displacement.y + 0.1f);
            
            float timeToApex = Mathf.Sqrt(2f * apex / gravity);
            float timeFromApexToTarget = Mathf.Sqrt(2f * (apex - displacement.y) / gravity);
            float totalTime = timeToApex + timeFromApexToTarget;

            float vx = displacement.x / totalTime;
            float vy = gravity * timeToApex;

            rBody.linearVelocity = new Vector2(vx, vy);
            
            Destroy(gameObject, totalTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Dummy>(out Dummy dummy))
            {
                CombatSystem.Instance.ReportDamage(attackPlayer, dummy, damage);
                Destroy(gameObject);
            }
        }
    }
}
