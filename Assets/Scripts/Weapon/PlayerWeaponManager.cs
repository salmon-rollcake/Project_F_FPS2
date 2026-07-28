using UnityEngine;
using System.Collections.Generic;

namespace MyFPS2
{

    public class PlayerWeaponManager : MonoBehaviour
    {
        [Header("Weapon Sockets")]
        [SerializeField] private Transform weaponParentSocket;
        [SerializeField] private Transform defaultWeaponPosition;

        [Header("Initial Loadout")]
        [SerializeField] private List<WeaponData> startingWeapons = new List<WeaponData>();

        [Header("UI Reference")]
        [SerializeField] private CrosshairManager crosshairManager;

        private readonly List<WeaponController> _equippedWeapons = new List<WeaponController>();
        private int _currentWeaponIndex = -1;
        private bool _isSwapping = false;

        private void Start()
        {
            InitializeWeapons();
        }

        private void Update()
        {
            HandleInput();
        }

        private void InitializeWeapons()
        {
            if (startingWeapons.Count == 0) return;

            foreach (var data in startingWeapons)
            {
                if (data == null || data.weaponPrefab == null) continue;

                // 1. WeaponParentSocket의 자식으로 생성
                GameObject instance = Instantiate(data.weaponPrefab, weaponParentSocket);

                if (!instance.TryGetComponent<WeaponController>(out var weaponCtrl))
                {
                    weaponCtrl = instance.AddComponent<WeaponController>();
                }

                weaponCtrl.Initialize(data);

                // 2. 일단 Socket 위치로 정렬 후 비활성화
                weaponCtrl.SnapToTransform(weaponParentSocket);
                instance.SetActive(false);

                _equippedWeapons.Add(weaponCtrl);
            }

            // 첫 번째 무기 활성화 및 DefaultPosition 위치로 설정
            if (_equippedWeapons.Count > 0)
            {
                EquipWeaponInstant(0);
            }
        }

        private void EquipWeaponInstant(int index)
        {
            _currentWeaponIndex = index;
            WeaponController activeWeapon = _equippedWeapons[_currentWeaponIndex];

            // DefaultWeaponPosition 위치로 즉시 설정 및 활성화
            activeWeapon.SnapToTransform(defaultWeaponPosition);
            activeWeapon.gameObject.SetActive(true);

            // 크로스헤어 연동
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

            // 1) 현재 활성 무기: DefaultPosition -> Socket 위치로 이동 후 비활성화
            currentWeapon.AnimateToTransform(weaponParentSocket, false, () =>
            {
                // 2) 교체할 무기: 이동 시작 전 Socket 위치로 정렬
                nextWeapon.SnapToTransform(weaponParentSocket);

                // 3) Socket -> DefaultPosition 위치로 이동 후 활성화
                nextWeapon.AnimateToTransform(defaultWeaponPosition, true, () =>
                {
                    _currentWeaponIndex = newIndex;
                    _isSwapping = false;

                    // 교체 완료 시 크로스헤어 업데이트
                    if (crosshairManager != null && nextWeapon.Data != null)
                    {
                        crosshairManager.SetCrosshair(nextWeapon.Data);
                    }
                });
            });
        }
    }
}