using UnityEngine;
using VContainer.Unity;

namespace TurretRush.Bootstrap
{
    /// <summary>
    /// Asks the device for the frame rate the game is tuned around. Mobile players
    /// default to 30 regardless of the screen, and aiming by dragging a finger is
    /// exactly the input that suffers for it. vSync goes first: while it is on,
    /// targetFrameRate is ignored.
    /// </summary>
    public sealed class DisplaySettings : IStartable
    {
        private const int TargetFrameRate = 60;

        public void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFrameRate;

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
