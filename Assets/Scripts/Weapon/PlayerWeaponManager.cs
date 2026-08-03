using UnityEngine;
using System.Collections.Generic;

namespace MyFPS2
{
    public class PlayerWeaponManager : MonoBehaviour
    {
        [Header("Weapon Sockets")]
        [SerializeField] private Transform weaponParentSocket;
        [SerializeField] private Transform defaultWeaponPosition;
        [SerializeField] private Transform aimPosition;

        [Header("Camera")]
        [SerializeField] private Camera playerCamera;
        private float _defaultFOV = 60f;

        [Header("Input & System Ref")]
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private CrosshairManager crosshairManager;
        [SerializeField] private WeaponSwayAndBob weaponSwayAndBob;
        [SerializeField] private SnipeScopeUI snipeScopeUI;
        [SerializeField] private ChargeUI chargeUI;
        [SerializeField] private WeaponRecoil weaponRecoil;

        [Header("Initial Loadout")]
        [SerializeField] private List<WeaponData> startingWeapons = new List<WeaponData>();

        private readonly List<WeaponController> _equippedWeapons = new List<WeaponController>();
        private int _currentWeaponIndex = -1;
        private bool _isSwapping = false;

        // Snipe 전용 상태
        private bool _isInSnipeScopeMode = false;
        private const float SNIPE_DISTANCE_THRESHOLD = 0.05f; // AimPoint 도달 판정 거리

        private void Awake()
        {
            if (inputHandler == null) inputHandler = GetComponentInParent<PlayerInputHandler>();
            if (weaponSwayAndBob == null) weaponSwayAndBob = GetComponent<WeaponSwayAndBob>();
            if (weaponRecoil == null) weaponRecoil = GetComponent<WeaponRecoil>(); // 반동 컴포넌트 참조
            if (playerCamera == null && Camera.main != null) playerCamera = Camera.main;
            if (playerCamera != null) _defaultFOV = playerCamera.fieldOfView;
        }

        private void Start()
        {
            InitializeWeapons();
        }

        private void Update()
        {
            HandleInput();
            HandleAimingAndVisuals();

            // 현재 무기 사격 로직
            if (_currentWeaponIndex >= 0 && _currentWeaponIndex < _equippedWeapons.Count && !_isSwapping)
            {
                _equippedWeapons[_currentWeaponIndex].HandleFiring(inputHandler);
            }
        }

        private void InitializeWeapons()
        {
            if (startingWeapons.Count == 0) return;

            foreach (var data in startingWeapons)
            {
                if (data == null || data.weaponPrefab == null) continue;

                GameObject instance = Instantiate(data.weaponPrefab, weaponParentSocket);

                if (!instance.TryGetComponent<WeaponController>(out var weaponCtrl))
                {
                    weaponCtrl = instance.AddComponent<WeaponController>();
                }

                weaponCtrl.Initialize(data);
                weaponCtrl.SnapToTransform(weaponParentSocket);

                // Charge UI 연동
                weaponCtrl.OnChargeProgressChanged += OnChargeProgressChanged;

                instance.SetActive(false);
                _equippedWeapons.Add(weaponCtrl);
            }

            if (_equippedWeapons.Count > 0)
            {
                EquipWeaponInstant(0);
            }
        }

        private void OnChargeProgressChanged(float ratio)
        {
            if (chargeUI != null) chargeUI.UpdateChargeProgress(ratio);
        }

        private void EquipWeaponInstant(int index)
        {
            _currentWeaponIndex = index;
            WeaponController activeWeapon = _equippedWeapons[_currentWeaponIndex];

            activeWeapon.SnapToTransform(defaultWeaponPosition);
            activeWeapon.SetWeaponVisibility(true);
            activeWeapon.gameObject.SetActive(true);

            UpdateCrosshairForWeapon(activeWeapon.Data);
        }

        private void HandleInput()
        {
            if (_isSwapping || _equippedWeapons.Count <= 1) return;

            if (Input.GetKeyDown(KeyCode.E) || Input.GetAxis("Mouse ScrollWheel") < -0.01f)
            {
                SelectNextWeapon(1);
            }
            else if (Input.GetKeyDown(KeyCode.Q) || Input.GetAxis("Mouse ScrollWheel") > 0.01f)
            {
                SelectNextWeapon(-1);
            }
        }

        private void SelectNextWeapon(int direction)
        {
            int targetIndex = (_currentWeaponIndex + direction + _equippedWeapons.Count) % _equippedWeapons.Count;
            SwitchWeapon(targetIndex);
        }

