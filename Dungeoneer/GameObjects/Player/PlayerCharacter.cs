using Microsoft.Xna.Framework;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.GameObjects.Player;

public class PlayerCharacter
{
    // The AnimatedSprite used when drawing each slime segment
    private AnimatedSprite _sprite;

    // The current position coordinates of the PC in level.
    public Vector2 Position { get; set; }

    /// <summary>
    /// Creates a new PC using the specified animated sprite.
    /// </summary>
    /// <param name="sprite">The AnimatedSprite to use when drawing the PC.</param>
    public PlayerCharacter(AnimatedSprite sprite, float xPos, float yPos)
    {
        _sprite = sprite;
        Position = new Vector2(xPos, yPos);
    }

    /// <summary>
    /// Draws the PC.
    /// </summary>
    public void Draw()
    {

        _sprite.Draw(Core.SpriteBatch, Position);
    }

    public void Update(GameTime gameTime)
    {
        // Update the animated sprite.
        _sprite.Update(gameTime);
    }
}
