using UnityEngine;

namespace MyFPS2
{

    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapons/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("General")]
        public string weaponName;
        public GameObject weaponPrefab;

        [Header("Stats")]
        [Tooltip("무기의 최대 사거리 (적 포착 레이캐스트 거리)")]
        public float range = 50f;

        [Header("Crosshair")]
        [Tooltip("이 무기에 연동할 크로스헤어 설정")]
        public CrosshairData crosshairData;

        [Header("Aim Down Sights (ADS)")]
        [Tooltip("기본 AimPosition에 추가 적용할 무기별 위치/회전 보정값")]
        public Vector3 aimPositionOffset = Vector3.zero;
        public Vector3 aimRotationOffset = Vector3.zero;

        [Tooltip("조준 시 적용할 카메라 FOV (기본 카메라 FOV보다 작은 값 설정)")]
        public float aimFOV = 40f;

        [Tooltip("조준 전환 속도")]
        public float aimSpeed = 12f;

        [Header("Switch Visuals")]
        public float swapDuration = 0.25f;
    }
}