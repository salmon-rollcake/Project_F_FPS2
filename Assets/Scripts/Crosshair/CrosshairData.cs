using UnityEngine;

namespace MyFPS2
{

    [CreateAssetMenu(fileName = "NewCrosshairData", menuName = "Weapons/Crosshair Data")]
    public class CrosshairData : ScriptableObject
    {
        [Header("Visuals")]
        public Sprite crosshairSprite;

        [Header("Size Settings")]
        public Vector2 defaultSize = new Vector2(32f, 32f);
        public Vector2 targetAcquiredSize = new Vector2(48f, 48f);

        [Header("Color Settings")]
        public Color defaultColor = Color.white;
        public Color targetAcquiredColor = Color.red;

        [Header("Transition")]
        [Tooltip("크기 및 색상 변경 시 보간 속도")]
        public float transitionSpeed = 15f;
    }
}