using UnityEngine;

namespace MyFPS2
{
    
public interface IDamageable
{
    /// <summary>
    /// 대상에게 대미지를 가합니다.
    /// </summary>
    /// <param name="baseDamage">기본 대미지 양</param>
    /// <param name="hitPoint">타격 위치 (피격 이펙트/혈흔 발생용, 선택적)</param>
    /// <param name="hitNormal">타격 법선 벡터 (선택적)</param>
    void InflictDamage(float baseDamage, Vector3 hitPoint = default, Vector3 hitNormal = default);
}
}