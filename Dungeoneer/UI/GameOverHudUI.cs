using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.Scenes;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using MonoGameLibrary;

namespace Dungeoneer.UI;

public class GameOverHudUI : ContainerRuntime
{
    private ContainerRuntime _buttonColumn;

    private AnimatedButton _restartButton;
    private AnimatedButton _mainMenuButton;

    private string _previousLevel;

    public GameOverHudUI(string previousLevel)
    {
        _previousLevel = previousLevel;
        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        _buttonColumn = CreateButtonColumn();
        _buttonColumn.Anchor(Gum.Wireframe.Anchor.Bottom);
        _buttonColumn.Y = 0f;
        AddChild(_buttonColumn);

        _restartButton = CreateButton("Restart", Gum.Wireframe.Anchor.Center);
        _restartButton.Y = -25f;
        _restartButton.IsFocused = true;
        _restartButton.Click += (s, e) =>
        {
            Core.ChangeScene(new GameScene(_previousLevel));
        };
        _buttonColumn.AddChild(_restartButton);

        _mainMenuButton = CreateButton("Main Menu", Gum.Wireframe.Anchor.Center);
        _mainMenuButton.Y = 25f;
        _mainMenuButton.Click += (s, e) =>
        {
            Core.ChangeScene(new TitleScene());
        };
        _buttonColumn.AddChild(_mainMenuButton);
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
}
