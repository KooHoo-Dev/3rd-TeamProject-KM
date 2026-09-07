using BlueMarble;
using UnityEngine;

namespace Combat
{
    [RequireComponent(typeof(Animator))]
    public class AnimatedProjectile : MonoBehaviour
    {
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void Launch(Vector2 targetPosition, int damageAmount, string attackerId)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            transform.right = direction; 

            ProjectileHit hitScript = GetComponentInChildren<ProjectileHit>();
            if (hitScript != null)
            {
                hitScript.Init(damageAmount, attackerId);
            }
        }

        private void Update()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.normalizedTime >= 1.0f)
            {
                Destroy(gameObject);
            }
        }
    }
}