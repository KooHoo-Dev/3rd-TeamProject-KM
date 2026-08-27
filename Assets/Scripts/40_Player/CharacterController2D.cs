using System.Collections;
using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Study.Utilities
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterController2D : MonoBehaviour
    {
        private const float GROUND_STICK_SPEED = -2.0f;

        [Header("Settings")]
        public float        speed               = 4.0f;
        public float        gravity             = -9.81f; 
        public float        groundCheckDistance = 0.5f;  
        public float        skinWidth           = 0.02f;
        public float        dodgeSpeedBonus     = 2.0f;
        public float        dodgeDuration       = 0.2f;   
        public LayerMask    collisionLayer;

        private Rigidbody2D rBody;
        private Collider2D  col;
        
        private Vector3 externalDelta       = Vector3.zero;   
        private float   moveInput           = 0.0f;

        private Coroutine dodgeRoutine = null;
        
        public float VerticalVelocity { get; private set; } = 0.0f;
        public bool IsGrounded { get; private set; } = false;
        public bool IsInvincible { get; private set; } = false;  // 무적 상태인가?


        #region Unity Methods

        private void Awake()
        {
            rBody = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            rBody.bodyType = RigidbodyType2D.Kinematic;
        }

        private void FixedUpdate()
        {
            IsGrounded = CheckGrounded();

            ApplyGravaity();

            Vector3 desireMove =
                new Vector3(moveInput * speed, VerticalVelocity) * Time.fixedDeltaTime
                + externalDelta;

            externalDelta = Vector3.zero;

            Vector3 moveVector = Vector3.zero;
            moveVector.x = ResolveAxisMovement(desireMove.x, Vector3.right);
            moveVector.y = ResolveAxisMovement(desireMove.y, Vector3.up);

            if (moveVector.y != desireMove.y) VerticalVelocity = 0.0f;

            rBody.MovePosition(transform.position + moveVector);
        }

        #endregion

        #region Public Methods (명령 통로)

        public void SetMoveInput(float horizontal)
        {
            moveInput = Mathf.Clamp(horizontal, -1.0f, 1.0f);
        }

        public void AddExternalMovement(Vector3 delta)
        {
            externalDelta += delta;
        }
        
        // 회피 상태. 이동 입력이 있으면 해당 방향으로 빠르게 이동하며 일시적으로 무적 상태가 된다.
        // 없으면 제자리에서 무적 상태만 얻는다. 이미 회피 중이면 무시한다.
        public void Dodge(float direction)
        {
            if (IsInvincible) return;

            dodgeRoutine = StartCoroutine(DodgeCoroutine(direction));
        }

        #endregion

        #region Private Methods (운동 파이프라인)

        private void ApplyGravaity()
        {
            if (IsGrounded && VerticalVelocity <= 0)
            {
                VerticalVelocity += GROUND_STICK_SPEED;
                return;
            }

            VerticalVelocity += gravity * Time.fixedDeltaTime;
        }

        private float ResolveAxisMovement(float move, Vector3 axis)
        {
            if (move == 0.0f) return 0.0f;

            Vector3 dir = (move > 0.0f ? axis : -axis);
            float distance = Mathf.Abs(move);
            Bounds box = GetCastBox();
            
            RaycastHit2D hit = Physics2D.BoxCast(
                box.center, box.size, 0.0f, dir, distance, collisionLayer);

            if (hit.collider == null) return move;

            float allowed = Mathf.Max(0, hit.distance - skinWidth);

            return Mathf.Sign(move) * allowed;
        }

        private Bounds GetCastBox()
        {
            Bounds box = col.bounds; 

            Vector3 size = box.size;
            size.x -= skinWidth * 2;
            size.y -= skinWidth * 2;
            box.size = size;

            return box;
        }

        private bool CheckGrounded()
        {
            Bounds box = GetCastBox();
            RaycastHit2D hit = Physics2D.BoxCast(
                box.center, box.size, 0.0f, Vector2.down, groundCheckDistance, collisionLayer);

            bool result = (hit.collider != null);
            return result;
        }

        #endregion

        #region Dodge Pipeline

        private IEnumerator DodgeCoroutine(float direction)
        {
            IsInvincible = true;

            float dir = Mathf.Abs(direction) > 0.01f ? Mathf.Sign(direction) : 0.0f;
            float elapsed = 0.0f;

            while (elapsed < dodgeDuration)
            {
                if (dir != 0.0f)
                {
                    AddExternalMovement(Vector3.right * dir * (speed + dodgeSpeedBonus) * Time.fixedDeltaTime);
                }

                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            IsInvincible = false;
            dodgeRoutine = null;
        }

        #endregion
    }
}
