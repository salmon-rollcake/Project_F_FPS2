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

        [Header("Switch Visuals")]
        public float swapDuration = 0.25f;
    }
}