using UnityEngine;

namespace TurretRush.Input
{
    public interface IInputService
    {
        bool IsPressed { get; }

        /// <summary>True only on the frame the press began.</summary>
        bool TapStarted { get; }

        /// <summary>Screen-space movement since the previous frame, in pixels.</summary>
        Vector2 DragDelta { get; }
    }
}
