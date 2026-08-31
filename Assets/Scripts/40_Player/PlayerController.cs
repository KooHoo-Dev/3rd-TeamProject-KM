using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using BlueMarble;

namespace Player
{
    // 이동 및 회피 조작, 애니메이터 담당 컴포넌트.
    // PS : 아직 리소스가 없어 애니메이터는 추후 추가.
    
    public class PlayerController : MonoBehaviour
    {
        private CharacterController2D controller2D;
        private InventoryManager myInventory;

        private void Awake()
        {
            controller2D = GetComponent<CharacterController2D>();
            myInventory = InventoryManager.Instance;
        }
        
        private void Update()
        {
            HandleMovement();
            HandleDodge();
        }
        
        private void HandleMovement()
        {
            float inputVector = SimpleInput.GetAxisHorizontalRaw();
            controller2D.SetMoveInput(inputVector);
        }
        
        private void HandleDodge()
        {
            if (SimpleInput.GetKeyDown(Key.Space))
            {
                float direction = SimpleInput.GetAxisHorizontalRaw();
                _ = controller2D.Dodge(direction);
            }
        }
    }
}
