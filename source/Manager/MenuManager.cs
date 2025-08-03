using KorzUtils.Helper;
using MenuChanger;
using MenuChanger.MenuElements;

namespace TrialOfCrusaders.Manager;

/// <summary>
/// Handles the menu in the file selection.
/// </summary>
public class MenuManager : ModeMenuConstructor
{
    internal static void AddMode() => ModeMenu.AddMode(new MenuManager());

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
        PhaseManager.TransitionTo(Enums.Phase.Initialize);
        UIManager.instance.StartNewGame();
    }
}
