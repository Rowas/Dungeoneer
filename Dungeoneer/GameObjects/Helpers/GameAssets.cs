using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.GameObjects.Helpers;

public static class GameAssets
{
    public static SpriteFont Font { get; private set; }
    public static SpriteFont Font5x { get; private set; }
    public static TextureAtlas GameObjectAtlas { get; private set; }
    public static Texture2D GameOverBackground { get; private set; }

    public static void Load()
    {
        Font = Core.Content.Load<SpriteFont>("fonts/04B_30");
        Font5x = Core.Content.Load<SpriteFont>("fonts/04B_30_5x");
        GameObjectAtlas = TextureAtlas.FromFile(Core.Content, "images/GameObjectAtlas.xml");
        GameOverBackground = Core.Content.Load<Texture2D>("Images/GameOverBackground");
    }
}
