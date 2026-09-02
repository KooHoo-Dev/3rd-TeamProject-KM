using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using BlueMarble;
using Cysharp.Threading.Tasks;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        private CharacterController2D controller2D;
        private InventoryManager myInventory;
        private EquipmentCaster myCaster;

        public bool IsMoving => Mathf.Abs(SimpleInput.GetAxisHorizontalRaw()) > 0.01f;

        public bool IsCastingSkill { get; private set; } = false;

        private void Awake()
        {
            controller2D = GetComponent<CharacterController2D>();
            myCaster = GetComponent<EquipmentCaster>();

            // 테스트 용
            myInventory = InventoryManager.Instance;
        }
    

        public void SetInventory(InventoryManager inventory)
        {
            myInventory = inventory;
        }
        
        private void Update()
        {
            HandleMovement();
            HandleDodge();

            UpdateEquipmentCooldowns();
            HandleEquipmentUse();
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

        private void UpdateEquipmentCooldowns()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < InventoryManager.MaxSlotCount; i++)
            {
                EquipmentInstance eq = myInventory.GetAt(i);
                if (eq != null)
                {
                    eq.Cooldown(dt);
                }
            }
        }
        
        private void HandleEquipmentUse()
        {
            if (SimpleInput.GetKeyDown(Key.Digit1)) TryUseEquipmentSlot(0);
            if (SimpleInput.GetKeyDown(Key.Digit2)) TryUseEquipmentSlot(1);
            if (SimpleInput.GetKeyDown(Key.Digit3)) TryUseEquipmentSlot(2);
            if (SimpleInput.GetKeyDown(Key.Digit4)) TryUseEquipmentSlot(3);
            if (SimpleInput.GetKeyDown(Key.Digit5)) TryUseEquipmentSlot(4);
        }

        private async UniTaskVoid TryUseEquipmentSlot(int slotIndex)
        {
            EquipmentInstance eq = myInventory.GetAt(slotIndex);

            if (eq == null) return;
            
            if (eq.TryUse())
            {
                IsCastingSkill = true;
                
                myCaster.ActivateEquipment(eq.Def);
                
                await UniTask.Delay(500); 
                
                IsCastingSkill = false;
            }
        }
    }
}