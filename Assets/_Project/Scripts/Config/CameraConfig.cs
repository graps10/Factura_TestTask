using DG.Tweening;
using UnityEngine;

namespace TurretRush.Config
{
    [CreateAssetMenu(menuName = "TurretRush/Camera Config", fileName = "CameraConfig")]
    public sealed class CameraConfig : ScriptableObject
    {
        [Header("Gameplay pose")]
        [Tooltip("World-space offset from the car. Not car-local: the camera must not " +
                 "inherit the car's yaw or the whole screen swings as it drifts.")]
        [SerializeField] private Vector3 offset = new(0f, 9f, -11f);

        [SerializeField] private Vector3 eulerAngles = new(28f, 0f, 0f);

        [Header("Follow")]
        [SerializeField, Min(0.01f)] private float smoothTime = 0.15f;

        [Tooltip("0 keeps the camera locked to the centre line, 1 glues it to the car. " +
                 "Something in between lets the player feel the car crossing the road.")]
        [SerializeField, Range(0f, 1f)] private float lateralFollow = 0.6f;

        [Header("Intro pose")]
        [Tooltip("Where the camera sits before the level starts - low and close " +
                 "behind the car, showing it off rather than the road ahead.")]
        [SerializeField] private Vector3 introOffset = new(0f, 3.2f, -7.5f);

        [SerializeField] private Vector3 introEulerAngles = new(10f, 0f, 0f);

        [SerializeField, Min(0.05f)] private float introDuration = 0.9f;

        [SerializeField] private Ease introEase = Ease.InOutCubic;

        [Header("Shake")]
        [Tooltip("How fast the knock oscillates. Higher reads as a sharper impact.")]
        [SerializeField, Min(0.1f)] private float shakeFrequency = 22f;

        public Vector3 Offset => offset;

        public Vector3 IntroOffset => introOffset;

        public Vector3 IntroEulerAngles => introEulerAngles;

        public float IntroDuration => introDuration;

        public Ease IntroEase => introEase;

        public float ShakeFrequency => shakeFrequency;

        public Vector3 EulerAngles => eulerAngles;

        public float SmoothTime => smoothTime;

        public float LateralFollow => lateralFollow;
    }
}
