using UnityEngine;
using System;
using System.Collections;

namespace MyFPS2
{
    public class WeaponController : MonoBehaviour
    {
        public WeaponData Data { get; private set; }

        // 현재 남은 탄약 수량 (외부에서 읽기 전용)
        public int CurrentAmmo { get; private set; }
        public bool IsReloading { get; private set; }

        private Vector3 _originalScale = Vector3.one;
        private float _lastFireTime;
        private float _currentChargeTimer;
        private bool _isCharging;

        public event Action<float> OnChargeProgressChanged;

        // 재장전 진행률 이벤트 (0.0 ~ 1.0)
        public event Action<float> OnReloadProgressChanged;
        // 재장전 완료 이벤트
        public event Action OnReloadCompleted;

        private Renderer[] _weaponRenderers;
        private Coroutine _moveCoroutine;
        private Coroutine _reloadCoroutine;

        [SerializeField] private WeaponRecoil weaponRecoil;

        [Header("Muzzle Reference")]
        [SerializeField] private Transform muzzleTransform;

        private void Awake()
        {
            _weaponRenderers = GetComponentsInChildren<Renderer>();
            _originalScale = transform.localScale;

            if (weaponRecoil == null)
            {
                weaponRecoil = GetComponentInParent<WeaponRecoil>();
            }
        }

        public void Initialize(WeaponData data)
        {
            Data = data;
            _isCharging = false;
            _currentChargeTimer = 0f;

            // 무기 초기화 시 최대 탄약으로 탄약 채움
            if (Data != null)
            {
                CurrentAmmo = Data.maxAmmo;
            }

            if (weaponRecoil == null)
            {
                weaponRecoil = GetComponentInParent<WeaponRecoil>();
            }
        }

        public void HandleFiring(PlayerInputHandler input)
        {
            if (Data == null || IsReloading) return;

            // R 키 입력 시 재장전 시작
            if (Input.GetKeyDown(KeyCode.R) && CurrentAmmo < Data.maxAmmo)
            {
                StartReload();
                return;
            }

            if (CurrentAmmo <= 0) return;

            switch (Data.fireType)
            {
                case WeaponFireType.Manual:
                case WeaponFireType.Snipe:
                    if (input.FireDown && Time.time >= _lastFireTime + Data.fireRate)
                    {
                        ExecuteShoot();
                    }
                    break;

                case WeaponFireType.Auto:
                    if (input.FireHeld && Time.time >= _lastFireTime + Data.fireRate)
                    {
                        ExecuteShoot();
                    }
                    break;

                case WeaponFireType.Charge:
                    HandleChargeFire(input);
                    break;
            }
        }

        private void HandleChargeFire(PlayerInputHandler input)
        {
            if (input.FireDown)
            {
                _isCharging = true;
                _currentChargeTimer = 0f;
            }

            if (_isCharging && input.FireHeld)
            {
                _currentChargeTimer += Time.deltaTime;
                float ratio = Mathf.Clamp01(_currentChargeTimer / Data.chargeTime);
                OnChargeProgressChanged?.Invoke(ratio);

                if (_currentChargeTimer >= Data.chargeTime)
                {
                    ExecuteShoot();
                    ResetCharge();
                }
            }

            if (input.FireUp && _isCharging)
            {
                ResetCharge();
            }
        }

        private void ResetCharge()
        {
            _isCharging = false;
            _currentChargeTimer = 0f;
            OnChargeProgressChanged?.Invoke(0f);
        }

        public void StartReload()
        {
            if (IsReloading || CurrentAmmo >= Data.maxAmmo) return;

            if (_reloadCoroutine != null) StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = StartCoroutine(Co_Reload());
        }

        private IEnumerator Co_Reload()
        {
            IsReloading = true;
            float elapsed = 0f;
            float duration = (Data != null && Data.reloadTime > 0f) ? Data.reloadTime : 2.0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                
                // HUD로 재장전 Fill 게이지(0~1) 전달
                OnReloadProgressChanged?.Invoke(progress);
                yield return null;
            }

            CurrentAmmo = Data.maxAmmo;
            IsReloading = false;

            // 재장전 완료 이벤트 호출
            OnReloadCompleted?.Invoke();
        }

        // 무기 교체 등으로 취소될 경우 코루틴 정지
        private void OnDisable()
        {
            if (IsReloading)
            {
                IsReloading = false;
                if (_reloadCoroutine != null) StopCoroutine(_reloadCoroutine);
            }
        }

