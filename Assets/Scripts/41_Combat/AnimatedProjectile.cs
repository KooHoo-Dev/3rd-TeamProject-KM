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

        public void Launch(float direction, int damageAmount, string attackerId)
        {
            transform.right = new Vector2(direction, 0).normalized; 

            HitBox hitScript = GetComponentInChildren<HitBox>();
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