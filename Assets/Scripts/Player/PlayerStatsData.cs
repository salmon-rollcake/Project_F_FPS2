using UnityEngine;

namespace MyFPS2
{

    [CreateAssetMenu(fileName = "NewPlayerStats", menuName = "Player/Player Stats Data")]
    public class PlayerStatsData : ScriptableObject
    {
        [Header("Movement")]
        public float walkSpeed = 5f;
        public float sprintSpeed = 8f;
        public float gravity = -19.62f; // 기본 유니티 중력(-9.81)보다 약간 무겁게 설정하여 쾌적한 FPS 느낌 제공
        public float jumpHeight = 1.2f;

        [Header("Look / Camera")]
        public float mouseSensitivity = 2f;
        public float minVerticalAngle = -89f;
        public float maxVerticalAngle = 89f;
    }
}