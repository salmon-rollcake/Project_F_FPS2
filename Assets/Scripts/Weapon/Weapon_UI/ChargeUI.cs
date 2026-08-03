using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace MyFPS2
{
    public class ChargeUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject container;

        [Tooltip("TextMeshPro를 사용하는 경우 연결")]
        [SerializeField] private TMP_Text chargeTextTMP;

        [Tooltip("기본 Legacy Text를 사용하는 경우 연결")]
        [SerializeField] private Text chargeTextLegacy;

        [Header("Settings")]
        [Tooltip("100% 완료 및 발사 후 UI가 사라질 때까지의 딜레이 시간 (초)")]
        [SerializeField] private float hideDelayAfterFull = 0.3f;

        private Coroutine _hideCoroutine;

        private void Awake()
        {
            if (container != null)
                container.SetActive(false);
        }

        /// <summary>
        /// PlayerWeaponManager 및 WeaponController에서 호출하는 충전율 업데이트 (0.0f ~ 1.0f)
        /// </summary>
        public void UpdateChargeProgress(float progressRatio)
        {
            if (container == null) return;

            // 충전이 진행 중이거나 막 시작된 경우
            if (progressRatio > 0f)
            {
                // 진행 중일 때는 하이드 코루틴 중단
                if (_hideCoroutine != null)
                {
                    StopCoroutine(_hideCoroutine);
                    _hideCoroutine = null;
                }

                if (!container.activeSelf)
                    container.SetActive(true);

                int percent = Mathf.FloorToInt(progressRatio * 100f);
                SetText($"{percent}%");

                // 100% 도달 시 약간의 딜레이 후 UI 숨김
                if (progressRatio >= 1.0f && _hideCoroutine == null)
                {
                    _hideCoroutine = StartCoroutine(Co_HideAfterDelay());
                }
            }
            else
            {
                // progressRatio가 0인 경우 (충전 취소/초기화 시)
                if (_hideCoroutine != null)
                {
                    StopCoroutine(_hideCoroutine);
                    _hideCoroutine = null;
                }
                container.SetActive(false);
            }
        }

        private void SetText(string text)
        {
            if (chargeTextTMP != null)
            {
                chargeTextTMP.text = text;
            }
            else if (chargeTextLegacy != null)
            {
                chargeTextLegacy.text = text;
            }
        }

        private IEnumerator Co_HideAfterDelay()
        {
            yield return new WaitForSeconds(hideDelayAfterFull);
            if (container != null)
            {
                container.SetActive(false);
            }
            _hideCoroutine = null;
        }
    }
}
