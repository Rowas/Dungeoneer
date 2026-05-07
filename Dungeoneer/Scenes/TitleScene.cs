using Dungeoneer.GameObjects.Helpers;
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

    private const string DUNGEON_TEXT = "Dungeoneer";
    private const string SLIME_TEXT = "Into the Deep";

    private Vector2 _dungeonTextPos;
    private Vector2 _dungeonTextOrigin;
    private Vector2 _slimeTextPos;
    private Vector2 _slimeTextOrigin;

    public override void LoadContent()
    {

    }

    public override void Initialize()
    {
        base.Initialize();

        GumService.Default.Root.Children.Clear();

        InitializeUI();

        Core.ExitOnEscape = false;

        var vp = Core.GraphicsDevice.Viewport;

        float halfWidth = vp.Width * 0.5f;
        float quarterHeight = vp.Height * 0.25f;
        float thirdHeight = vp.Height * 0.33f;

        Vector2 size = GameAssets.Font5x.MeasureString(DUNGEON_TEXT);
        _dungeonTextPos = new Vector2(halfWidth, quarterHeight);
        _dungeonTextOrigin = size * 0.5f;

        size = GameAssets.Font5x.MeasureString(SLIME_TEXT);
        _slimeTextPos = new Vector2(halfWidth, thirdHeight);
        _slimeTextOrigin = size * 0.5f;
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        Color dropShadowColor = Color.Black * 0.5f;

        Core.SpriteBatch.DrawString(GameAssets.Font5x, DUNGEON_TEXT, _dungeonTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _dungeonTextOrigin, 1.0f, SpriteEffects.None, 1.0f);
        Core.SpriteBatch.DrawString(GameAssets.Font5x, DUNGEON_TEXT, _dungeonTextPos, Color.HotPink, 0.0f, _dungeonTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

        Core.SpriteBatch.DrawString(GameAssets.Font5x, SLIME_TEXT, _slimeTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _slimeTextOrigin, 0.33f, SpriteEffects.None, 1.0f);
        Core.SpriteBatch.DrawString(GameAssets.Font5x, SLIME_TEXT, _slimeTextPos, Color.HotPink, 0.0f, _slimeTextOrigin, 0.33f, SpriteEffects.None, 1.0f);

        Core.SpriteBatch.End();

        _titleSceneHudUI.Draw();
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();

        _titleSceneHudUI = new TitleSceneHudUI();

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