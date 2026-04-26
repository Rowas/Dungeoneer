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
    private GameSession _currentSession;
    private SaveGameService _saveLoadService = new();

    public SaveLoadScene(bool activeGame = false, GameSession currentSession = null)
    {
        _activeGame = activeGame;
        _currentSession = currentSession;
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
        _saveLoadHud.GameSelectionButton += GameSelectionButtonclicked;

        Core.ExitOnEscape = false;
    }

    private void GameSelectionButtonclicked(object sender, (string, string) e)
    {
        if (e.Item2 == "save")
        {
            // Handle save game logic here
            _saveLoadService.SaveGame(_currentSession, e.Item1);
        }
        else if (e.Item2 == "load")
        {
            // Handle load game logic here
            if (_saveLoadService.SaveGameExists(e.Item1))
            {
                _currentSession = _saveLoadService.LoadGame(e.Item1);
                Core.ChangeScene(new GameScene(_currentSession.Level, _currentSession));
            }
        }
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
        Core.ChangeScene(new GameScene(_currentSession.Level, _currentSession));
    }

    private void OnNoActiveGameBackClicked(object sender, EventArgs args)
    {
        Core.ChangeScene(new TitleScene());
    }
}