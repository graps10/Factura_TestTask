using UnityEngine;

namespace TurretRush.Config
{
    [CreateAssetMenu(menuName = "TurretRush/Car Config", fileName = "CarConfig")]
    public sealed class CarConfig : ScriptableObject
    {
        [Header("Durability")]
        [SerializeField, Min(1)] private int maxHealth = 100;

        [Header("Motion")]
        [SerializeField, Min(0f)] private float speed = 12f;

        [Tooltip("Metres per second squared while spinning up. Low enough that the " +
                 "start reads as a launch rather than a teleport.")]
        [SerializeField, Min(0.1f)] private float acceleration = 8f;

        [SerializeField, Min(0.1f)] private float braking = 14f;

        [Header("Lateral drift")]
        [Tooltip("Maximum sideways offset from the centre line, in metres. Clamped at " +
                 "runtime to the level's drivable half width.")]
        [SerializeField, Min(0f)] private float driftAmplitude = 4f;

        [Tooltip("Radians per metre travelled. 0.035 gives roughly a 180 m wavelength.")]
        [SerializeField, Min(0.0001f)] private float driftFrequency = 0.035f;

        [Tooltip("Shifts the whole curve. Change it to get a different-looking road.")]
        [SerializeField] private float driftPhase;

        [Tooltip("Distance ahead sampled to work out which way the car is pointing.")]
        [SerializeField, Min(0.1f)] private float headingLookAhead = 2f;

        public int MaxHealth => maxHealth;

        public float Speed => speed;

        public float Acceleration => acceleration;

        public float Braking => braking;

        public float DriftAmplitude => driftAmplitude;

        public float DriftFrequency => driftFrequency;

        public float DriftPhase => driftPhase;

        public float HeadingLookAhead => headingLookAhead;
    }
}
