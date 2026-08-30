using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Study.Utilities
{
    [RequireComponent(typeof(Collider2D))]
    public class CharacterController2D : MonoBehaviour
    {
        [Header("Settings")]
        public float        speed               = 4.0f;
        public float        skinWidth           = 0.02f;
        public float        dodgeSpeedBonus     = 2.0f;
        public float        dodgeDuration       = 0.2f;   
        public LayerMask    collisionLayer;

        private Collider2D  col;
        
        private Vector3 externalDelta       = Vector3.zero;   
        private float   moveInput           = 0.0f;

        private CancellationTokenSource dodgeCts;

        public bool IsInvincible { get; private set; } = false;


        #region Unity Methods

        private void Awake()
        {
            col = GetComponent<Collider2D>();
        }

        private void Update()
        {
            Vector3 desireMove =
                new Vector3(moveInput * speed, 0) * Time.fixedDeltaTime
                + externalDelta;

            externalDelta = Vector3.zero;

            Vector3 moveVector = Vector3.zero;
            moveVector.x = ResolveAxisMovement(desireMove.x, Vector3.right);
            
            transform.position += moveVector;
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

        #endregion

        #region Private Methods (운동 파이프라인)

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

        #endregion

        #region Dodge Pipeline

        public async UniTaskVoid Dodge(float direction)
        {
            if (IsInvincible) return;

            IsInvincible = true;
            
            dodgeCts?.Cancel();
            dodgeCts?.Dispose();
            dodgeCts = new CancellationTokenSource();
            
            float dir = Mathf.Abs(direction) > 0.01f ? Mathf.Sign(direction) : 0.0f;
            float elapsed = 0.0f;

            try
            {
                while (elapsed < dodgeDuration)
                {
                    if (dir != 0.0f)
                    {
                        AddExternalMovement(Vector3.right * dir * (speed + dodgeSpeedBonus) * Time.deltaTime);
                    }

                    elapsed += Time.deltaTime;
                    
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: dodgeCts.Token);
                }
            }
            catch (System.OperationCanceledException)
            {
                
            }
            finally
            {
                IsInvincible = false;
                dodgeCts?.Dispose();
                dodgeCts = null;
            }
        }

        #endregion
    }
}
