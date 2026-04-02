using Dungeoneer.GameObjects.Bases;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using System;

namespace Dungeoneer.GameObjects.Monsters;

public class Ogre : ActorBase
{
    public override string ActorName { get; protected set; } = "Ogre";
    public override int HealthPool { get; set; } = 30;
    public override int HealthCurrent { get; set; } = 30;
    public override int MinDamage { get; set; } = 5;
    public override int MaxDamage { get; set; } = 10;
    public override int Armor { get; set; } = 3;
    public override int XPValue { get; set; } = 15;

    public Ogre(
        AnimatedSprite spriteIdle,
        AnimatedSprite spriteMove,
        float xPos,
        float yPos,
        Func<ActorBase, Vector2, bool> canMoveToWorldPos,
        Func<ActorBase, Vector2, ActorBase?> getBlockingActorAtWorldPos,
        int _entityId,
        char mapKind)
        : base(spriteIdle, spriteMove, new Vector2(xPos, yPos), canMoveToWorldPos, getBlockingActorAtWorldPos, _entityId, 'G')
    {
    }
    protected override Vector2? GetDesiredDirection(GameTime gameTime)
    {
        Random rand = new();

        var direction = rand.NextDouble();

        return null; // För att göra råttorna stillastående, ta bort denna rad för att låta dem röra sig

        if (direction >= 0 && direction < 0.25)
            return -Vector2.UnitY; // Up
        else if (direction >= 0.25 && direction < 0.5)
            return Vector2.UnitY; // Down
        else if (direction >= 0.5 && direction < 0.75)
            return -Vector2.UnitX; // Left
        else
            return Vector2.UnitX; // Right
    }

    protected override void UpdateSpriteFacing()
    {
        ActiveSprite.Effects = SpriteEffects.FlipHorizontally;
    }
}
