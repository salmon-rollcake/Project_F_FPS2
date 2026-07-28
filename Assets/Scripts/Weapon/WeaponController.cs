using UnityEngine;
using System.Collections;

namespace MyFPS2
{

    public class WeaponController : MonoBehaviour
    {
        public WeaponData Data { get; private set; }
        private Coroutine _moveCoroutine;

        public void Initialize(WeaponData data)
        {
            Data = data;
        }

        /// <summary>
        /// 지정된 Target Transform 위치/회전/스케일로 부드럽게 이동
        /// </summary>
        public void AnimateToTransform(Transform targetTransform, bool setActiveOnComplete, System.Action onComplete = null)
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            gameObject.SetActive(true);
            _moveCoroutine = StartCoroutine(Co_AnimateTransform(targetTransform, setActiveOnComplete, onComplete));
        }

        private IEnumerator Co_AnimateTransform(Transform target, bool setActiveOnComplete, System.Action onComplete)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            Vector3 startScale = transform.localScale;

            float duration = (Data != null && Data.swapDuration > 0) ? Data.swapDuration : 0.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                transform.position = Vector3.Lerp(startPos, target.position, t);
                transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);
                transform.localScale = Vector3.Lerp(startScale, target.lossyScale, t); // 스케일 동기화

                yield return null;
            }

            // 최종 위치/회전/스케일 정착
            SnapToTransform(target);

            gameObject.SetActive(setActiveOnComplete);
            onComplete?.Invoke();
        }

        /// <summary>
        /// 목표 Transform으로 위치, 회전, 스케일을 즉시 맞춤
        /// </summary>
        public void SnapToTransform(Transform target)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;

            // 부모 소켓의 스케일에 영향받지 않도록 월드 스케일 기준 대입
            Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            transform.localScale = new Vector3(
                target.lossyScale.x / parentScale.x,
                target.lossyScale.y / parentScale.y,
                target.lossyScale.z / parentScale.z
            );
        }
    }
}