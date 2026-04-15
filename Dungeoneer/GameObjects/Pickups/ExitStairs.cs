using Dungeoneer.GameObjects.Bases;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.GameObjects.Pickups;

public class ExitStairs : PropBase
{
    public override string PropName { get; protected set; } = "exit-stairs";

    public ExitStairs(
        Sprite sprite,
        float xPos,
        float yPos,
        int propId,
        char mapKind)
        : base(sprite, new Vector2(xPos, yPos), propId, 'E')
    {

    }

    protected override void OnInteract(ActorBase player)
    {
        // Move to the next level or end the game if this is the final level.
        // This is a placeholder as the functionality currently isn't implemented.
    }
}