        public void SwitchWeapon(int newIndex)
        {
            if (_isSwapping || newIndex == _currentWeaponIndex || newIndex < 0 || newIndex >= _equippedWeapons.Count)
                return;

            ExitSnipeMode(); // 교체 전 저격 모드 초기화

            _isSwapping = true;
            WeaponController currentWeapon = _equippedWeapons[_currentWeaponIndex];
            WeaponController nextWeapon = _equippedWeapons[newIndex];

            currentWeapon.SetWeaponVisibility(true);
            currentWeapon.AnimateToTransform(weaponParentSocket, false, () =>
            {
                nextWeapon.SnapToTransform(weaponParentSocket);
                nextWeapon.SetWeaponVisibility(true);

                nextWeapon.AnimateToTransform(defaultWeaponPosition, true, () =>
                {
                    _currentWeaponIndex = newIndex;
                    _isSwapping = false;

                    UpdateCrosshairForWeapon(nextWeapon.Data);
                });
            });
        }

        private void UpdateCrosshairForWeapon(WeaponData data)
        {
            if (crosshairManager == null || data == null) return;

            // Snipe 타입은 기본 상태에서 크로스헤어를 비활성화
            if (data.fireType == WeaponFireType.Snipe)
            {
                crosshairManager.SetCrosshair(null);
            }
            else
            {
                crosshairManager.SetCrosshair(data);
            }
        }

        private void HandleAimingAndVisuals()
        {
            if (_currentWeaponIndex < 0 || _currentWeaponIndex >= _equippedWeapons.Count || _isSwapping)
                return;

            WeaponController activeWeapon = _equippedWeapons[_currentWeaponIndex];
            WeaponData data = activeWeapon.Data;
            bool isAimingInput = inputHandler != null && inputHandler.IsAiming;

            // 1. Target Position & FOV 계산
            Vector3 basePos;
            Quaternion baseRot;
            float targetFOV;

            if (isAimingInput)
            {
                basePos = aimPosition.TransformPoint(data.aimPositionOffset);
                baseRot = aimPosition.rotation * Quaternion.Euler(data.aimRotationOffset);

                targetFOV = (data.fireType == WeaponFireType.Snipe) ? data.snipeFOV : data.aimFOV;
            }
            else
            {
                basePos = defaultWeaponPosition.position;
                baseRot = defaultWeaponPosition.rotation;
                targetFOV = _defaultFOV;

                if (_isInSnipeScopeMode)
                {
                    ExitSnipeMode();
                }
            }

            // 2. Sway, Bobbing, Recoil 오프셋 계산
            Vector3 swayPos = Vector3.zero;
            Quaternion swayRot = Quaternion.identity;
            Vector3 bobPos = Vector3.zero;

            Vector3 recoilPos = Vector3.zero;
            Quaternion recoilRot = Quaternion.identity;

            // Sway & Bobbing (저격 스코프 모드가 아닐 때)
            if (weaponSwayAndBob != null && !_isInSnipeScopeMode)
            {
                weaponSwayAndBob.CalculateSway(data, isAimingInput, out swayPos, out swayRot);
                bobPos = weaponSwayAndBob.CalculateBobbing(data, isAimingInput);
            }

            // -------------------------------------------------------------
            // 반동 계산 (저격 스코프 모드가 아닐 때)
            // -------------------------------------------------------------
            if (weaponRecoil != null && !_isInSnipeScopeMode)
            {
                weaponRecoil.CalculateRecoil(data, out recoilPos, out recoilRot);
            }

            // 3. 최종 위치 및 회전 연산 (Sway + Bobbing + Recoil 위치 및 회전 합성)
            Vector3 finalPos = basePos + defaultWeaponPosition.TransformDirection(swayPos + bobPos + recoilPos);
            Quaternion finalRot = baseRot * swayRot * recoilRot;

            float speed = data.aimSpeed;
            activeWeapon.SmoothMoveTo(finalPos, finalRot, speed);

            if (playerCamera != null)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * speed);
            }

            // 4. Snipe 판정
            if (data.fireType == WeaponFireType.Snipe && isAimingInput)
            {
                float distToAimPoint = Vector3.Distance(activeWeapon.transform.position, basePos);

                if (distToAimPoint <= SNIPE_DISTANCE_THRESHOLD && !_isInSnipeScopeMode)
                {
                    EnterSnipeMode(activeWeapon, data);
                }
            }
        }

        private void EnterSnipeMode(WeaponController activeWeapon, WeaponData data)
        {
            _isInSnipeScopeMode = true;

            // 무기 안보이게 처리 & 스코프 UI 켜기
            activeWeapon.SetWeaponVisibility(false);
            if (snipeScopeUI != null)
            {
                snipeScopeUI.ShowScope(data.scopeOverlaySprite);
            }
        }

        private void ExitSnipeMode()
        {
            if (!_isInSnipeScopeMode) return;

            _isInSnipeScopeMode = false;

            if (_currentWeaponIndex >= 0 && _currentWeaponIndex < _equippedWeapons.Count)
            {
                _equippedWeapons[_currentWeaponIndex].SetWeaponVisibility(true);
            }

            if (snipeScopeUI != null)
            {
                snipeScopeUI.HideScope();
            }
        }
    }
}
