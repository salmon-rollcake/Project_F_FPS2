using UnityEngine;

namespace MyFPS2
{

    [DisallowMultipleComponent]
    public class Damageable : MonoBehaviour, IDamageable
    {
        [Header("Damage Settings")]
        [Tooltip("기본 대미지에 곱해질 부위별 대미지 배율 (예: 머리=2.0, 몸통=1.0, 다리=0.8)")]
        [SerializeField] private float damageMultiplier = 1.0f;

        [Header("Health Target")]
        [Tooltip("비워둘 경우 상위 부모(InParent)에서 Health 컴포넌트를 자동으로 탐색합니다.")]
        [SerializeField] private Health targetHealth;

        public float DamageMultiplier => damageMultiplier;

        private void Awake()
        {
            // 1. Target Health가 할당되어 있지 않다면, 부모 오브젝트 탐색 (캐싱)
            if (targetHealth == null)
            {
                targetHealth = GetComponentInParent<Health>();

                if (targetHealth == null)
                {
                    Debug.LogWarning($"[Damageable] {gameObject.name}의 상위 오브젝트에서 Health 컴포넌트를 찾을 수 없습니다.", this);
                }
            }
        }

        /// <summary>
        /// 외부(총알, 레이캐스트, 투사체 등)에서 대미지를 입힐 때 호출하는 메서드
        /// </summary>
        public void InflictDamage(float baseDamage, Vector3 hitPoint = default, Vector3 hitNormal = default)
        {
            // 유효성 및 사망 상태 검사
            if (targetHealth == null || targetHealth.IsDead) return;

            // 1. 충돌체(부위)에 따른 대미지 계산
            float calculatedDamage = CalculateMultiplierDamage(baseDamage);

            // 2. 0 초과 대미지일 경우 타겟 Health 컴포넌트에 TakeDamage 호출
            if (calculatedDamage > 0f)
            {
                targetHealth.TakeDamage(calculatedDamage);
            }
        }

        /// <summary>
        /// 부위별 배율 대미지 계산 메서드
        /// </summary>
        private float CalculateMultiplierDamage(float rawDamage)
        {
            return Mathf.Max(rawDamage * damageMultiplier, 0f);
        }
    }
}