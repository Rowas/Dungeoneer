using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.Scenes;
using Gum.DataTypes;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using MonoGameLibrary;
using RenderingLibrary.Graphics;
using System.Collections.Generic;

namespace Dungeoneer.UI;

public class CreditsHudUI : ContainerRuntime
{
    private ContainerRuntime _buttonColumn;

    private AnimatedButton _backButton;

    private int _lineNumber = 1;
    private int _lineSpacing = 40;
    private float _lineStartPosition = -60f;

    public CreditsHudUI()
    {
        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        _buttonColumn = CreateButtonColumn();
        _buttonColumn.Anchor(Gum.Wireframe.Anchor.Bottom);
        _buttonColumn.Y = 0f;
        AddChild(_buttonColumn);

        _backButton = CreateButton("Back", Gum.Wireframe.Anchor.Bottom);
        _backButton.Y = -25f;
        _backButton.IsFocused = true;
        _backButton.Click += (s, e) =>
        {
            Core.ChangeScene(new TitleScene());
        };
        _buttonColumn.AddChild(_backButton);

        var LineStack = new ContainerRuntime();
        LineStack.Anchor(Gum.Wireframe.Anchor.Center);
        LineStack.Y = 30f;
        LineStack.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        LineStack.WidthUnits = DimensionUnitType.RelativeToChildren;
        LineStack.HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(LineStack);

        var titleLine = CreateTextLine();
        titleLine.Text = "Credits";
        titleLine.Anchor(Gum.Wireframe.Anchor.Center);
        titleLine.Y = _lineStartPosition - _lineSpacing * 2;
        titleLine.Color = Color.Brown;
        titleLine.FontScale = 1.5f;
        LineStack.AddChild(titleLine);

        foreach (var credit in _credits)
        {
            var line = CreateTextLine();
            line.Text = $"{credit.Role}: {credit.Name} ({credit.URL})";
            line.Y = _lineStartPosition + (_lineNumber * _lineSpacing);
            LineStack.AddChild(line);
            _lineNumber++;
        }
    }

    public void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
    }

    public void Draw()
    {
        GumService.Default.Draw();
    }

    private ContainerRuntime CreateButtonColumn()
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
        line.Anchor(Gum.Wireframe.Anchor.Center);
        line.WidthUnits = DimensionUnitType.RelativeToChildren;
        line.HorizontalAlignment = HorizontalAlignment.Center;
        line.VerticalAlignment = VerticalAlignment.Center;
        line.UseCustomFont = true;
        line.CustomFontFile = "fonts/04b_30.fnt";
        line.Color = Color.BurlyWood;
        line.FontScale = 0.5f;

        return line;
    }

    private class CreditEntry
    {
        public string Role { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
    }

    private List<CreditEntry> _credits = new List<CreditEntry>
    {
        new CreditEntry { Role = "Game Design", Name = "Andreas 'Rowas' Lind-Sahlin", URL = "https://github.com/Rowas" },
        new CreditEntry { Role = "Programming", Name = "Andreas 'Rowas' Lind-Sahlin", URL = "https://github.com/Rowas" },
        new CreditEntry { Role = "Font Art", Name = "MonoGame Tutorial", URL = "https://docs.monogame.net/articles/tutorials/building_2d_games/16_working_with_spritefonts/index.html" },
        new CreditEntry { Role = "Player Slime Art", Name = "tienlev", URL = "https://tienlev.itch.io/slime-pixel-set" },
        new CreditEntry { Role = "World Tileset Art", Name = "screamingbrainstudios", URL = "https://screamingbrainstudios.itch.io/top-down-dungeon-pack" },
        new CreditEntry { Role = "Weapons and Potions Prop Art", Name = "Shade", URL = "https://merchant-shade.itch.io/16x16-mixed-rpg-icons" },
        new CreditEntry { Role = "Food Prop Art", Name = "ghostpixxells", URL = "https://ghostpixxells.itch.io/pixelfood" },
        new CreditEntry { Role = "Boss Monster Art", Name = "deepdivegamestudio", URL = "https://deepdivegamestudio.itch.io/undead-asset-pack" },
        new CreditEntry { Role = "Vermin Monster Art", Name = "deepdivegamestudio", URL = "https://deepdivegamestudio.itch.io/vermin-asset-pack" },
        new CreditEntry { Role = "Monster Monster Art", Name = "deepdivegamestudio", URL = "https://deepdivegamestudio.itch.io/monsterassetpack" },
        new CreditEntry { Role = "Game Over Splash Art", Name = "gurigraphics", URL = "https://gurigraphics.itch.io/game-over-screen" },
        new CreditEntry { Role = "Original music by: ", Name = "Marllon Silva (xDeviruchi)", URL = "https://www.youtube.com/xdeviruchi" },
        new CreditEntry { Role = "Sound Effects", Name = "JDSherbert", URL = "https://jdsherbert.itch.io/pixel-game-essentials-sfx-pack" },
        new CreditEntry { Role = "Sound Effects", Name = "Leohpaz", URL = "https://leohpaz.itch.io/minifantasy-dungeon-sfx-pack" },
    };
}
