using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.Scenes;
using Gum.Forms;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using MonoGameLibrary;

namespace Dungeoneer;

public class Game1 : Core
{
    public Game1() : base("Dungeoneer", 1920, 1080, false)
    {

    }

    protected override void Initialize()
    {
        base.Initialize();

        GameAssets.Load();

        InitializeGum();

        ChangeScene(new TitleScene());
    }

    protected override void LoadContent()
    {

    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        base.Draw(gameTime);
    }

    private void InitializeGum()
    {
        // Initialize the Gum service. The second parameter specifies
        // the version of the default visuals to use.
        GumService.Default.Initialize(this, DefaultVisualsVersion.Newest);

        // Tell the Gum service which content manager to use.  We will tell it to
        // use the global content manager from our Core.
        GumService.Default.ContentLoader.XnaContentManager = Core.Content;

        // Register keyboard input for UI control.
        FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);

        // Register gamepad input for Ui control.
        FrameworkElement.GamePadsForUiControl.AddRange(GumService.Default.Gamepads);

        // Customize the tab reverse UI navigation to also trigger when the keyboard
        // Up arrow key is pushed.
        FrameworkElement.TabReverseKeyCombos.Add(
           new KeyCombo() { PushedKey = Keys.Up });

        // Customize the tab UI navigation to also trigger when the keyboard
        // Down arrow key is pushed.
        FrameworkElement.TabKeyCombos.Add(
           new KeyCombo() { PushedKey = Keys.Down });

        // No change to the asset size or zoom as they are designed
        // to match the rest of the assets in the game.
        GumService.Default.CanvasWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        GumService.Default.CanvasHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        GumService.Default.Renderer.Camera.Zoom = 1.0f;
    }
}
