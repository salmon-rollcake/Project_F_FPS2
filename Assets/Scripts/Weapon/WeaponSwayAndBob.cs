using UnityEngine;

namespace MyFPS2
{

    public class WeaponSwayAndBob : MonoBehaviour
    {
        [SerializeField] private PlayerInputHandler inputHandler;

        private Vector3 _swayOffsetPos;
        private Quaternion _swayOffsetRot = Quaternion.identity;

        private float _bobTimerX;
        private float _bobTimerY;
        private Vector3 _currentBobPos;

        private void Awake()
        {
            if (inputHandler == null)
                inputHandler = GetComponentInParent<PlayerInputHandler>();
        }

        /// <summary>
        /// 시점 전환(마우스) 시 발생하는 관성 Sway 계산 (기존 동일)
        /// </summary>
        public void CalculateSway(WeaponData data, bool isAiming, out Vector3 swayPos, out Quaternion swayRot)
        {
            if (data == null || inputHandler == null)
            {
                swayPos = Vector3.zero;
                swayRot = Quaternion.identity;
                return;
            }

            float multiplier = isAiming ? 0.2f : 1f;
            Vector2 lookInput = inputHandler.LookInput;

            float moveX = Mathf.Clamp(-lookInput.x * data.swayAmount * multiplier, -data.maxSwayAmount, data.maxSwayAmount);
            float moveY = Mathf.Clamp(-lookInput.y * data.swayAmount * multiplier, -data.maxSwayAmount, data.maxSwayAmount);
            Vector3 targetSwayPos = new Vector3(moveX, moveY, 0f);

            _swayOffsetPos = Vector3.Lerp(_swayOffsetPos, targetSwayPos, Time.deltaTime * data.swaySmoothness);

            float rotX = lookInput.y * data.swayRotationAmount * multiplier;
            float rotY = -lookInput.x * data.swayRotationAmount * multiplier;
            Quaternion targetSwayRot = Quaternion.Euler(rotX, rotY, rotY * 0.5f);

            _swayOffsetRot = Quaternion.Slerp(_swayOffsetRot, targetSwayRot, Time.deltaTime * data.swaySmoothness);

            swayPos = _swayOffsetPos;
            swayRot = _swayOffsetRot;
        }

        /// <summary>
        /// WASD 입력 방향에 연동되는 방향성 Bobbing 위치 계산
        /// </summary>
        public Vector3 CalculateBobbing(WeaponData data, bool isAiming)
        {
            if (data == null || inputHandler == null) return Vector3.zero;

            Vector2 moveInput = inputHandler.MoveInput; // x: A(-1)/D(1), y: S(-1)/W(1)
            bool isMoving = moveInput.sqrMagnitude > 0.01f;

            // 정지 시 타이머 리셋 및 위치 원복
            if (!isMoving)
            {
                _bobTimerX = 0f;
                _bobTimerY = 0f;
                _currentBobPos = Vector3.Lerp(_currentBobPos, Vector3.zero, Time.deltaTime * data.swaySmoothness);
                return _currentBobPos;
            }

            // 1. 배율 설정 (걷기 / 달리기 / 조준 상태 반영)
            bool isSprinting = inputHandler.IsSprinting && !isAiming;

            float frequency = isSprinting ? data.sprintBobFrequency : data.walkBobFrequency;
            float baseAmount = isSprinting ? data.sprintBobAmount : data.walkBobAmount;

            // 조준 중일 때는 흔들림 완화 (30% 배율 유지하여 조준 중 이동 시에도 손맛 제공)
            if (isAiming)
            {
                baseAmount *= 0.3f;
                frequency *= 0.8f;
            }

            // 2. 시간 진행 (W/S 수직 입력과 A/D 수평 입력 반응)
            _bobTimerY += Time.deltaTime * frequency;
            _bobTimerX += Time.deltaTime * (frequency * 0.5f);

            // 3. 방향별 위치 오프셋 계산
            // W/S 입력 (Vertical) -> Y축(상하) 흔들림 주도
            float targetY = Mathf.Sin(_bobTimerY) * baseAmount * Mathf.Abs(moveInput.y);

            // A/D 입력 (Horizontal) -> X축(좌우) 흔들림 주도 (A 입력 시 좌측, D 입력 시 우측으로 흔들림)
            float targetX = Mathf.Cos(_bobTimerX) * baseAmount * moveInput.x;

            // 약간의 Z축(앞뒤) 미세 진동으로 입체감 추가
            float targetZ = Mathf.Sin(_bobTimerY * 2f) * (baseAmount * 0.3f) * Mathf.Abs(moveInput.y);

            Vector3 targetBobPos = new Vector3(targetX, targetY, targetZ);

            // 4. 부드러운 위치 보간 적용
            _currentBobPos = Vector3.Lerp(_currentBobPos, targetBobPos, Time.deltaTime * data.swaySmoothness);

            return _currentBobPos;
        }
    }
}