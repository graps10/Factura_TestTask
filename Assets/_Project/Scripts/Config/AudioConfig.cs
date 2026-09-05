using System;
using UnityEngine;

namespace TurretRush.Config
{
    /// <summary>One sound and how loudly to play it.</summary>
    [Serializable]
    public sealed class SfxCue
    {
        [SerializeField] private AudioClip clip;

        [SerializeField, Range(0f, 1f)] private float volume = 0.6f;

        [Tooltip("Randomises pitch by this fraction each time. Without it a sound " +
                 "repeated four times a second turns into a machine gun.")]
        [SerializeField, Range(0f, 0.4f)] private float pitchVariance = 0.08f;

        public AudioClip Clip => clip;

        public float Volume => volume;

        public float PitchVariance => pitchVariance;
    }

    [CreateAssetMenu(menuName = "TurretRush/Audio Config", fileName = "AudioConfig")]
    public sealed class AudioConfig : ScriptableObject
    {
        [Tooltip("How many sounds can overlap. Each voice keeps its own pitch, which " +
                 "is why this is a pool rather than one source playing everything.")]
        [SerializeField, Range(2, 16)] private int voiceCount = 8;

        [Header("Gunnery")]
        [Tooltip("Played four times a second - keep it short and quiet.")]
        [SerializeField] private SfxCue shot;

        [SerializeField] private SfxCue enemyHit;
        [SerializeField] private SfxCue enemyKilled;

        [Header("Damage taken")]
        [Tooltip("The one piece of bad news in the game. Loud enough to cut through " +
                 "the gunfire.")]
        [SerializeField] private SfxCue carHit;

        [SerializeField] private SfxCue carDestroyed;

        [Header("Result")]
        [SerializeField] private SfxCue win;

        public int VoiceCount => voiceCount;

        public SfxCue Shot => shot;

        public SfxCue EnemyHit => enemyHit;

        public SfxCue EnemyKilled => enemyKilled;

        public SfxCue CarHit => carHit;

        public SfxCue CarDestroyed => carDestroyed;

        public SfxCue Win => win;
    }
}
