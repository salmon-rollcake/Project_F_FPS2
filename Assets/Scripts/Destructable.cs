using UnityEngine;
using System.Collections;

namespace MyFPS2
{
    public enum DestructionMode
{
    Destroy,        // Instantiate 기반 생성 오브젝트 (Destroy)
    Disable,        // 오브젝트 풀링(Object Pool) 사용 오브젝트 (SetActive(false))
    None            // 파괴/비활성화 없이 이펙트/사망 이벤트만 전달할 때
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public class Destructable : MonoBehaviour
{
    [Header("Destruction Settings")]
    [Tooltip("사망 시 처리 방식 (Destroy / Disable)")]
    [SerializeField] private DestructionMode mode = DestructionMode.Destroy;

    [Tooltip("사망 이벤트 발생 후 실제 파괴/비활성화까지의 지연 시간 (초)")]
    [SerializeField] private float destroyDelay = 0.0f;

    [Header("Death Visual/Audio FX (Optional)")]
    [Tooltip("사망 시 생성할 피격/폭발 파티클 이펙트")]
    [SerializeField] private GameObject deathEffectPrefab;

    [Tooltip("사망 이펙트가 남아있을 시간")]
    [SerializeField] private float effectLifeTime = 3.0f;

    private Health _health;
    private Coroutine _destructionCoroutine;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        // 1. Health의 OnDeath 이벤트 구독 (System.Action)
        if (_health != null)
        {
            _health.OnDeath += HandleObjectDestruction;
        }
    }

    private void OnDisable()
    {
        // 2. 메모리 누수 방지 및 중복 호출 방지를 위한 이벤트 구독 해제
        if (_health != null)
        {
            _health.OnDeath -= HandleObjectDestruction;
        }

        // 코루틴 실행 중 비활성화될 때 정지
        if (_destructionCoroutine != null)
        {
            StopCoroutine(_destructionCoroutine);
            _destructionCoroutine = null;
        }
    }

    /// <summary>
    /// Health.OnDeath() 이벤트가 발행되었을 때 파괴/처리 수행
    /// </summary>
    private void HandleObjectDestruction()
    {
        // 1. 사망 연출 이펙트 스폰
        SpawnDeathEffect();

        // 2. 지연 시간이 있다면 코루틴으로 처리, 없으면 즉시 처리
        if (destroyDelay > 0f)
        {
            if (_destructionCoroutine != null) StopCoroutine(_destructionCoroutine);
            _destructionCoroutine = StartCoroutine(ProcessDestructionRoutine());
        }
        else
        {
            ExecuteDestruction();
        }
    }

    private IEnumerator ProcessDestructionRoutine()
    {
        yield return new WaitForSeconds(destroyDelay);
        ExecuteDestruction();
    }

    /// <summary>
    /// 실제 오브젝트 파괴 또는 비활성화 동작
    /// </summary>
    private void ExecuteDestruction()
    {
        switch (mode)
        {
            case DestructionMode.Destroy:
                Destroy(gameObject);
                break;

            case DestructionMode.Disable:
                // 풀링 시스템을 위해 초기화 상태로 되돌리고 비활성화
                gameObject.SetActive(false);
                break;

            case DestructionMode.None:
                // 아무 작업도 하지 않음 (외부 Ragdoll 시스템이나 시체 유지)
                break;
        }
    }

    /// <summary>
    /// 사망 파티클/사운드 이펙트 생성
    /// </summary>
    private void SpawnDeathEffect()
    {
        if (deathEffectPrefab != null)
        {
            GameObject effectInstance = Instantiate(deathEffectPrefab, transform.position, transform.rotation);
            if (effectLifeTime > 0f)
            {
                Destroy(effectInstance, effectLifeTime);
            }
        }
    }
}
}
