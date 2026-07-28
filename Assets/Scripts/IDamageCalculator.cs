using UnityEngine;

namespace MyFPS2
{
// 대미지 계산 추상화 인터페이스
public interface IDamageCalculator
{
    float CalculateDamage(float rawDamage, GameObject target);
}

// [기본 구현 예시] 방어력(Stat/Component)을 반영할 수 있는 계산기
public class DefaultDamageCalculator : IDamageCalculator
{
    private readonly float _armor;

    public DefaultDamageCalculator(float armor = 0f)
    {
        _armor = armor;
    }

    public float CalculateDamage(float rawDamage, GameObject target)
    {
        // 방어력에 따른 감소 로직 예시 (필요시 방어력 차감 방식 변경 가능)
        float finalDamage = rawDamage - _armor;
        return Mathf.Max(finalDamage, 0f);
    }
}
}