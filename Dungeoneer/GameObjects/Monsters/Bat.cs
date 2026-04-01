using Dungeoneer.GameObjects.Bases;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;
using System;

namespace Dungeoneer.GameObjects.Monsters;

public class Bat : ActorBase
{
    public override string ActorName { get; protected set; } = "Bat";
    public override int HealthPool { get; set; } = 10;
    public override int HealthCurrent { get; set; } = 10;
    public override int MinDamage { get; set; } = 1;
    public override int MaxDamage { get; set; } = 3;
    public override int Armor { get; set; } = 1;

    public Bat(
        AnimatedSprite spriteIdle,
        AnimatedSprite spriteMove,
        float xPos,
        float yPos,
        Func<ActorBase, Vector2, bool> canMoveToWorldPos,
        Func<ActorBase, Vector2, ActorBase?> getBlockingActorAtWorldPos,
        int _entityId,
        char mapKind)
        : base(spriteIdle, spriteMove, new Vector2(xPos, yPos), canMoveToWorldPos, getBlockingActorAtWorldPos, _entityId, 'b')
    {
    }
    protected override Vector2? GetDesiredDirection(GameTime gameTime)
    {
        if (InCombat)
            return null;

        Random rand = new();

        var direction = rand.NextDouble();

        if (direction >= 0 && direction < 0.25)
            return -Vector2.UnitY; // Up
        else if (direction >= 0.25 && direction < 0.5)
            return Vector2.UnitY; // Down
        else if (direction >= 0.5 && direction < 0.75)
            return -Vector2.UnitX; // Left
        else
            return Vector2.UnitX; // Right
    }
}