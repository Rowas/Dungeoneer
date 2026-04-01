using Gum.DataTypes;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;

namespace Dungeoneer.UI;

public class GameHudUI : ContainerRuntime
{
    public const float TopBarHeightPx = 64f * 2f;

    private static readonly string s_hpFormat = "HP: {0:D2}/{1:D2}";
    private static readonly string s_xpFormat = "XP: {0:D2}/{1:D2}";

    private ContainerRuntime _topBar;
    private TextRuntime _hpText;
    private TextRuntime _xpText;

    public GameHudUI()
    {
        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        _topBar = CreateTopBar();
        AddChild(_topBar);

        _hpText = CreateHpText();
        _topBar.AddChild(_hpText);

        _xpText = CreateXpText();
        _topBar.AddChild(_xpText);
    }

    private static ContainerRuntime CreateTopBar()
    {
        var top = new ContainerRuntime();
        top.Dock(Gum.Wireframe.Dock.Top);
        top.HeightUnits = DimensionUnitType.Absolute;
        top.Height = TopBarHeightPx;

        return top;
    }

    private static TextRuntime CreateHpText()
    {
        var text = new TextRuntime();
        text.Anchor(Gum.Wireframe.Anchor.TopLeft);
        text.WidthUnits = DimensionUnitType.RelativeToChildren;
        text.X = 20f;
        text.Y = 10f;
        text.UseCustomFont = true;
        text.CustomFontFile = "fonts/04b_30.fnt";
        text.Color = Color.Red;
        text.FontScale = 1.20f;
        text.Text = string.Format(s_hpFormat, 0, 0);
        return text;
    }

    private static TextRuntime CreateXpText()
    {
        var text = new TextRuntime();
        text.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        text.WidthUnits = DimensionUnitType.RelativeToChildren;
        text.X = 20f;
        text.Y = -10f;
        text.UseCustomFont = true;
        text.CustomFontFile = "fonts/04b_30.fnt";
        text.Color = Color.White;
        text.FontScale = 1.20f;
        text.Text = string.Format(s_xpFormat, 0, 0);
        return text;
    }

    public void SetHp(int current, int max)
    {
        _hpText.Text = string.Format(s_hpFormat, current, max);
    }

    public void SetXp(int current, int max)
    {
        _xpText.Text = string.Format(s_xpFormat, current, max);
    }

    public void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
    }

    public void Draw()
    {
        GumService.Default.Draw();
    }
}
