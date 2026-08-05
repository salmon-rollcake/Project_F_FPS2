using UnityEngine;

namespace MyFPS2
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Bullet : MonoBehaviour
    {
        [Header("Impact Effects")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private AudioClip hitSound;

        private float _damage;
        private bool _isExplosive;
        private float _explosionRadius;
        private float _gravity;
        private Rigidbody _rb;
        private bool _hasHit;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// 탄환 발사 시 무기에서 전달받아 초기화 (범위 폭발 옵션 추가)
        /// </summary>
        public void Initialize(Vector3 direction, float speed, float gravity, float damage, float lifeTime, bool isExplosive = false, float explosionRadius = 0f)
        {
            _damage = damage;
            _gravity = gravity;
            _isExplosive = isExplosive;
            _explosionRadius = explosionRadius;
            _hasHit = false;

            // 글로벌 Physics.gravity 조작 대신 개별 커스텀 중력 사용
            _rb.useGravity = false;

            // 초기 속도 적용 (총구 정면 방향)
            _rb.linearVelocity = direction.normalized * speed;

            // 일정 시간 후 자동 파괴
            Destroy(gameObject, lifeTime);
        }

        private void FixedUpdate()
        {
            if (!_hasHit && _gravity > 0f)
            {
                _rb.AddForce(Vector3.down * _gravity, ForceMode.Acceleration);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // 이미 충돌했거나 발사자/다른 탄환 레이어면 무시
            if (_hasHit) return;
            _hasHit = true;

            // 충돌 지점 및 법선 계산 (Concave MeshCollider 대응)
            Vector3 hitPoint = transform.position;
            Vector3 hitNormal = -transform.forward;

            // Raycast를 사용해 정확한 충돌지점(hitPoint)과 표면 법선(hitNormal)을 추출
            Ray ray = new Ray(transform.position - transform.forward * 0.5f, transform.forward);
            if (other.Raycast(ray, out RaycastHit hit, 1.0f))
            {
                hitPoint = hit.point;
                hitNormal = hit.normal;
            }
            // Convex MeshCollider 또는 일반 Collider일 경우에만 ClosestPoint 사용
            else if (other is BoxCollider || other is SphereCollider || other is CapsuleCollider || (other is MeshCollider meshCol && meshCol.convex))
            {
                hitPoint = other.ClosestPoint(transform.position);
            }

            // 1. 적(Enemy) / 피격 대상 데미지 처리
            if (_isExplosive)
            {
                // [범위 폭발 공격] hitPoint 기준 반경 내 모든 대상 타격
                Collider[] hitColliders = Physics.OverlapSphere(hitPoint, _explosionRadius);
                foreach (var col in hitColliders)
                {
                    ApplyDamageToTarget(col, hitPoint, hitNormal);
                }
            }
            else
            {
                // [단일 공격] 직접 충돌한 대상 타격
                ApplyDamageToTarget(other, hitPoint, hitNormal);
            }

            // 2. 적중 이펙트(VFX) 생성
            if (hitEffectPrefab != null)
            {
                Quaternion hitRotation = Quaternion.LookRotation(hitNormal);
                GameObject effect = Instantiate(hitEffectPrefab, hitPoint, hitRotation);
                Destroy(effect, 2f); // 이펙트 2초 후 파괴
            }

            // 3. 적중 사운드(SFX) 재생
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, hitPoint);
            }

            // 4. 탄환 제거
            Destroy(gameObject);
        }

        /// <summary>
        /// IDamageable 및 Health 대상 데미지 전달 공통 헬퍼 메서드
        /// </summary>
        private void ApplyDamageToTarget(Collider targetCollider, Vector3 point, Vector3 normal)
        {
            if (targetCollider.GetComponentInParent<IDamageable>() is { } damageable)
            {
                damageable.InflictDamage(_damage, point, normal);
            }
            else if (targetCollider.GetComponentInParent<Health>() is { } health)
            {
                health.TakeDamage(_damage);
            }
        }

        // 에디터 Gizmo로 폭발 범위 시각화
        private void OnDrawGizmosSelected()
        {
            if (_isExplosive)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, _explosionRadius);
            }
        }
    }
}
