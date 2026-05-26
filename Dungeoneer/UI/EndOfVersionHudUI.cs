using Dungeoneer.GameObjects.Helpers;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using System;

namespace Dungeoneer.UI;

internal class EndOfVersionHudUI : ContainerRuntime
{
    private ContainerRuntime _buttonColumn;

    private AnimatedButton _restartButton;
    private AnimatedButton _mainMenuButton;
    private AnimatedButton _exitButton;

    public event EventHandler RestartGameButtonClick;
    public event EventHandler MainMenuButtonClick;
    public event EventHandler QuitButtonClick;
    public event EventHandler CreditsButtonClick;

    public EndOfVersionHudUI()
    {
        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        _buttonColumn = CreateButtonColumn();
        _buttonColumn.Anchor(Gum.Wireframe.Anchor.Bottom);
        _buttonColumn.Y = 0f;
        AddChild(_buttonColumn);

        _restartButton = CreateButton("Restart", Gum.Wireframe.Anchor.Center);
        _restartButton.Y = -125f;
        _restartButton.IsFocused = true;
        _restartButton.Click += RestartGameClick;
        _buttonColumn.AddChild(_restartButton);

        _mainMenuButton = CreateButton("Main Menu", Gum.Wireframe.Anchor.Center);
        _mainMenuButton.Y = -75f;
        _mainMenuButton.Click += MainMenuClick;
        _buttonColumn.AddChild(_mainMenuButton);

        //_creditsButton = CreateButton("Credits", Gum.Wireframe.Anchor.Center);
        //_creditsButton.Y = -25f;
        //_creditsButton.Click += ShowCreditsClick;
        //_buttonColumn.AddChild(_creditsButton);

        _exitButton = CreateButton("Quit Game", Gum.Wireframe.Anchor.Center);
        _exitButton.Y = -25f;
        _exitButton.Click += QuitGameClick;
        _buttonColumn.AddChild(_exitButton);
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

        return column;
    }

    private AnimatedButton CreateButton(string buttonText, Gum.Wireframe.Anchor location)
    {
        var button = new AnimatedButton(GameAssets.GameObjectAtlas);
        button.Text = buttonText;
        button.Anchor(location);

        return button;
    }

    private void RestartGameClick(object sender, EventArgs args)
    {
        RestartGameButtonClick(sender, args);
    }

    private void MainMenuClick(object sender, EventArgs args)
    {
        MainMenuButtonClick(sender, args);
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