        private void ExecuteShoot()
        {
            // 탄약 소모
            CurrentAmmo = Mathf.Max(0, CurrentAmmo - 1);
            _lastFireTime = Time.time;

            if (Data != null && Data.bulletPrefab != null && muzzleTransform != null)
            {
                // 1. 화면 중앙(크로스헤어) 기준 목표 지점(Target Point) 계산
                Vector3 targetPoint;
                Camera mainCam = Camera.main;

                if (mainCam != null)
                {
                    // 화면 중앙에서 레이 생성
                    Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                    float maxRayDistance = 500f; // 레이캐스트 최대 거리

                    // 무언가 피격되었다면 그 지점을 목표로 설정
                    if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
                    {
                        targetPoint = hit.point;
                    }
                    else
                    {
                        // 공중이거나 부딪힌 대상이 없으면 원거리 상의 정면 지점을 목표로 설정
                        targetPoint = ray.GetPoint(maxRayDistance);
                    }
                }
                else
                {
                    // 예외 처리: 카메라인 경우 총구 정면 사용
                    targetPoint = muzzleTransform.position + muzzleTransform.forward * 100f;
                }

                // 2. 총구(Muzzle)에서 목표 지점으로 향하는 기본 방향 계산
                Vector3 baseDirection = (targetPoint - muzzleTransform.position).normalized;
                
                int count = Mathf.Max(1, Data.bulletsPerShot);

                for (int i = 0; i < count; i++)
                {
                    Vector3 fireDirection = muzzleTransform.forward;

                    // 산탄/퍼짐 각도(Spread Angle) 적용
                    if (Data.spreadAngle > 0f)
                    {
                        // baseDirection을 기준으로 임의의 산탄 회전 오프셋 적용
                        Quaternion randomSpread = Quaternion.Euler(
                            UnityEngine.Random.Range(-Data.spreadAngle, Data.spreadAngle) * 0.5f,
                            UnityEngine.Random.Range(-Data.spreadAngle, Data.spreadAngle) * 0.5f,
                            0f
                        );

                        fireDirection = Quaternion.LookRotation(baseDirection) * randomSpread * Vector3.forward;
                        fireDirection.Normalize();
                    }

                    // 탄환 생성 및 회전값 적용 (화면 중앙 목표 지점을 바라보도록 생성)
                    GameObject bulletObj = Instantiate(Data.bulletPrefab, muzzleTransform.position, Quaternion.LookRotation(fireDirection));

                    if (bulletObj.TryGetComponent<Bullet>(out var bullet))
                    {
                        bullet.Initialize(
                            fireDirection,
                            Data.bulletSpeed,
                            Data.bulletGravity,
                            Data.damage,
                            Data.bulletLifeTime,
                            Data.isExplosive,
                            Data.explosionRadius
                        );
                    }
                }
            }

            // 총구 화염 및 사운드
            if (Data != null)
            {
                if (Data.muzzleFlashPrefab != null && muzzleTransform != null)
                {
                    GameObject flash = Instantiate(Data.muzzleFlashPrefab, muzzleTransform.position, muzzleTransform.rotation, muzzleTransform);
                    Destroy(flash, 0.1f);
                }

                if (Data.fireSound != null)
                {
                    AudioSource.PlayClipAtPoint(Data.fireSound, transform.position);
                }
            }

            // 반동 트리거
            if (weaponRecoil != null && Data != null)
            {
                weaponRecoil.TriggerRecoil(Data);
            }

            Debug.Log($"[{Data.weaponName}] 사격! (남은 탄약: {CurrentAmmo}/{Data.maxAmmo})");
        }

        /// <summary>
        /// 재장전 메서드 (필요시 호출)
        /// </summary>
        public void Reload()
        {
            if (Data != null)
            {
                CurrentAmmo = Data.maxAmmo;
            }
        }

        public void SetWeaponVisibility(bool isVisible)
        {
            if (_weaponRenderers == null) return;

            foreach (var rend in _weaponRenderers)
            {
                rend.enabled = isVisible;
            }
        }

        #region Transform Movement
        public void AnimateToTransform(Transform targetTransform, bool setActiveOnComplete, Action onComplete = null)
        {
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            gameObject.SetActive(true);
            _moveCoroutine = StartCoroutine(Co_AnimateTransform(targetTransform, setActiveOnComplete, onComplete));
        }

        private IEnumerator Co_AnimateTransform(Transform target, bool setActiveOnComplete, Action onComplete)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            float duration = (Data != null && Data.swapDuration > 0) ? Data.swapDuration : 0.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                transform.position = Vector3.Lerp(startPos, target.position, t);
                transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);

                yield return null;
            }

            SnapToTransform(target);
            gameObject.SetActive(setActiveOnComplete);
            onComplete?.Invoke();
        }

        public void SmoothMoveTo(Vector3 targetWorldPos, Quaternion targetWorldRot, float speed)
        {
            transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * speed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetWorldRot, Time.deltaTime * speed);
        }

        public void SnapToTransform(Transform target)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
            transform.localScale = _originalScale;
        }
        #endregion
    }
}
