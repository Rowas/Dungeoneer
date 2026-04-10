using Dungeoneer.GameObjects.GameSessions;
using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.Scenes;
using Gum.DataTypes;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using MonoGameLibrary;
using RenderingLibrary.Graphics;

namespace Dungeoneer.UI;

public class LevelUpHudUI : ContainerRuntime
{
    private ContainerRuntime _buttonColumn;

    private AnimatedButton _continueGameButton;

    private TextRuntime _levelIncreaseLine;
    private TextRuntime _hpIncreaseLine;
    private TextRuntime _minDmgIncreaseLine;
    private TextRuntime _maxDmgIncreaseLine;
    private TextRuntime _armorIncreaseLine;
    private TextRuntime _newXPLine;

    private readonly string s_levelFormat = "Level increased to: {0:D2}";
    private readonly string s_hpFormat = "Max HP Increased by: {0:D2}";
    private readonly string s_minDmgFormat = "Min Damage Increased by: {0:D2}";
    private readonly string s_maxDmgFormat = "Max Damage Increased by: {0:D2}";
    private readonly string s_armorFormat = "Armor Increased by: {0:D2}";
    private readonly string s_xpFormat = "New required XP: {0:D2}";



    public LevelUpHudUI(GameSession session, LevelUp leveled)
    {
        if (leveled == null)
            return;

        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        _buttonColumn = CreateColumn();
        _buttonColumn.Anchor(Gum.Wireframe.Anchor.Bottom);
        _buttonColumn.Y = 0f;
        AddChild(_buttonColumn);

        _continueGameButton = CreateButton("Continue", Gum.Wireframe.Anchor.Center);
        _continueGameButton.Y = -25f;
        _continueGameButton.IsFocused = true;
        _continueGameButton.Click += (s, e) =>
        {
            Core.ChangeScene(new GameScene(session.Level, session));
        };
        _buttonColumn.AddChild(_continueGameButton);

        var LineStack = new ContainerRuntime();
        LineStack.Anchor(Gum.Wireframe.Anchor.Center);
        LineStack.Y = 30f;
        LineStack.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        LineStack.WidthUnits = DimensionUnitType.RelativeToChildren;
        LineStack.HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(LineStack);

        _levelIncreaseLine = CreateTextLine();
        _levelIncreaseLine.Anchor(Gum.Wireframe.Anchor.Center);
        _levelIncreaseLine.Y = -60f;
        _levelIncreaseLine.Text = string.Format(s_levelFormat, session.Player.CurrentLevel);
        LineStack.AddChild(_levelIncreaseLine);

        _hpIncreaseLine = CreateTextLine();
        _hpIncreaseLine.Anchor(Gum.Wireframe.Anchor.Center);
        _hpIncreaseLine.Y = 0f;
        _hpIncreaseLine.Text = string.Format(s_hpFormat, leveled.HealthPoolIncrease);
        LineStack.AddChild(_hpIncreaseLine);

        _minDmgIncreaseLine = CreateTextLine();
        _minDmgIncreaseLine.Anchor(Gum.Wireframe.Anchor.Center);
        _minDmgIncreaseLine.Y = 30f;
        _minDmgIncreaseLine.Text = string.Format(s_minDmgFormat, leveled.MinDamageIncrease);
        LineStack.AddChild(_minDmgIncreaseLine);

        _maxDmgIncreaseLine = CreateTextLine();
        _maxDmgIncreaseLine.Anchor(Gum.Wireframe.Anchor.Center);
        _maxDmgIncreaseLine.Y = 60f;
        _maxDmgIncreaseLine.Text = string.Format(s_maxDmgFormat, leveled.MaxDamageIncrease);
        LineStack.AddChild(_maxDmgIncreaseLine);

        _armorIncreaseLine = CreateTextLine();
        _armorIncreaseLine.Anchor(Gum.Wireframe.Anchor.Center);
        _armorIncreaseLine.Y = 90f;
        _armorIncreaseLine.Text = string.Format(s_armorFormat, leveled.ArmorIncrease);
        LineStack.AddChild(_armorIncreaseLine);

        _newXPLine = CreateTextLine();
        _newXPLine.Anchor(Gum.Wireframe.Anchor.Center);
        _newXPLine.Y = 150f;
        _newXPLine.Text = string.Format(s_xpFormat, leveled.NewXPRequirement);
        LineStack.AddChild(_newXPLine);
    }

    public void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
    }

    public void Draw()
    {
        GumService.Default.Draw();
    }

    private ContainerRuntime CreateColumn()
    {

        var column = new ContainerRuntime();
        column.Anchor(Gum.Wireframe.Anchor.Center);

        return column;
    }

    private AnimatedButton CreateButton(string buttonText, Gum.Wireframe.Anchor location)
    {
        var button = new AnimatedButton(GameAssets.GameObjectAtlas);
        button.Text = buttonText;
        button.Anchor(location);

        return button;
    }

    private TextRuntime CreateTextLine()
    {
        var line = new TextRuntime();
        line.Anchor(Gum.Wireframe.Anchor.TopLeft);
        line.WidthUnits = DimensionUnitType.RelativeToChildren;
        line.HorizontalAlignment = HorizontalAlignment.Center;
        line.VerticalAlignment = VerticalAlignment.Center;
        line.UseCustomFont = true;
        line.CustomFontFile = "fonts/04b_30.fnt";
        line.Color = Color.White;
        line.FontScale = 0.75f;

        return line;
    }
}