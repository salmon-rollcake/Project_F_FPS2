using UnityEngine;
using System;
using System.Collections;

namespace MyFPS2
{
    public class WeaponController : MonoBehaviour
    {
        public WeaponData Data { get; private set; }

        private Vector3 _originalScale = Vector3.one;
        private float _lastFireTime;
        private float _currentChargeTimer;
        private bool _isCharging;

        public event Action<float> OnChargeProgressChanged;

        private Renderer[] _weaponRenderers;
        private Coroutine _moveCoroutine;

        // 반동 스크립트 참조 추가
        [SerializeField] private WeaponRecoil weaponRecoil;

        [Header("Muzzle Reference")]
        [SerializeField] private Transform muzzleTransform; // 총구 Transform

        private void Awake()
        {
            _weaponRenderers = GetComponentsInChildren<Renderer>();
            _originalScale = transform.localScale;

            // 부모 오브젝트(Player/WeaponHolder)에서 WeaponRecoil 컴포넌트 자동 검색
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

            // 동적 생성 후 부모 설정 시점에 다시 한번 참조 확인
            if (weaponRecoil == null)
            {
                weaponRecoil = GetComponentInParent<WeaponRecoil>();
            }
        }

        public void HandleFiring(PlayerInputHandler input)
        {
            if (Data == null) return;

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

        private void ExecuteShoot()
        {
            _lastFireTime = Time.time;

            if (Data != null && Data.bulletPrefab != null && muzzleTransform != null)
            {
                // bulletsPerShot 수만큼 반복 생성 (일반 총은 1회, 샷건은 지정된 수만큼)
                int count = Mathf.Max(1, Data.bulletsPerShot);

                for (int i = 0; i < count; i++)
                {
                    // 산탄 퍼짐 각도 계산 (spreadAngle 적용)
                    Vector3 fireDirection = muzzleTransform.forward;

                    if (Data.spreadAngle > 0f)
                    {
                        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * Mathf.Tan(Data.spreadAngle * 0.5f * Mathf.Deg2Rad);
                        fireDirection += muzzleTransform.right * randomCircle.x + muzzleTransform.up * randomCircle.y;
                        fireDirection.Normalize();
                    }

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

            Debug.Log($"[{Data.weaponName}] {Data.fireType} 사격! (발사체 수: {Data.bulletsPerShot})");
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
