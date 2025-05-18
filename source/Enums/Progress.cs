using System;

namespace TrialOfCrusaders.Enums;

[Flags]
public enum Progress
{
    None = 0,

    Dash = 1,

    Claw = 2,

    Fireball = 4,

    Quake = 8,

    ShadeCloak = 16,

    CrystalHeart = 32,

    Tear = 64,

    Wings = 128,

    Lantern = 256
}
