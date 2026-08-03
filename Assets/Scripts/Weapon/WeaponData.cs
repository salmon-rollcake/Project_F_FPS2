using UnityEngine;

namespace MyFPS2
{

    public enum WeaponFireType
    {
        Manual, // 단발
        Auto,   // 연사
        Charge, // 충전 사격
        Snipe   // 저격 (특수 조준 모드)
    }

    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapons/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("General")]
        public string weaponName;
        public GameObject weaponPrefab;
        public WeaponFireType fireType = WeaponFireType.Manual;

        [Header("Bullet & Firing Settings")]
        public GameObject bulletPrefab;
        public float range = 50f;
        public float damage = 20f;
        [Tooltip("연사 간격 (초 단위) - Auto/Manual 재사격 대기시간")]
        public float fireRate = 0.2f;
        [Tooltip("충전 완료까지 걸리는 시간 (초 단위) - Charge 전용")]
        public float chargeTime = 1.5f;
        [Tooltip("탄환 날아가는 속도")]
        public float bulletSpeed = 100f;
        [Tooltip("탄환에 적용될 중력 수치 (0이면 직진, 9.81이면 일반 중력)")]
        public float bulletGravity = 0f;
        [Tooltip("탄환 소멸까지의 시간(초)")]
        public float bulletLifeTime = 5f;
        [Tooltip("한 번 사격 시 발사될 탄환 개수 (일반 무기는 1)")]
        public int bulletsPerShot = 1;
        [Tooltip("탄환 탄착군 퍼짐 정도 (각도)")]
        public float spreadAngle = 5f;

        [Header("Explosive / Area Damage Settings")]
        [Tooltip("범위 공격(스플래시) 여부")]
        public bool isExplosive = false;
        [Tooltip("폭발/범위 데미지 반경")]
        public float explosionRadius = 3f;

        [Header("Muzzle Visuals & Sound")]
        public GameObject muzzleFlashPrefab;
        public AudioClip fireSound;

        [Header("Snipe Settings (Snipe 전용)")]
        [Tooltip("저격 모드 진입 시 스코프 줌 FOV (매우 작은 값일수록 고배율 줌)")]
        public float snipeFOV = 15f;
        [Tooltip("저격 모드 전용 스코프 UI 스프라이트")]
        public Sprite scopeOverlaySprite;

        [Header("Crosshair")]
        public CrosshairData crosshairData;

        [Header("Aim Down Sights (ADS)")]
        public Vector3 aimPositionOffset = Vector3.zero;
        public Vector3 aimRotationOffset = Vector3.zero;
        public float aimFOV = 40f;
        public float aimSpeed = 12f;

        [Header("Weapon Sway & Bobbing")]
        public float swayAmount = 0.02f;
        public float maxSwayAmount = 0.05f;
        public float swayRotationAmount = 2f;
        public float swaySmoothness = 8f;
        public float walkBobFrequency = 10f;
        public float walkBobAmount = 0.015f;
        public float sprintBobFrequency = 14f;
        public float sprintBobAmount = 0.03f;

        [Header("Weapon Recoil Settings")]
        [Tooltip("1회 발사 시 뒤로 밀리는 힘")]
        public float kickBackAmount = 0.08f;
        [Tooltip("1회 발사 시 위로 밀리는 힘")]
        public float kickUpAmount = 0.03f;
        [Tooltip("1회 발사 시 총구가 위로 들리는 회전 각도 (X축)")]
        public float kickRotationX = 4f;
        [Tooltip("1회 발사 시 좌우로 흔들리는 랜덤 회전 각도 (Y/Z축)")]
        public float kickRotationRandomY = 1.5f;

        [Header("Recoil Constraints & Recovery")]
        [Tooltip("최대 위치 오프셋 (Z축 뒤로 밀리는 최대 한도)")]
        public float maxPositionOffset = 0.2f;
        [Tooltip("최대 회전 오프셋 (X축 위로 들리는 최대 각도)")]
        public float maxRotationOffset = 15f;
        [Tooltip("반동이 가해지는 속도")]
        public float recoilSpeed = 25f;
        [Tooltip("원래 위치로 복귀하는 속도")]
        public float returnSpeed = 10f;

        [Header("Switch Visuals")]
        public float swapDuration = 0.25f;
    }
}