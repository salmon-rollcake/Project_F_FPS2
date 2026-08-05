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

        [Header("UI Reference")]
        [SerializeField] private WeaponHUDManager hudManager; // 통합 HUD 매니저 참조

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
            if (weaponRecoil == null) weaponRecoil = GetComponent<WeaponRecoil>();
            if (playerCamera == null && Camera.main != null) playerCamera = Camera.main;
            if (playerCamera != null) _defaultFOV = playerCamera.fieldOfView;
        }

        private void Start()
        {
            InitializeWeapons();

            // 보유 무기 개수에 맞춰 HUD 슬롯 생성 및 초기화
            if (hudManager != null && _equippedWeapons.Count > 0)
            {
                hudManager.InitializeHUD(_equippedWeapons.Count);
                hudManager.RegisterWeaponEvents(_equippedWeapons);
                hudManager.UpdateHUD(_equippedWeapons, _currentWeaponIndex);
            }
        }

        private void Update()
        {
            HandleInput();
            HandleAimingAndVisuals();

            // 현재 무기 사격 로직 처리
            if (_currentWeaponIndex >= 0 && _currentWeaponIndex < _equippedWeapons.Count && !_isSwapping)
            {
                _equippedWeapons[_currentWeaponIndex].HandleFiring(inputHandler);
            }

            // 실시간 HUD (탄약 수치, 게이지, 선택 상태 등) 업데이트
            UpdateHUD();
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

            // 숫자키 조작 (1~9번 무기)
            for (int i = 0; i < _equippedWeapons.Count; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SwitchWeapon(i);
                    return;
                }
            }

            // 키보드 E/Q 및 마우스 휠 조작
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
                    UpdateHUD();
                });
            });
        }

        private void UpdateCrosshairForWeapon(WeaponData data)
        {
            if (crosshairManager == null || data == null) return;

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

            // 1. Target Position & FOV 연산
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

            // 2. Sway, Bobbing, Recoil 연산
            Vector3 swayPos = Vector3.zero;
            Quaternion swayRot = Quaternion.identity;
            Vector3 bobPos = Vector3.zero;

            Vector3 recoilPos = Vector3.zero;
            Quaternion recoilRot = Quaternion.identity;

            if (weaponSwayAndBob != null && !_isInSnipeScopeMode)
            {
                weaponSwayAndBob.CalculateSway(data, isAimingInput, out swayPos, out swayRot);
                bobPos = weaponSwayAndBob.CalculateBobbing(data, isAimingInput);
            }

            if (weaponRecoil != null && !_isInSnipeScopeMode)
            {
                weaponRecoil.CalculateRecoil(data, out recoilPos, out recoilRot);
            }

            // 3. 최종 위치 및 회전 연산 적용
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

        /// <summary>
        /// HUD 매니저에 현재 상태 전파
        /// </summary>
        public void UpdateHUD()
        {
            if (hudManager != null && _equippedWeapons.Count > 0)
            {
                hudManager.UpdateHUD(_equippedWeapons, _currentWeaponIndex);
            }
        }
    }
}
