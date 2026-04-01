using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;

namespace Dungeoneer.Scenes;

public class GameOverScene : Scene
{
    private GameOverHud _gameOverHud;

    public override void LoadContent()
    {

    }

    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        GumService.Default.Root.Children.Clear();

        _gameOverHud = new GameOverHud();
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

public class GameOverHud : ContainerRuntime
{
    private ContainerRuntime _buttonColumn;

    private AnimatedButton _restartButton;
    private AnimatedButton _mainMenuButton;

    public GameOverHud()
    {
        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        _buttonColumn = CreateButtonColumn();
        _buttonColumn.Anchor(Gum.Wireframe.Anchor.Bottom);
        _buttonColumn.Y = 0f;
        AddChild(_buttonColumn);

        _restartButton = CreateButton("Restart", Gum.Wireframe.Anchor.Center);
        _restartButton.Y = -25f;
        _restartButton.IsFocused = true;
        _restartButton.Click += (s, e) =>
        {
            Core.ChangeScene(new GameScene());
        };
        _buttonColumn.AddChild(_restartButton);

        _mainMenuButton = CreateButton("Main Menu", Gum.Wireframe.Anchor.Center);
        _mainMenuButton.Y = 25f;
        _mainMenuButton.Click += (s, e) =>
        {
            Core.ChangeScene(new TitleScene());
        };
        _buttonColumn.AddChild(_mainMenuButton);
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
        column.Anchor(Gum.Wireframe.Anchor.Center);

        return column;
    }

    private AnimatedButton CreateButton(string buttonText, Gum.Wireframe.Anchor location)
    {
        var button = new AnimatedButton(GameAssets.GameObjectAtlas);
        button.Text = buttonText;
        button.Anchor(location);

        return button;
    }
}