namespace TrialOfCrusaders.Enums;

internal enum RunResult
{
    /// <summary>
    /// Successfully ended the run.
    /// </summary>
    Completed,

    /// <summary>
    /// Died in the run.
    /// </summary>
    Failed,

    /// <summary>
    /// Exited the run through the menu (ToDo: If continue run should be implemented, allow a forfeit function somewhere (Dream Gate maybe?))
    /// </summary>
    Forfeited
}
