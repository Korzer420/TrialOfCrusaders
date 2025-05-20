using KorzUtils.Helper;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ScorchingGround : Power
{
    private GameObject _diveObject;

    public GameObject DiveObject => _diveObject ??= HeroController.instance.transform.Find("Spells/Q Flash Slam").gameObject;

    public override (float, float, float) BonusRates => new(0f, 40f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => HasPower<DesolateDive>();

    protected override void Enable() => On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter += SendMessage_OnEnter;

    protected override void Disable() => On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter -= SendMessage_OnEnter;

    private void SendMessage_OnEnter(On.HutongGames.PlayMaker.Actions.SendMessage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SendMessage self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Quake Finish"))
        {
            GameObject gameObject = new("Burning Ground");
            gameObject.AddComponent<BurningGround>();
            gameObject.transform.position = DiveObject.transform.position;
            gameObject.SetActive(true);
        }
        orig(self);
    }
}