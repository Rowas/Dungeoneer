using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using System;

namespace Dungeoneer.Scenes;

internal class EndOfVersionScene : Scene
{
    private EndOfVersionHudUI _endOfVersionHudUI;

    private const string PRIMARY_TEXT = "End of current version";
    private const string SUB_TEXT_1 = "Thank you for playing!";
    private const string SUB_TEXT_2 = "We hope you enjoyed your journey through the dungeon!";

    private Vector2 _primaryTextPos;
    private Vector2 _primaryTextOrigin;
    private Vector2 _subTextPos;
    private Vector2 _subTextOrigin;
    private Vector2 _secondarySubTextPos;
    private Vector2 _secondarySubTextOrigin;

    public override void Initialize()
    {
        base.Initialize();

        GumService.Default.Root.Children.Clear();

        InitializeUI();

        Core.ExitOnEscape = false;

        var vp = Core.GraphicsDevice.Viewport;

        float halfWidth = vp.Width * 0.5f;
        float primaryTextHeight = vp.Height * 0.25f;
        float subTextHeight = vp.Height * 0.4f;

        Vector2 size = GameAssets.Font5x.MeasureString(PRIMARY_TEXT);
        _primaryTextPos = new Vector2(halfWidth, primaryTextHeight);
        _primaryTextOrigin = size * 0.5f;

        size = GameAssets.Font5x.MeasureString(SUB_TEXT_1);
        _subTextPos = new Vector2(halfWidth, subTextHeight);
        _subTextOrigin = size * 0.5f;

        size = GameAssets.Font5x.MeasureString(SUB_TEXT_2);
        _secondarySubTextPos = new Vector2(halfWidth, subTextHeight * 1.25f);
        _secondarySubTextOrigin = size * 0.5f;
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        Color dropShadowColor = Color.Black * 0.5f;

        Core.SpriteBatch.DrawString(GameAssets.Font5x, PRIMARY_TEXT, _primaryTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _primaryTextOrigin, 0.75f, SpriteEffects.None, 1.0f);
        Core.SpriteBatch.DrawString(GameAssets.Font5x, PRIMARY_TEXT, _primaryTextPos, Color.HotPink, 0.0f, _primaryTextOrigin, 0.75f, SpriteEffects.None, 1.0f);

        Core.SpriteBatch.DrawString(GameAssets.Font5x, SUB_TEXT_1, _subTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _subTextOrigin, 0.33f, SpriteEffects.None, 1.0f);
        Core.SpriteBatch.DrawString(GameAssets.Font5x, SUB_TEXT_1, _subTextPos, Color.HotPink, 0.0f, _subTextOrigin, 0.33f, SpriteEffects.None, 1.0f);

        Core.SpriteBatch.DrawString(GameAssets.Font5x, SUB_TEXT_2, _secondarySubTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _secondarySubTextOrigin, 0.33f, SpriteEffects.None, 1.0f);
        Core.SpriteBatch.DrawString(GameAssets.Font5x, SUB_TEXT_2, _secondarySubTextPos, Color.HotPink, 0.0f, _secondarySubTextOrigin, 0.33f, SpriteEffects.None, 1.0f);

        Core.SpriteBatch.End();

        _endOfVersionHudUI.Draw();
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();

        _endOfVersionHudUI = new EndOfVersionHudUI();

        _endOfVersionHudUI.RestartGameButtonClick += RestartGame;
        _endOfVersionHudUI.MainMenuButtonClick += ShowMainMenu;
        _endOfVersionHudUI.CreditsButtonClick += ShowCredits;
        _endOfVersionHudUI.QuitButtonClick += QuitGame;
    }

    public override void Update(GameTime gameTime)
    {
        _endOfVersionHudUI.Update(gameTime);
    }

    public void RestartGame(object sender, EventArgs args)
    {
        Core.ChangeScene(new GameScene("level1"));
    }

    public void ShowMainMenu(object sender, EventArgs args)
    {
        Core.ChangeScene(new TitleScene());
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
