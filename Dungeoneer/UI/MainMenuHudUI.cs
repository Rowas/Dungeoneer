using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.Scenes;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using MonoGameLibrary;

namespace Dungeoneer.UI;

public class MainMenuHudUI : ContainerRuntime
{
    private ContainerRuntime _buttonColumn;

    private AnimatedButton _startGameButton;
    private AnimatedButton _quitGameButton;
    private AnimatedButton _creditsButton;
    private AnimatedButton _saveLoadGameButton;

    public MainMenuHudUI()
    {
        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        _buttonColumn = CreateButtonColumn();
        _buttonColumn.Anchor(Gum.Wireframe.Anchor.Center);
        _buttonColumn.Y = 0f;
        AddChild(_buttonColumn);

        _startGameButton = CreateButton("Start Game", Gum.Wireframe.Anchor.Center);
        _startGameButton.Y = -25f;
        _startGameButton.IsFocused = true;
        _startGameButton.Click += (s, e) =>
        {
            Core.ChangeScene(new GameScene("level1"));
        };
        _buttonColumn.AddChild(_startGameButton);

        _saveLoadGameButton = CreateButton("Load Game", Gum.Wireframe.Anchor.Center);
        _saveLoadGameButton.Y = 25f;
        _saveLoadGameButton.Click += (s, e) =>
        {
            Core.ChangeScene(new SaveLoadScene());
        };
        _buttonColumn.AddChild(_saveLoadGameButton);

        _creditsButton = CreateButton("Credits", Gum.Wireframe.Anchor.Center);
        _creditsButton.Y = 75f;
        _creditsButton.Click += (s, e) =>
        {
            Core.ChangeScene(new CreditsScene());
        };
        _buttonColumn.AddChild(_creditsButton);

        _quitGameButton = CreateButton("Quit Game", Gum.Wireframe.Anchor.Center);
        _quitGameButton.Y = 125f;
        _quitGameButton.Click += (s, e) =>
        {
            Core.Instance.Exit();
        };
        _buttonColumn.AddChild(_quitGameButton);
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
