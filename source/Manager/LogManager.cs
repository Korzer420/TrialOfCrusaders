using KorzUtils.Enums;
using KorzUtils.Helper;
using System;

namespace TrialOfCrusaders.Manager;

internal static class LogManager
{
    internal static void Log(string message, LogType logType)
    {
#if DEBUG
        LogHelper.Write<TrialOfCrusaders>(message, logType);
#endif
    }

    internal static void Log(string message, Exception exception) => LogHelper.Write<TrialOfCrusaders>(message, exception);
}
