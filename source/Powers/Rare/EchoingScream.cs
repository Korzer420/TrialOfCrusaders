using KorzUtils.Helper;
using System.Collections;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Uncommon;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Rare;

internal class EchoingScream : Power
{
    private GameObject _scream;

    private GameObject _shriek;

    public override (float, float, float) BonusRates => new(0f, 100f, 0f);

    public override Rarity Tier => Rarity.Rare;

    public override bool CanAppear => CombatController.HasPower<HowlingWraiths>(out _);

    public GameObject Scream
    {
        get
        {
            if (_scream == null)
            {
                _scream = GameObject.Instantiate(HeroController.instance.transform.Find("Spells/Scr Heads").gameObject);
                GameObject.DontDestroyOnLoad(_scream);
            }
            return _scream;
        }
    }

    public GameObject Shriek
    {
        get
        {
            if (_shriek == null)
            {
                _shriek = GameObject.Instantiate(HeroController.instance.transform.Find("Spells/Scr Heads 2").gameObject);
                GameObject.DontDestroyOnLoad(_shriek);
            }
            return _shriek;
        }
    }

    protected override void Enable() => On.HutongGames.PlayMaker.Actions.Tk2dPlayAnimationWithEvents.OnEnter += Tk2dPlayAnimationWithEvents_OnEnter;

    protected override void Disable() => On.HutongGames.PlayMaker.Actions.Tk2dPlayAnimationWithEvents.OnEnter -= Tk2dPlayAnimationWithEvents_OnEnter;

    private IEnumerator EchoScream(GameObject holder, Vector3 position)
    {
        yield return new WaitForSeconds(3f);
        GameObject scream = GameObject.Instantiate(PDHelper.ScreamLevel == 1 ? Scream : Shriek);
        scream.transform.SetParent(holder.transform);
        scream.transform.position = position;
        scream.LocateMyFSM("FSM").FsmVariables.FindFsmBool("Unparent").Value = false;
        scream.LocateMyFSM("FSM").FsmVariables.FindFsmBool("Reposition").Value = false;
        Component.Destroy(scream.LocateMyFSM("Deactivate on Hit"));
        bool echo;
        do
        {
            echo = Random.Range(1, 41) < CombatController.SpiritLevel;
            scream.SetActive(false);
            scream.SetActive(true);
            yield return new WaitForSeconds(Random.Range(3, 6));
        }
        while (echo);
        GameObject.Destroy(holder);
    }

    private void Tk2dPlayAnimationWithEvents_OnEnter(On.HutongGames.PlayMaker.Actions.Tk2dPlayAnimationWithEvents.orig_OnEnter orig, HutongGames.PlayMaker.Actions.Tk2dPlayAnimationWithEvents self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", null))
            if (self.State.Name == "Scream End" || self.State.Name == "Scream End 2")
            {
                GameObject holder = new("Scream Echo");
                holder.SetActive(true);
                holder.AddComponent<Dummy>()
                    .StartCoroutine(EchoScream(holder, HeroController.instance.transform.position + new Vector3(0f, 4f)));
            }
        orig(self);
    }
}
