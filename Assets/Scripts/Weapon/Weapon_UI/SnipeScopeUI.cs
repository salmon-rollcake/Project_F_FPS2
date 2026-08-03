using UnityEngine;
using UnityEngine.UI;

namespace MyFPS2
{
    public class SnipeScopeUI : MonoBehaviour
    {
        [SerializeField] private Image scopeOverlayImage;

        private void Awake()
        {
            if (scopeOverlayImage != null)
                scopeOverlayImage.gameObject.SetActive(false);
        }

        public void ShowScope(Sprite scopeSprite)
        {
            if (scopeOverlayImage == null) return;

            scopeOverlayImage.sprite = scopeSprite;
            scopeOverlayImage.gameObject.SetActive(true);
        }

        public void HideScope()
        {
            if (scopeOverlayImage == null) return;
            scopeOverlayImage.gameObject.SetActive(false);
        }
    }
}