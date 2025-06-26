using Newtonsoft.Json;
using System;

namespace TrialOfCrusaders.Data;

[Serializable]
public class ArchiveEntryData
{
    #region Properties

    public string PowerName { get; set; }

    public int PickedAmount { get; set; }

    public int OfferedAmount { get; set; }

    [JsonIgnore]
    public double PickRate => OfferedAmount == 0
        ? 0
        : Math.Round((double)OfferedAmount / PickedAmount * 100, 2, MidpointRounding.AwayFromZero);

    #endregion
}
