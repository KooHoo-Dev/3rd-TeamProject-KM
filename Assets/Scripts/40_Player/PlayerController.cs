using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using BlueMarble;
using Cysharp.Threading.Tasks;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        private const float PositionSendInterval = 0.1f;
        private const float NetworkPositionLerpSpeed = 15f;

        private CharacterController2D controller2D;
        private InventoryManager myInventory;
        private EquipmentCaster myCaster;
        private float nextPositionSendTime;
        private float networkTargetX;
        private bool hasNetworkPosition;

        // false이면 움직이지 않음
        public bool isLocalPlayer = true;
        
        public bool IsMoving => Mathf.Abs(SimpleInput.GetAxisHorizontalRaw()) > 0.01f;

        public bool IsCastingSkill { get; private set; } = false;

        private void Awake()
        {
            controller2D = GetComponent<CharacterController2D>();
            myCaster = GetComponent<EquipmentCaster>();
            
            myInventory = InventoryManager.Instance;
        }
    

        public void SetInventory(InventoryManager inventory)
        {
            myInventory = inventory;
        }
        
        private void Update()
        {
            if (isLocalPlayer == false)
            {
                ApplyNetworkPosition();
                return;
            }

            HandleMovement();
            HandleDodge();

            UpdateEquipmentCooldowns();
            HandleEquipmentUse();
        }

        private void LateUpdate()
        {
            if (isLocalPlayer == false || !Combat.CombatSystem.IsInitialized) return;

            Combat.CombatSystem combatSystem = Combat.CombatSystem.Instance;
            if (!combatSystem.IsCombatActive || Time.unscaledTime < nextPositionSendTime) return;

            nextPositionSendTime = Time.unscaledTime + PositionSendInterval;
            combatSystem.ReportLocalPosition(transform.position.x);
        }

        public void SetLocalPlayer(bool value)
        {
            isLocalPlayer = value;
            nextPositionSendTime = 0f;
            networkTargetX = transform.position.x;
            hasNetworkPosition = false;
        }

        public void SetNetworkPosition(float x)
        {
            if (isLocalPlayer) return;

            networkTargetX = x;
            hasNetworkPosition = true;
        }

        private void ApplyNetworkPosition()
        {
            if (!hasNetworkPosition) return;

            Vector3 position = transform.position;
            position.x = Mathf.Lerp(
                position.x,
                networkTargetX,
                NetworkPositionLerpSpeed * Time.deltaTime);
            transform.position = position;
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
                Equipment eq = myInventory.GetAt(i);
                if (eq != null)
                {
                    eq.Cooldown(dt);
                }
            }
        }
        
        private void HandleEquipmentUse()
        {
            if (SimpleInput.GetKeyDown(Key.Digit1)) TryUseEquipmentSlot(0).Forget();
            if (SimpleInput.GetKeyDown(Key.Digit2)) TryUseEquipmentSlot(1).Forget();
            if (SimpleInput.GetKeyDown(Key.Digit3)) TryUseEquipmentSlot(2).Forget();
            if (SimpleInput.GetKeyDown(Key.Digit4)) TryUseEquipmentSlot(3).Forget();
            if (SimpleInput.GetKeyDown(Key.Digit5)) TryUseEquipmentSlot(4).Forget();
        }

        private async UniTaskVoid TryUseEquipmentSlot(int slotIndex)
        {
            Equipment eq = myInventory.GetAt(slotIndex);

            if (eq == null) return;
            
            if (eq.TryUse())
            {
                IsCastingSkill = true;
                try
                {
                    myCaster.ActivateEquipment(eq.Def);
                    await UniTask.Delay(500);
                }
                finally
                {
                    IsCastingSkill = false;
                }
            }
            else
            {
                if(eq.RemainingCooldown>0) Debug.Log($"[쿨타임] {eq.Def.Name} 쿨타임 {eq.RemainingCooldown:F1}초");
            }
        }
    }
}
