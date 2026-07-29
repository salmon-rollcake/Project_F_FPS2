using UnityEngine;
using System.Collections.Generic;

namespace MyFPS2
{

    public class PlayerWeaponManager : MonoBehaviour
    {
        [Header("Weapon Sockets")]
        [SerializeField] private Transform weaponParentSocket;
        [SerializeField] private Transform defaultWeaponPosition;
        [SerializeField] private Transform aimPosition; // 조준 기준 소켓

        [Header("Camera")]
        [SerializeField] private Camera playerCamera;
        private float _defaultFOV = 60f;

        [Header("Input & Ref")]
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private CrosshairManager crosshairManager;

        [Header("Initial Loadout")]
        [SerializeField] private List<WeaponData> startingWeapons = new List<WeaponData>();

        private readonly List<WeaponController> _equippedWeapons = new List<WeaponController>();
        private int _currentWeaponIndex = -1;
        private bool _isSwapping = false;

        private void Awake()
        {
            if (inputHandler == null)
                inputHandler = GetComponentInParent<PlayerInputHandler>();

            if (playerCamera == null && Camera.main != null)
                playerCamera = Camera.main;

            if (playerCamera != null)
                _defaultFOV = playerCamera.fieldOfView;
        }

        private void Start()
        {
            InitializeWeapons();
        }

        private void Update()
        {
            HandleInput();
            HandleAiming();
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
                instance.SetActive(false);

                _equippedWeapons.Add(weaponCtrl);
            }

            if (_equippedWeapons.Count > 0)
            {
                EquipWeaponInstant(0);
            }
        }

        private void EquipWeaponInstant(int index)
        {
            _currentWeaponIndex = index;
            WeaponController activeWeapon = _equippedWeapons[_currentWeaponIndex];

            activeWeapon.SnapToTransform(defaultWeaponPosition);
            activeWeapon.gameObject.SetActive(true);

            if (crosshairManager != null && activeWeapon.Data != null)
            {
                crosshairManager.SetCrosshair(activeWeapon.Data);
            }
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

            _isSwapping = true;
            WeaponController currentWeapon = _equippedWeapons[_currentWeaponIndex];
            WeaponController nextWeapon = _equippedWeapons[newIndex];

            currentWeapon.AnimateToTransform(weaponParentSocket, false, () =>
            {
                nextWeapon.SnapToTransform(weaponParentSocket);

                nextWeapon.AnimateToTransform(defaultWeaponPosition, true, () =>
                {
                    _currentWeaponIndex = newIndex;
                    _isSwapping = false;

                    if (crosshairManager != null && nextWeapon.Data != null)
                    {
                        crosshairManager.SetCrosshair(nextWeapon.Data);
                    }
                });
            });
        }

        /// <summary>
        /// 조준 및 FOV 보간 처리
        /// </summary>
        private void HandleAiming()
        {
            if (_currentWeaponIndex < 0 || _currentWeaponIndex >= _equippedWeapons.Count || _isSwapping)
                return;

            WeaponController activeWeapon = _equippedWeapons[_currentWeaponIndex];
            WeaponData data = activeWeapon.Data;

            bool isAiming = inputHandler != null && inputHandler.IsAiming;

            // 1. 무기 Target Transform & Offset 계산
            Vector3 targetPos;
            Quaternion targetRot;
            float targetFOV;

            if (isAiming)
            {
                // AimPosition 기준 + WeaponData의 Offset 반영
                targetPos = aimPosition.TransformPoint(data.aimPositionOffset);
                targetRot = aimPosition.rotation * Quaternion.Euler(data.aimRotationOffset);
                targetFOV = data.aimFOV;
            }
            else
            {
                // DefaultPosition 기준
                targetPos = defaultWeaponPosition.position;
                targetRot = defaultWeaponPosition.rotation;
                targetFOV = _defaultFOV;
            }

            // 2. 무기 위치 및 FOV 부드럽게 이동
            float speed = data.aimSpeed;
            activeWeapon.SmoothMoveTo(targetPos, targetRot, speed);

            if (playerCamera != null)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * speed);
            }
        }
    }
}