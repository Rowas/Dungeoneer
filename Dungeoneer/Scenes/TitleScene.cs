using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;

namespace Dungeoneer.Scenes;

public class TitleScene : Scene
{
    private MainMenuHud _mainMenuHud;

    public override void LoadContent()
    {

    }

    public override void Initialize()
    {
        base.Initialize();

        GumService.Default.Root.Children.Clear();

        _mainMenuHud = new MainMenuHud();

        //Core.ChangeScene(new GameScene());
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        var vp = Core.GraphicsDevice.Viewport;
        var dst = new Rectangle(0, 0, vp.Width, vp.Height);

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        //Core.SpriteBatch.Draw(GameAssets.GameOverBackground, dst, Color.White);

        Core.SpriteBatch.End();

        _mainMenuHud.Draw();
    }

    public override void Update(GameTime gameTime)
    {
        _mainMenuHud.Update(gameTime);
    }
}


public class MainMenuHud : ContainerRuntime
{
    private ContainerRuntime _buttonColumn;

    private AnimatedButton _startGameButton;
    private AnimatedButton _quitGameButton;

    public MainMenuHud()
    {
        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        _buttonColumn = CreateButtonColumn();
        _buttonColumn.Anchor(Gum.Wireframe.Anchor.Center);
        _buttonColumn.Y = 0f;
        AddChild(_buttonColumn);

        _startGameButton = CreateButton("Start Game", Gum.Wireframe.Anchor.Center);
        _startGameButton.Y = -25f;
        _startGameButton.IsFocused = true;
        _startGameButton.Click += (s, e) =>
        {
            Core.ChangeScene(new GameScene());
        };
        _buttonColumn.AddChild(_startGameButton);

        _quitGameButton = CreateButton("Quit Game", Gum.Wireframe.Anchor.Center);
        _quitGameButton.Y = 25f;
        _quitGameButton.Click += (s, e) =>
        {
            Core.Instance.Exit();
        };
        _buttonColumn.AddChild(_quitGameButton);
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