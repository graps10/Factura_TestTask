namespace TurretRush.Level
{
    /// <summary>
    /// A system that owns state a level restart has to undo.
    /// </summary>
    public interface IResettable
    {
        void ResetToStart();
    }
}
