using Dungeoneer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;

namespace Dungeoneer.Scenes;

public class CreditsScene : Scene
{
    private CreditsHudUI _creditsUI;

    public override void LoadContent()
    {

    }

    public override void Initialize()
    {
        base.Initialize();

        GumService.Default.Root.Children.Clear();

        _creditsUI = new CreditsHudUI();
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        var vp = Core.GraphicsDevice.Viewport;
        var dst = new Rectangle(0, 0, vp.Width, vp.Height);

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        Core.SpriteBatch.End();

        _creditsUI.Draw();
    }

    public override void Update(GameTime gameTime)
    {
        _creditsUI.Update(gameTime);
    }
}