using System;

namespace TrialOfCrusaders.Data;

internal static class ExtensionData
{
	#region Methods

    internal static int Lower(this int value, int toSubtract) => Math.Max(0, value - toSubtract);

    #endregion
}
