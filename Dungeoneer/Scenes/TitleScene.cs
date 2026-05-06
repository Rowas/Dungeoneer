using Dungeoneer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using System;

namespace Dungeoneer.Scenes;

public class TitleScene : Scene
{
    private TitleSceneHudUI _titleSceneHudUI;

    public override void LoadContent()
    {

    }

    public override void Initialize()
    {
        base.Initialize();

        GumService.Default.Root.Children.Clear();

        InitializeUI();

        Core.ExitOnEscape = false;
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        Core.SpriteBatch.End();

        _titleSceneHudUI.Draw();
    }

    private void InitializeUI()
    {
        // Clear out any previous UI element incase we came here
        // from a different scene.
        GumService.Default.Root.Children.Clear();

        // Create the game scene ui instance.
        _titleSceneHudUI = new TitleSceneHudUI();

        // Subscribe to the events from the game scene ui.
        _titleSceneHudUI.StartGameButtonClick += StartGame;
        _titleSceneHudUI.LoadGameButtonClick += LoadGame;
        _titleSceneHudUI.CreditsButtonClick += ShowCredits;
        _titleSceneHudUI.QuitButtonClick += QuitGame;
    }

    public override void Update(GameTime gameTime)
    {
        _titleSceneHudUI.Update(gameTime);
    }

    public void StartGame(object sender, EventArgs args)
    {
        Core.ChangeScene(new GameScene("level1"));
    }

    public void LoadGame(object sender, EventArgs args)
    {
        Core.ChangeScene(new SaveLoadScene());
    }

    public void ShowCredits(object sender, EventArgs args)
    {
        Core.ChangeScene(new CreditsScene());
    }

    public void QuitGame(object sender, EventArgs args)
    {
        Core.Instance.Exit();
    }
}