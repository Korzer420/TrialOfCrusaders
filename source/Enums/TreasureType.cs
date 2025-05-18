namespace TrialOfCrusaders.Enums;

internal enum TreasureType
{
    /// <summary>
    /// 3 power options, may be higher rarity.
    /// </summary>
    NormalOrb,

    /// <summary>
    /// 3 rare power choices.
    /// </summary>
    RareOrb,

    /// <summary>
    /// All 3 stat choices (if not maxed out already).
    /// Includes a more detailed description of the stats.
    /// </summary>
    PrismaticOrb,

    /// <summary>
    /// A combat stat.
    /// </summary>
    CombatOrb,

    /// <summary>
    /// A spirit stat.
    /// </summary>
    SpiritOrb,

    /// <summary>
    /// An endurance stat.
    /// </summary>
    EnduranceOrb,

    /// <summary>
    /// A stat choice between the two lower onces. (If two are tied for the highest stat, just a normal orb)
    /// </summary>
    CatchUpStat,

    Dash,

    Claw,

    Wings,

    Tear,

    CrystalHeart,

    ShadeCloak,

    Lantern,

    Fireball,

    Quake
}
