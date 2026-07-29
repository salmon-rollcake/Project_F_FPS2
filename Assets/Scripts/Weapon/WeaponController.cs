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
        /// 무기 스왑 시 사용되는 코루틴 기반 강제 이동 애니메이션
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
                transform.localScale = Vector3.Lerp(startScale, target.lossyScale, t);

                yield return null;
            }

            SnapToTransform(target);
            gameObject.SetActive(setActiveOnComplete);
            onComplete?.Invoke();
        }

        /// <summary>
        /// 목표 위치/회전값으로 실시간 부드럽게 이동 (조준 / 조준 해제용)
        /// </summary>
        public void SmoothMoveTo(Vector3 targetWorldPos, Quaternion targetWorldRot, float speed)
        {
            transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * speed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetWorldRot, Time.deltaTime * speed);
        }

        public void SnapToTransform(Transform target)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;

            Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            transform.localScale = new Vector3(
                target.lossyScale.x / parentScale.x,
                target.lossyScale.y / parentScale.y,
                target.lossyScale.z / parentScale.z
            );
        }
    }
}