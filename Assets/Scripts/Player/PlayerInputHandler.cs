using UnityEngine;

namespace MyFPS2
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool JumpTriggered { get; private set; }
        public bool IsAiming { get; private set; }

        // 발사 관련 입력
        public bool FireDown { get; private set; } // 클릭 순간 (Manual, Charge 시작)
        public bool FireHeld { get; private set; } // 클릭 유지 (Auto, Charge 진행)
        public bool FireUp { get; private set; }   // 클릭 해제 (Charge 취소)

        private void Update()
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(moveX, moveZ).normalized;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            LookInput = new Vector2(mouseX, mouseY);

            IsSprinting = Input.GetKey(KeyCode.LeftShift);
            IsAiming = Input.GetMouseButton(1);

            // 사격 입력
            FireDown = Input.GetMouseButtonDown(0);
            FireHeld = Input.GetMouseButton(0);
            FireUp = Input.GetMouseButtonUp(0);

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