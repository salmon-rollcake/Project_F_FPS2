using UnityEngine;
using System;

namespace MyFPS2
{
    public class Health : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool isInvulnerable = false;

        // 현재 체력 및 사망 상태
        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public bool IsDead { get; private set; }
        public bool IsInvulnerable
        {
            get => isInvulnerable;
            set => isInvulnerable = value;
        }

        // 데미지 계산 전략 (DIP: 외부에서 주입 가능, 미지정 시 기본 계산기 사용)
        private IDamageCalculator _damageCalculator;

        #region Events (System.Action)
        // float: 현재 체력, float: 최대 체력
        public event Action<float, float> OnHealthChanged;
        // float: 실제 입은 대미지
        public event Action<float> OnDamaged;
        // float: 실제 회복된 양
        public event Action<float> OnHealed;
        // 사망 시 발동
        public event Action OnDeath;
        #endregion

        private void Awake()
        {
            CurrentHealth = maxHealth;
            IsDead = false;

            // 기본 데미지 계산기 설정 (추후 외부에서 SetDamageCalculator로 변경 가능)
            _damageCalculator = new DefaultDamageCalculator(armor: 0f);
        }

        /// <summary>
        /// 외부에서 데미지 계산 방식을 주입(의존성 주입)받을 수 있는 메서드 (DIP)
        /// </summary>
        public void SetDamageCalculator(IDamageCalculator calculator)
        {
            _damageCalculator = calculator ?? new DefaultDamageCalculator();
        }

        /// <summary>
        /// 대미지 처리
        /// </summary>
        public void TakeDamage(float amount)
        {
            // 1. 이미 죽었거나 무적 상태이면 대미지 무시
            if (IsDead || isInvulnerable) return;

            // 2. 추가 변수(방어력 등)를 고려한 대미지 계산
            float actualDamage = _damageCalculator.CalculateDamage(amount, gameObject);

            // 3. 실제 입은 대미지가 0 초과일 때만 체력 차감 및 이벤트 발생
            if (actualDamage > 0f)
            {
                CurrentHealth = Mathf.Max(CurrentHealth - actualDamage, 0f);

                OnDamaged?.Invoke(actualDamage);
                OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

                // 4. 체력이 0 이하가 되면 사망 처리
                if (CurrentHealth <= 0f)
                {
                    HandleDeath();
                }
            }
        }

        /// <summary>
        /// 체력 회복
        /// </summary>
        public void Heal(float amount)
        {
            // 이미 죽었거나 유효하지 않은 값이면 무시
            if (IsDead || amount <= 0f) return;

            // 1. 입력받은 회복량과 현재 체력을 고려하여 실제 회복량 산출
            float previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
            float actualHealAmount = CurrentHealth - previousHealth;

            // 2. 실제 회복량이 0 초과일 때만 힐 구현 및 이벤트 발생
            if (actualHealAmount > 0f)
            {
                OnHealed?.Invoke(actualHealAmount);
                OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            }
        }

        /// <summary>
        /// 사망 처리
        /// </summary>
        private void HandleDeath()
        {
            // 두 번 죽는 것 방지 (Guard Clause)
            if (IsDead) return;

            // 남은 체력이 0 이하일 때 실행 조건 검증
            if (CurrentHealth <= 0f)
            {
                IsDead = true;
                OnDeath?.Invoke();
            }
        }
    }
}