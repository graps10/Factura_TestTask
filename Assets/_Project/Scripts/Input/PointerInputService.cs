using UnityEngine;
using UnityEngine.InputSystem;

namespace TurretRush.Input
{
    public sealed class PointerInputService : IInputService
    {
        public bool IsPressed
        {
            get
            {
                var pointer = Pointer.current;
                return pointer != null && pointer.press.isPressed;
            }
        }

        public bool TapStarted
        {
            get
            {
                var pointer = Pointer.current;
                return pointer != null && pointer.press.wasPressedThisFrame;
            }
        }

        public Vector2 DragDelta
        {
            get
            {
                var pointer = Pointer.current;
                if (pointer == null || !pointer.press.isPressed)
                    return Vector2.zero;

                return pointer.delta.ReadValue();
            }
        }
    }
}
