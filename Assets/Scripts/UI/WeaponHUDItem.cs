using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace MyFPS2
{
    public class WeaponHUDItem : MonoBehaviour
    {
        [Header("Slot Components")]
        [SerializeField] private TMP_Text weaponIndexText;   // 슬롯 번호
        [SerializeField] private Image ammoFillImage;        // Fill Amount 게이지
        [SerializeField] private TMP_Text ammoCountText;     // CurrentAmmo 단독 표시
        [SerializeField] private GameObject reloadIndicator; // 재장전 표시
        [SerializeField] private Image backgroundImage;      // 배경 이미지

        [Header("Visual Styles")]
        [SerializeField] private CanvasGroup canvasGroup; // 알파(투명도) 제어용 CanvasGroup

        private Color _originalBgColor = Color.white;
        private Color _originalFillColor = Color.white;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            if (backgroundImage != null) _originalBgColor = backgroundImage.color;
            if (ammoFillImage != null) _originalFillColor = ammoFillImage.color;
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void Setup(int slotIndex)
        {
            if (weaponIndexText != null)
                weaponIndexText.text = slotIndex.ToString();
        }

        /// <summary>
        /// 일반 사격 상태 UI 업데이트
        /// </summary>
        public void UpdateState(int currentAmmo, int maxAmmo, bool isSelected, bool isReloading)
        {
            // 재장전 중이 아닐 때만 일반 Fill 게이지 적용
            if (!isReloading && ammoFillImage != null)
            {
                ammoFillImage.fillAmount = (maxAmmo > 0) ? (float)currentAmmo / maxAmmo : 0f;
            }

            if (ammoCountText != null) ammoCountText.text = currentAmmo.ToString();
            if (reloadIndicator != null) reloadIndicator.SetActive(currentAmmo < maxAmmo);

            // [요구사항 4] 남은 탄약이 0일 때 배경색을 붉은색(Alpha 255)으로 표시
            if (backgroundImage != null)
            {
                if (currentAmmo <= 0)
                {
                    backgroundImage.color = new Color(1f, 0.2f, 0.2f, 1f); // 붉은색, 불투명(255)
                }
                else
                {
                    backgroundImage.color = _originalBgColor;
                }
            }

            // [요구사항 5] 활성화/비활성화 무기 스케일 및 투명도 차등 적용
            float targetScale = isSelected ? 1.05f : 0.9f;
            float targetAlpha = isSelected ? 1.0f : 0.4f;

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * targetScale, Time.deltaTime * 10f);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * 10f);
            }
        }

        /// <summary>
        /// [요구사항 2] 재장전 진행 중 Fill Amount 업데이트
        /// </summary>
        public void UpdateReloadProgress(float progress)
        {
            if (ammoFillImage != null)
            {
                ammoFillImage.fillAmount = progress;
            }
        }

        /// <summary>
        /// [요구사항 3] 장전 완료 시 Fill Image 검은색 점멸
        /// </summary>
        public void TriggerReloadFlash()
        {
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(Co_FlashBlack());
        }

        private IEnumerator Co_FlashBlack()
        {
            if (ammoFillImage == null) yield break;

            // 검은색으로 변경 후 원래 색상으로 복귀 (2회 반복)
            for (int i = 0; i < 2; i++)
            {
                ammoFillImage.color = Color.black;
                yield return new WaitForSeconds(0.08f);
                ammoFillImage.color = _originalFillColor;
                yield return new WaitForSeconds(0.08f);
            }
        }
    }
}
