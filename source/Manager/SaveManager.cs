using System.Collections.Generic;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;

namespace TrialOfCrusaders.Manager;

/// <summary>
/// Manages all save data related things.
/// </summary>
internal static class SaveManager
{
    #region Save Data

    #region Combat Data
    
    public static int CombatLevel
    {
        get => CombatController.CombatLevel;
        set => CombatController.CombatLevel = value;
    }

    public static int SpiritLevel
    {
        get => CombatController.SpiritLevel;
        set => CombatController.SpiritLevel = value;
    }

    public static int EnduranceLevel
    {
        get => CombatController.EnduranceLevel;
        set => CombatController.EnduranceLevel = value;
    }

    public static List<Power> ObtainedPowers
    {
        get => CombatController.ObtainedPowers;
        set => CombatController.ObtainedPowers = value;
    }

    #endregion

    #region Stage Data



    #endregion

    #endregion

    public static void SetupNewRun()
	{

	}

	public static void Unload()
	{

	}
}