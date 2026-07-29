using UnityEngine;

namespace MyFPS2
{

    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private PlayerStatsData stats;
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private Transform cameraHolder; // FPS 카메라가 위치한 트랜스폼

        private float _xRotation = 0f;

        private void Awake()
        {
            if (inputHandler == null)
                inputHandler = GetComponent<PlayerInputHandler>();
        }

        private void Start()
        {
            // FPS 게임 필수: 마우스 커서 잠금 및 숨김
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleLook();
        }

        private void HandleLook()
        {
            Vector2 lookInput = inputHandler.LookInput * stats.mouseSensitivity;

            // 상하 회전 (카메라 X축 회전 - 반전 방지 Clamp)
            _xRotation -= lookInput.y;
            _xRotation = Mathf.Clamp(_xRotation, stats.minVerticalAngle, stats.maxVerticalAngle);

            if (cameraHolder != null)
            {
                cameraHolder.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            }

            // 좌우 회전 (플레이어 몸통 Y축 회전)
            transform.Rotate(Vector3.up * lookInput.x);
        }
    }
}
