using Dungeoneer.GameObjects.Player;
using Dungeoneer.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;

namespace Dungeoneer.Scenes;

public class GameScene : Scene
{
    // Defines the tilemap to draw.
    private Tilemap _tilemap;

    private PlayerCharacter _playerCharacter;

    // The font to use to render normal text.
    private SpriteFont _font;

    // The font used to render the title text.
    private SpriteFont _font5x;

    private DungeonMap _dungeonMap;

    private bool _showTilesetDebug;

    public override void LoadContent()
    {
        // Load the font for the standard text.
        _font = Core.Content.Load<SpriteFont>("fonts/04B_30");

        // Load the font for the title text.
        _font5x = Content.Load<SpriteFont>("fonts/04B_30_5x");

        // Create the tilemap from the XML configuration file.
        _tilemap = Tilemap.FromFile(Content, "images/tilemap.xml");
        _tilemap.Scale = new Vector2(1.0f, 1.0f);

        _dungeonMap = new DungeonMap(64);
        _dungeonMap.LoadContent(Content, "Images/DungeonAtlas");
        _dungeonMap.LoadMap(Content, "LevelFiles/Level1_w_Boss.txt");

        // 1) Ladda atlas
        TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas.xml");

        // 2) Skapa animation från atlas
        AnimatedSprite pcSprite = atlas.CreateAnimatedSprite("slime-idle-pink-animation");
        pcSprite.Scale = new Vector2(1f, 1f);

        _playerCharacter = new PlayerCharacter(
            pcSprite,
            _dungeonMap.PlayerStart.X,
            _dungeonMap.PlayerStart.Y
        );
    }

    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        //Core.ExitOnEscape = false;

    }
    public override void Update(GameTime gameTime)
    {
        if (Core.Input.Keyboard.IsKeyDown(Keys.F1))
            _showTilesetDebug = true;
        if (Core.Input.Keyboard.IsKeyDown(Keys.F2))
            _showTilesetDebug = false;

        _playerCharacter.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        // Clear the back buffer.
        Core.GraphicsDevice.Clear(Color.CornflowerBlue);

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _dungeonMap.Draw(Core.SpriteBatch);

        _playerCharacter.Draw();

        if (_showTilesetDebug)
        {
            // Rita tileset-debug i övre vänstra hörnet
            _dungeonMap.DrawWallTilesetDebug(Core.SpriteBatch, _font, new Vector2(20, 20));
        }

        // Always end the sprite batch when finished.
        Core.SpriteBatch.End();
    }
}