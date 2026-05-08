using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;

namespace Dungeoneer.Scenes;

public class GameOverScene : Scene
{
    private GameOverHudUI _gameOverHud;

    public override void LoadContent()
    {

    }

    public override void Initialize()
    {
        base.Initialize();

        GumService.Default.Root.Children.Clear();

        _gameOverHud = new GameOverHudUI();

        Core.ExitOnEscape = false;
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        var vp = Core.GraphicsDevice.Viewport;
        var dst = new Rectangle(0, 0, vp.Width, vp.Height);

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        Core.SpriteBatch.Draw(GameAssets.GameOverBackground, dst, Color.White);

        Core.SpriteBatch.End();

        _gameOverHud.Draw();
    }

    public override void Update(GameTime gameTime)
    {
        _gameOverHud.Update(gameTime);
    }
}