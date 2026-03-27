using Dungeoneer.GameObjects.Bases;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;
using System;

namespace Dungeoneer.GameObjects.Monsters;

public class Rat : ActorBase
{
    public override int healthPool { get; set; } = 8;
    public override int minDamage { get; set; } = 2;
    public override int maxDamage { get; set; } = 4;
    public override int armor { get; set; } = 0;

    public Rat(
        AnimatedSprite spriteIdle,
        AnimatedSprite spriteMove,
        float xPos,
        float yPos,
        Func<ActorBase, Vector2, bool> canMoveToWorldPos,
        Func<ActorBase, Vector2, ActorBase?> getBlockingActorAtWorldPos)
        : base(spriteIdle, spriteMove, new Vector2(xPos, yPos), canMoveToWorldPos, getBlockingActorAtWorldPos)
    {

    }
    protected override Vector2? GetDesiredDirection(GameTime gameTime)
    {
        Random rand = new();

        var direction = rand.NextDouble();

        //return null; // För att göra råttorna stillastående, ta bort denna rad för att låta dem röra sig

        if (direction >= 0 && direction < 0.2)
            return -Vector2.UnitY; // Up
        else if (direction >= 0.20 && direction < 0.4)
            return Vector2.UnitY; // Down
        else if (direction >= 0.4 && direction < 0.6)
            return -Vector2.UnitX; // Left
        else if (direction >= 0.6 && direction < 0.8)
            return Vector2.UnitX; // Right
        else
            return Vector2.Zero; // Stand Still
    }

    protected override void MoveToCombatLocation(ActorBase target)
    {
        throw new NotImplementedException();
    }
}
