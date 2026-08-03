using UnityEngine;

namespace MyFPS2
{
    public class WeaponRecoil : MonoBehaviour
    {
        private Vector3 _currentRecoilPos;
        private Vector3 _targetRecoilPos;

        private Vector3 _currentRecoilRot;
        private Vector3 _targetRecoilRot;

        /// <summary>
        /// 사격(Fire) 순간 호출되는 메인 반동 메서드
        /// 진행 중인 연출이 있더라도 즉시 초기화 후 새로운 반동 적용
        /// </summary>
        public void TriggerRecoil(WeaponData data)
        {
            if (data == null) return;

            // 1. 새로운 반동 입력 시 이전 진행 중이던 반동 오프셋을 즉시 리셋 (애니메이션 재시작 효과)
            _currentRecoilPos = Vector3.zero;
            _currentRecoilRot = Vector3.zero;

            // 2. 뒤쪽(-Z) + 위쪽(+Y) 방향 위치 오프셋 설정
            Vector3 targetPos = new Vector3(
                Random.Range(-data.kickUpAmount * 0.3f, data.kickUpAmount * 0.3f), // 좌우 미세 오차
                data.kickUpAmount,                                                  // 위쪽
                -data.kickBackAmount                                                // 뒤쪽
            );

            // 3. 위쪽(+X 회전) + 좌우 랜덤 회전 오프셋 설정
            Vector3 targetRot = new Vector3(
                -data.kickRotationX,                                                // 총구 들림
                Random.Range(-data.kickRotationRandomY, data.kickRotationRandomY), // 좌우 무작위
                Random.Range(-data.kickRotationRandomY, data.kickRotationRandomY)  // Roll 무작위
            );

            // 4. 최대 제한(Clamp) 적용 - 무한히 밀려나는 것을 방지
            _targetRecoilPos = new Vector3(
                Mathf.Clamp(targetPos.x, -data.maxPositionOffset, data.maxPositionOffset),
                Mathf.Clamp(targetPos.y, -data.maxPositionOffset, data.maxPositionOffset),
                Mathf.Clamp(targetPos.z, -data.maxPositionOffset, data.maxPositionOffset)
            );

            _targetRecoilRot = new Vector3(
                Mathf.Clamp(targetRot.x, -data.maxRotationOffset, data.maxRotationOffset),
                Mathf.Clamp(targetRot.y, -data.maxRotationOffset, data.maxRotationOffset),
                Mathf.Clamp(targetRot.z, -data.maxRotationOffset, data.maxRotationOffset)
            );
        }

        /// <summary>
        /// 매 프레임 반동 적용 및 복귀(Recovery) 오프셋을 계산
        /// </summary>
        public void CalculateRecoil(WeaponData data, out Vector3 recoilPos, out Quaternion recoilRot)
        {
            if (data == null)
            {
                recoilPos = Vector3.zero;
                recoilRot = Quaternion.identity;
                return;
            }

            // 목표 반동 지점을 향해 복귀(0,0,0으로 Lerp)
            _targetRecoilPos = Vector3.Lerp(_targetRecoilPos, Vector3.zero, Time.deltaTime * data.returnSpeed);
            _targetRecoilRot = Vector3.Lerp(_targetRecoilRot, Vector3.zero, Time.deltaTime * data.returnSpeed);

            // 현재 반동 오프셋을 목표 지점으로 빠른 보간
            _currentRecoilPos = Vector3.Slerp(_currentRecoilPos, _targetRecoilPos, Time.deltaTime * data.recoilSpeed);
            _currentRecoilRot = Vector3.Slerp(_currentRecoilRot, _targetRecoilRot, Time.deltaTime * data.recoilSpeed);

            recoilPos = _currentRecoilPos;
            recoilRot = Quaternion.Euler(_currentRecoilRot);
        }
    }
}
