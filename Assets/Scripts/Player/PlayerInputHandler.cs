using UnityEngine;

namespace MyFPS2
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool JumpTriggered { get; private set; }
        public bool IsAiming { get; private set; } // 조준 유지 여부

        private void Update()
        {
            // 이동 및 마우스
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(moveX, moveZ).normalized;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            LookInput = new Vector2(mouseX, mouseY);

            // 행동
            IsSprinting = Input.GetKey(KeyCode.LeftShift);
            IsAiming = Input.GetMouseButton(1); // 마우스 우클릭 유지 시 true

            if (Input.GetButtonDown("Jump"))
            {
                JumpTriggered = true;
            }
        }

        public void ResetJumpTrigger()
        {
            JumpTriggered = false;
        }
    }
}