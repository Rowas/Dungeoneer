using Dungeoneer.GameObjects.Bases;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;
using System;
using System.Collections.Generic;

namespace Dungeoneer.GameObjects.Player;

public class PlayerCharacter : ActorBase
{
    public override int healthPool { get; set; } = 20;
    public override int minDamage { get; set; } = 2;
    public override int maxDamage { get; set; } = 6;
    public override int armor { get; set; } = 1;

    private readonly Queue<Vector2> _inputBuffer = new(0);

    public PlayerCharacter(
        AnimatedSprite spriteIdle,
        AnimatedSprite spriteMove,
        float xPos,
        float yPos,
        Func<ActorBase, Vector2, bool> canMoveToWorldPos)
        : base(spriteIdle, spriteMove, new Vector2(xPos, yPos), canMoveToWorldPos)
    {
    }

    public override void Update(GameTime gameTime)
    {
        HandleInput();

        base.Update(gameTime);
    }

    protected override Vector2? GetDesiredDirection(GameTime gameTime)
    {
        if (_inputBuffer.Count == 0) return null;
        return _inputBuffer.Dequeue();
    }

    private void HandleInput()
    {
        Vector2 next = Vector2.Zero;

        if (GameController.MoveUp()) next = -Vector2.UnitY;
        else if (GameController.MoveDown()) next = Vector2.UnitY;
        else if (GameController.MoveLeft()) next = -Vector2.UnitX;
        else if (GameController.MoveRight()) next = Vector2.UnitX;

        if (next != Vector2.Zero && _inputBuffer.Count == 0)
            _inputBuffer.Enqueue(next);
        else _inputBuffer.Clear();
    }
}
