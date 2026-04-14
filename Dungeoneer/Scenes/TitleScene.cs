using Dungeoneer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;

namespace Dungeoneer.Scenes;

public class TitleScene : Scene
{
    private MainMenuHudUI _mainMenuHud;

    public override void LoadContent()
    {

    }

    public override void Initialize()
    {
        base.Initialize();

        GumService.Default.Root.Children.Clear();

        _mainMenuHud = new MainMenuHudUI();

        Core.ExitOnEscape = false;
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        Core.SpriteBatch.End();

        _mainMenuHud.Draw();
    }

    public override void Update(GameTime gameTime)
    {
        _mainMenuHud.Update(gameTime);
    }
}