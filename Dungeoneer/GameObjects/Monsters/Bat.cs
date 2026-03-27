using Dungeoneer.GameObjects.Bases;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;
using System;

namespace Dungeoneer.GameObjects.Monsters;

public class Bat : ActorBase
{
    public override int healthPool { get; set; } = 10;
    public override int minDamage { get; set; } = 1;
    public override int maxDamage { get; set; } = 3;
    public override int armor { get; set; } = 1;

    public Bat(
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

        if (direction >= 0 && direction < 0.25)
            return -Vector2.UnitY; // Up
        else if (direction >= 0.25 && direction < 0.5)
            return Vector2.UnitY; // Down
        else if (direction >= 0.5 && direction < 0.75)
            return -Vector2.UnitX; // Left
        else
            return Vector2.UnitX; // Right
    }

    protected override void MoveToCombatLocation(ActorBase target)
    {
        throw new NotImplementedException();
    }
}