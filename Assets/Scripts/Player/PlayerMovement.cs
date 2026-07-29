using UnityEngine;

namespace MyFPS2
{

    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerStatsData stats;
        [SerializeField] private PlayerInputHandler inputHandler;

        private CharacterController _characterController;
        private Vector3 _velocity;
        private bool _isGrounded;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            if (inputHandler == null)
                inputHandler = GetComponent<PlayerInputHandler>();
        }

        private void Update()
        {
            HandleGroundedStatus();
            HandleMovement();
            HandleJumpAndGravity();

            // 사용한 단발성 입력 리셋
            inputHandler.ResetJumpTrigger();
        }

        private void HandleGroundedStatus()
        {
            _isGrounded = _characterController.isGrounded;

            // 지면에 닿아있을 때 누적되는 음수 Y 속도 초기화
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // 지면에 단단히 붙어있도록 약간의 음수 값 유지
            }
        }

        private void HandleMovement()
        {
            Vector2 input = inputHandler.MoveInput;

            // 캐릭터 정면/우측 방향 기준으로 이동 벡터 계산 (WASD)
            Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;

            float currentSpeed = inputHandler.IsSprinting ? stats.sprintSpeed : stats.walkSpeed;

            _characterController.Move(moveDirection * (currentSpeed * Time.deltaTime));
        }

        private void HandleJumpAndGravity()
        {
            // 점프 처리
            if (inputHandler.JumpTriggered && _isGrounded)
            {
                // v = sqrt(h * -2 * g)
                _velocity.y = Mathf.Sqrt(stats.jumpHeight * -2f * stats.gravity);
            }

            // 중력 적용
            _velocity.y += stats.gravity * Time.deltaTime;

            // Y축 최종 이동 적용
            _characterController.Move(_velocity * Time.deltaTime);
        }
    }
}
