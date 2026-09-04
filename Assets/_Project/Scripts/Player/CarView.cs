using UnityEngine;

namespace TurretRush.Player
{
    /// <summary>
    /// The car's presence in the scene. Holds scene references and knows how to
    /// display things; it holds no rules. All movement decisions live in
    /// <see cref="CarMovement"/>.
    /// </summary>
    public sealed class CarView : MonoBehaviour
    {
        [Tooltip("WheelFL / WheelFR / WheelRL / WheelRR. Order does not matter.")]
        [SerializeField] private Transform[] wheels;

        [Tooltip("Measured from the model: 0.36 m.")]
        [SerializeField, Min(0.01f)] private float wheelRadius = 0.36f;

        [Tooltip("Flip if the wheels spin backwards while driving forwards.")]
        [SerializeField] private bool invertWheelSpin;

        public Transform Root => transform;

        /// <param name="travelledDelta">Metres covered since the last frame.</param>
        public void SpinWheels(float travelledDelta)
        {
            if (wheels == null || wheels.Length == 0)
                return;

            // Rolling without slipping: arc length = radius * angle.
            var degrees = travelledDelta / wheelRadius * Mathf.Rad2Deg;
            if (invertWheelSpin)
                degrees = -degrees;

            // The wheel meshes are flat in X, so their axle is the local X axis.
            for (var i = 0; i < wheels.Length; i++)
                wheels[i].Rotate(degrees, 0f, 0f, Space.Self);
        }
    }
}
