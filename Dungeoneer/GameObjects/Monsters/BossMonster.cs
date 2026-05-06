using Dungeoneer.GameObjects.Bases;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using System;

namespace Dungeoneer.GameObjects.Monsters;

public class BossMonster : ActorBase
{
    public override string ActorName { get; protected set; } = "Boss Monster";
    public override float CombatScale { get; set; } = 3.5f;
    public override int HealthPool { get; set; } = 50;
    public override int HealthCurrent { get; set; } = 50;
    public override int MinDamage { get; set; } = 5;
    public override int MaxDamage { get; set; } = 10;
    public override int Armor { get; set; } = 2;
    public override int XPValue { get; set; } = 25;

    public BossMonster(
        AnimatedSprite spriteIdle,
        AnimatedSprite spriteMove,
        float xPos,
        float yPos,
        Func<ActorBase, Vector2, bool> canMoveToWorldPos,
        Func<ActorBase, Vector2, ActorBase> getBlockingActorAtWorldPos,
        int _entityId,
        char mapKind)
        : base(spriteIdle, spriteMove, new Vector2(xPos, yPos), canMoveToWorldPos, getBlockingActorAtWorldPos, _entityId, 'B')
    {
    }
    protected override Vector2? GetDesiredDirection(GameTime gameTime, Random rand)
    {
        return null;
    }

    protected override void UpdateSpriteFacing()
    {
        ActiveSprite.Effects = SpriteEffects.FlipHorizontally;
    }
}
