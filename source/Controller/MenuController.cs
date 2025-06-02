using KorzUtils.Helper;
using MenuChanger;
using MenuChanger.MenuElements;

namespace TrialOfCrusaders.Controller;

internal class MenuController : ModeMenuConstructor
{
    internal static void AddMode() => ModeMenu.AddMode(new MenuController());

    public override void OnEnterMainMenu(MenuPage modeMenu) { }

    public override void OnExitMainMenu() { }

    public override bool TryGetModeButton(MenuPage modeMenu, out BigButton button)
    {
        button = new BigButton(modeMenu, SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities.Placeholder"), "ToC");
        button.OnClick += Button_OnClick;
        return true;
    }

    private void Button_OnClick()
    {
        PhaseController.TransitionTo(Enums.Phase.Initialize);
        UIManager.instance.StartNewGame();
    }
}
