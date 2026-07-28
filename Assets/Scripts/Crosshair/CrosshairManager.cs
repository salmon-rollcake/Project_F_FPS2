using UnityEngine;
using UnityEngine.UI;

namespace MyFPS2
{

    public class CrosshairManager : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField] private Image crosshairImage;

        [Header("Detection Settings")]
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Transform cameraTransform;

        private CrosshairData _currentData;
        private float _currentWeaponRange = 50f;
        private bool _isTargetAcquired = false;

        private RectTransform _rectTransform;

        private void Awake()
        {
            if (crosshairImage != null)
            {
                _rectTransform = crosshairImage.rectTransform;
            }

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            DetectEnemy();
            UpdateCrosshairVisuals();
        }

        /// <summary>
        /// 무기가 변경될 때 호출하여 현재 무기의 CrosshairData 및 Range를 연동
        /// </summary>
        public void SetCrosshair(WeaponData weaponData)
        {
            if (weaponData == null || weaponData.crosshairData == null)
            {
                crosshairImage.gameObject.SetActive(false);
                _currentData = null;
                return;
            }

            _currentData = weaponData.crosshairData;
            _currentWeaponRange = weaponData.range;

            // 스프라이트 적용 및 활성화
            crosshairImage.sprite = _currentData.crosshairSprite;
            crosshairImage.gameObject.SetActive(true);

            // 초기 상태 즉시 반영
            _isTargetAcquired = false;
            _rectTransform.sizeDelta = _currentData.defaultSize;
            crosshairImage.color = _currentData.defaultColor;
        }

        /// <summary>
        /// 카메라 정면으로 무기의 사거리만큼 레이캐스트를 날려 적을 감지
        /// </summary>
        private void DetectEnemy()
        {
            if (_currentData == null || cameraTransform == null) return;

            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, _currentWeaponRange, enemyLayer))
            {
                _isTargetAcquired = true;
            }
            else
            {
                _isTargetAcquired = false;
            }
        }

        /// <summary>
        /// 포착 여부에 따른 크기 및 색상 부드러운 보간(Lerp) 처리
        /// </summary>
        private void UpdateCrosshairVisuals()
        {
            if (_currentData == null) return;

            Vector2 targetSize = _isTargetAcquired ? _currentData.targetAcquiredSize : _currentData.defaultSize;
            Color targetColor = _isTargetAcquired ? _currentData.targetAcquiredColor : _currentData.defaultColor;

            float speed = Time.deltaTime * _currentData.transitionSpeed;

            // 크기 보간
            _rectTransform.sizeDelta = Vector2.Lerp(_rectTransform.sizeDelta, targetSize, speed);

            // 색상 보간
            crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, speed);
        }
    }
}
