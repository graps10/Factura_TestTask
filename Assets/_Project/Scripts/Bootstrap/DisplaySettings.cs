using UnityEngine;
using VContainer.Unity;

namespace TurretRush.Bootstrap
{
    /// <summary>
    /// Asks the device for the frame rate the game is tuned around.
    ///
    /// Mobile players default to 30 fps regardless of what the screen can do, so
    /// without this every phone runs the game at half the rate it was aimed at -
    /// and aiming a turret by dragging is exactly the kind of input that feels
    /// worse for it. vSync has to go first: while it is on, targetFrameRate is
    /// ignored entirely.
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
