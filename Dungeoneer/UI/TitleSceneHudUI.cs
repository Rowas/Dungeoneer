using Dungeoneer.GameObjects.Helpers;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using System;

namespace Dungeoneer.UI;

public class TitleSceneHudUI : ContainerRuntime
{
    private ContainerRuntime _buttonColumn;

    private AnimatedButton _startGameButton;
    private AnimatedButton _quitGameButton;
    private AnimatedButton _creditsButton;
    private AnimatedButton _saveLoadGameButton;

    public event EventHandler StartGameButtonClick;
    public event EventHandler LoadGameButtonClick;
    public event EventHandler QuitButtonClick;
    public event EventHandler CreditsButtonClick;

    public TitleSceneHudUI()
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
        _startGameButton.Click += StartGameClick;
        _buttonColumn.AddChild(_startGameButton);

        _saveLoadGameButton = CreateButton("Load Game", Gum.Wireframe.Anchor.Center);
        _saveLoadGameButton.Y = 25f;
        _saveLoadGameButton.Click += LoadGameClick;
        _buttonColumn.AddChild(_saveLoadGameButton);

        _creditsButton = CreateButton("Credits", Gum.Wireframe.Anchor.Center);
        _creditsButton.Y = 75f;
        _creditsButton.Click += ShowCreditsClick;
        _buttonColumn.AddChild(_creditsButton);

        _quitGameButton = CreateButton("Quit Game", Gum.Wireframe.Anchor.Center);
        _quitGameButton.Y = 125f;
        _quitGameButton.Click += QuitGameClick;
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

    private void StartGameClick(object sender, EventArgs args)
    {
        StartGameButtonClick(sender, args);
    }

    private void LoadGameClick(object sender, EventArgs args)
    {
        LoadGameButtonClick(sender, args);
    }

    private void ShowCreditsClick(object sender, EventArgs args)
    {
        CreditsButtonClick(sender, args);
    }

    private void QuitGameClick(object sender, EventArgs args)
    {
        QuitButtonClick(sender, args);
    }
}
