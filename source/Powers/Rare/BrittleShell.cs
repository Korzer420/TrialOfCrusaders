using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;

namespace TrialOfCrusaders.Powers.Rare;

public class BrittleShell : Power
{
    public int Stacks { get; set; } = 0;

    public override Rarity Tier => Rarity.Rare;

    public override (float, float, float) BonusRates => new(0, 0f, 100f);

    public override DraftPool Pools => DraftPool.Risk | DraftPool.Endurance;

    public override bool CanAppear => !HasPower<AchillesHeel>() && !HasPower<PaleShell>() && !HasPower<CaringShell>() 
        && !HasPower<Sturdy>() && !HasPower<ShiningBound>();

    protected override void Enable() => Stacks = 0;
}
