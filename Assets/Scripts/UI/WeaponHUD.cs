using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MyFPS2
{
    public class WeaponHUD : MonoBehaviour
    {
        [Header("Weapon Index UI List")]
        [Tooltip("Starting Weapons 순서대로 연결할 인덱스 표시 TMP (Element 0, 1, 2...)")]
        [SerializeField] private List<TMP_Text> weaponIndexTexts;

        [Header("Ammo Gauge & Count UI")]
        [SerializeField] private Image ammoFillImage;          // Fill Amount 컨트롤용 Image
        [SerializeField] private TMP_Text ammoCountText;       // 남은 탄약 / 최대 탄약 표시 TMP

        [Header("Reload Indicator")]
        [SerializeField] private GameObject reloadIndicator;   // 탄약 100%가 아닐 때 켜지는 오브젝트

        /// <summary>
        /// Starting Weapons 개수를 받아 HUD 초기 세팅
        /// </summary>
        public void InitializeHUD(int startingWeaponCount)
        {
            // 1 & 2. Starting Weapons 개수에 맞춰 인덱스 텍스트 표시
            for (int i = 0; i < weaponIndexTexts.Count; i++)
            {
                if (weaponIndexTexts[i] != null)
                {
                    // 설정된 개수 이내의 무기 슬롯만 활성화하고 Element 번호 표시
                    bool isActive = i < startingWeaponCount;
                    weaponIndexTexts[i].gameObject.SetActive(isActive);

                    if (isActive)
                    {
                        weaponIndexTexts[i].text = (i + 1).ToString(); // UI에는 1, 2, 3으로 표시
                    }
                }
            }

            // 시작 시 재장전 오버레이 비활성화
            if (reloadIndicator != null)
            {
                reloadIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// 현재 무기의 탄약 및 재장전 오브젝트 상태 업데이트
        /// </summary>
        public void UpdateAmmoUI(int currentAmmo, int maxAmmo)
        {
            if (maxAmmo <= 0) return;

            // 3. Fill Image의 Fill Amount (0.0f ~ 1.0f)
            float fillRatio = (float)currentAmmo / maxAmmo;
            if (ammoFillImage != null)
            {
                ammoFillImage.fillAmount = fillRatio;
            }

            // 4. 남은 탄약 숫자로 표시
            if (ammoCountText != null)
            {
                ammoCountText.text = $"{currentAmmo} / {maxAmmo}";
            }

            // 5. 남은 탄약이 100%가 아닐 경우 재장전 GameObject 활성화
            if (reloadIndicator != null)
            {
                bool isFull = currentAmmo >= maxAmmo;
                reloadIndicator.SetActive(!isFull);
            }
        }

        /// <summary>
        /// 무기 교체 시 현재 선택된 무기 인덱스 하이라이트 (선택 사항)
        /// </summary>
        public void SelectWeaponSlot(int selectedIndex)
        {
            for (int i = 0; i < weaponIndexTexts.Count; i++)
            {
                if (weaponIndexTexts[i] != null)
                {
                    // 현재 선택된 무기의 텍스트 색상을 강조하거나 Alpha 변경
                    weaponIndexTexts[i].color = (i == selectedIndex) ? Color.yellow : Color.white;
                }
            }
        }
    }
}
