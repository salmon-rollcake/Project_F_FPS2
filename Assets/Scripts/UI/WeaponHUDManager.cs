using System.Collections.Generic;
using UnityEngine;

namespace MyFPS2
{
    public class WeaponHUDManager : MonoBehaviour
    {
        [Header("UI Slot References")]
        [Tooltip("HUD 프리팹 내부에 이미 만들어져 있는 개별 슬롯 컴포넌트들")]
        [SerializeField] private List<WeaponHUDItem> hudItems;

        /// <summary>
        /// 보유 무기 개수에 맞게 슬롯 활성화/비활성화
        /// </summary>
        public void InitializeHUD(int startingWeaponCount)
        {
            for (int i = 0; i < hudItems.Count; i++)
            {
                if (hudItems[i] == null) continue;

                bool isActive = i < startingWeaponCount;
                hudItems[i].gameObject.SetActive(isActive);

                if (isActive)
                {
                    hudItems[i].Setup(i + 1);
                }
            }
        }

        /// <summary>
        /// 무기 이벤트(재장전 진행/완료)를 HUD 슬롯과 바인딩
        /// </summary>
        public void RegisterWeaponEvents(List<WeaponController> weapons)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                if (i >= hudItems.Count || weapons[i] == null) continue;

                int slotIndex = i;
                WeaponController weapon = weapons[i];

                // 재장전 진행률 이벤트 연결
                weapon.OnReloadProgressChanged += (progress) =>
                {
                    if (slotIndex < hudItems.Count && hudItems[slotIndex] != null)
                    {
                        hudItems[slotIndex].UpdateReloadProgress(progress);
                    }
                };

                // 재장전 완료 점멸 이벤트 연결
                weapon.OnReloadCompleted += () =>
                {
                    if (slotIndex < hudItems.Count && hudItems[slotIndex] != null)
                    {
                        hudItems[slotIndex].TriggerReloadFlash();
                    }
                };
            }
        }

        public void UpdateHUD(List<WeaponController> weapons, int currentWeaponIndex)
        {
            for (int i = 0; i < hudItems.Count; i++)
            {
                if (i < weapons.Count && weapons[i] != null && weapons[i].Data != null)
                {
                    WeaponController weapon = weapons[i];
                    hudItems[i].UpdateState(weapon.CurrentAmmo, weapon.Data.maxAmmo, i == currentWeaponIndex, weapon.IsReloading);
                }
            }
        }
    }
}
