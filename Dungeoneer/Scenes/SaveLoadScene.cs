using Dungeoneer.GameObjects.GameSessions;
using Dungeoneer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using System;


namespace Dungeoneer.Scenes;

public class SaveLoadScene : Scene
{
    private SaveLoadHudUI _saveLoadHud;
    private readonly bool _activeGame;
    private readonly GameSession _currentSession;
    private SaveGameService _saveLoadService = new();

    public SaveLoadScene(bool activeGame = false, GameSession currrentSession = null)
    {
        _activeGame = activeGame;
        _currentSession = currrentSession;
    }

    public override void LoadContent()
    {

    }

    public override void Initialize()
    {
        base.Initialize();

        GumService.Default.Root.Children.Clear();

        _saveLoadHud = new SaveLoadHudUI(_activeGame);

        _saveLoadHud.ActiveGameBack += OnActiveGameBackClicked;
        _saveLoadHud.NoActiveGameBack += OnNoActiveGameBackClicked;

        Core.ExitOnEscape = false;
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        Core.SpriteBatch.End();

        _saveLoadHud.Draw();
    }

    public override void Update(GameTime gameTime)
    {
        _saveLoadHud.Update(gameTime);
    }

    private void OnActiveGameBackClicked(object sender, EventArgs args)
    {
        //if (_currentSession != null)
        //{
        //    _saveLoadService.SaveGame(_currentSession);
        //}
        Core.ChangeScene(new GameScene(_currentSession.Level, _currentSession));
    }

    private void OnNoActiveGameBackClicked(object sender, EventArgs args)
    {
        Core.ChangeScene(new TitleScene());
    }
}