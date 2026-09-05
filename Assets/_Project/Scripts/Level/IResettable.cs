namespace TurretRush.Level
{
    /// <summary>
    /// A system holding state a restart has to undo. Implementing it is what puts a
    /// system into the restart, rather than another line in a growing reset method.
    /// </summary>
    public interface IResettable
    {
        void ResetToStart();
    }
}
