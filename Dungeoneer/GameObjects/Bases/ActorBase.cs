using Dungeoneer.GameObjects.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System;

namespace Dungeoneer.GameObjects.Bases;

/// <summary>
/// Gemensam bas för alla levande/rörliga aktörer på kartan.
/// Hanterar position, tile-move interpolation och animation/draw.
/// </summary>
public abstract class ActorBase
{
    protected AnimatedSprite IdleSprite { get; set; }
    protected AnimatedSprite MoveSprite { get; set; }
    protected AnimatedSprite ActiveSprite { get; set; }

    public Vector2 Position { get; protected set; }     // Aktuell tile-position i world space
    public Vector2 Direction { get; protected set; }    // Senast valda riktning
    protected int Heading { get; set; } = 1;            // 1 right, -1 left

    // Interpolation state (för mjuk förflyttning mellan tiles)
    protected Vector2 From { get; set; }
    protected Vector2 To { get; set; }
    protected float MoveProgress01 { get; set; }
    protected TimeSpan MoveAnimRemaining { get; set; }
    protected TimeSpan MoveAnimDuration { get; set; }
    public bool IsMoving => MoveAnimRemaining > TimeSpan.Zero;
    public Vector2 TargetPosition => To;

    private readonly Func<ActorBase, Vector2, bool> _canMoveToWorldPos;
    private readonly Func<ActorBase, Vector2, ActorBase?> _getBlockingActorAtWorldPos;

    public event Action<ActorBase, ActorBase>? BlockedByActor;

    public abstract int healthPool { get; set; }
    public abstract int minDamage { get; set; }
    public abstract int maxDamage { get; set; }
    public abstract int armor { get; set; }

    protected ActorBase(
        AnimatedSprite idleSprite,
        AnimatedSprite moveSprite,
        Vector2 startPosition,
        Func<ActorBase, Vector2, bool> canMoveToWorldPos,
        Func<ActorBase, Vector2, ActorBase?> getBlockingActorAtWorldPos)
    {
        IdleSprite = idleSprite;
        MoveSprite = moveSprite;
        ActiveSprite = idleSprite;
        _canMoveToWorldPos = canMoveToWorldPos;
        _getBlockingActorAtWorldPos = getBlockingActorAtWorldPos;
        Position = startPosition;
        From = startPosition;
        To = startPosition;
        if (MoveSprite?.Animation != null)
        {
            MoveAnimDuration = new TimeSpan(
                MoveSprite.Animation.Delay.Ticks * MoveSprite.Animation.Frames.Count
            );
        }
        else
        {
            MoveAnimDuration = TimeSpan.FromMilliseconds(150);
        }
        MoveAnimRemaining = TimeSpan.Zero;
        MoveProgress01 = 1f;
    }

    public virtual void Update(GameTime gameTime)
    {
        TickMovementAnimation(gameTime);

        // Underklass avgör NÄR/VART den vill röra sig (input, AI etc.)
        if (CanStartNewStep())
        {
            Vector2? desiredDirection = GetDesiredDirection(gameTime);
            if (desiredDirection.HasValue)
            {
                TryStartStep(desiredDirection.Value);
            }
        }

        SelectActiveSprite();
        UpdateSpriteFacing();
        ActiveSprite?.Update(gameTime);
    }

    public virtual void Draw()
    {
        ActiveSprite?.Draw(Core.SpriteBatch, GetDrawPosition());
    }

    protected virtual Vector2 GetDrawPosition()
    {
        if (MoveAnimRemaining > TimeSpan.Zero)
            return Vector2.Lerp(From, To, MoveProgress01);

        return Position;
    }

    protected virtual bool CanStartNewStep()
        => MoveAnimRemaining <= TimeSpan.Zero;

    protected abstract Vector2? GetDesiredDirection(GameTime gameTime);
    protected virtual bool CanEnterWorldPosition(Vector2 candidateWorldPos)
    => _canMoveToWorldPos?.Invoke(this, candidateWorldPos) ?? false;

    protected virtual int GetStepSize() => 64;

    protected bool TryStartStep(Vector2 desiredDirection)
    {
        if (desiredDirection == Vector2.Zero) return false;

        Direction = Normalize4(desiredDirection);

        int step = GetStepSize();
        Vector2 candidateTo = Position + Direction * step;

        if (!CanEnterWorldPosition(candidateTo))
        {
            var blocker = _getBlockingActorAtWorldPos?.Invoke(this, candidateTo);
            if (blocker != null)
                BlockedByActor?.Invoke(this, blocker);
            return false;
        }

        From = Position;
        To = candidateTo;
        MoveAnimRemaining = MoveAnimDuration;
        MoveProgress01 = 0f;

        if (Direction.X > 0) Heading = 1;
        else if (Direction.X < 0) Heading = -1;

        return true;
    }

    protected virtual void TickMovementAnimation(GameTime gameTime)
    {
        if (MoveAnimRemaining > TimeSpan.Zero)
            MoveAnimRemaining -= gameTime.ElapsedGameTime;

        if (MoveAnimDuration > TimeSpan.Zero)
        {
            var elapsed = MoveAnimDuration - MoveAnimRemaining;
            MoveProgress01 = MathHelper.Clamp(
                (float)(elapsed.TotalSeconds / MoveAnimDuration.TotalSeconds),
                0f, 1f
            );
        }
        else
        {
            MoveProgress01 = 1f;
        }

        if (MoveAnimRemaining <= TimeSpan.Zero && To != From)
        {
            Position = To;
            From = To;
            MoveProgress01 = 1f;
        }
    }

    protected virtual void SelectActiveSprite()
    {
        ActiveSprite = (MoveAnimRemaining > TimeSpan.Zero) ? MoveSprite : IdleSprite;
    }

    protected virtual void UpdateSpriteFacing()
    {
        if (ActiveSprite != null)
        {
            ActiveSprite.Effects = (Heading < 0)
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;
        }
    }

    protected virtual void MoveToCombatLocation(ActorBase target)
    {
        if (target.GetType() == typeof(PlayerCharacter))
        {
            target.Position = new Vector2();
            target.Heading = 1;
        }
        else
        {
            target.Position = new Vector2();
            target.Heading = -1;
        }
    }

    private static Vector2 Normalize4(Vector2 d)
    {
        if (Math.Abs(d.X) > Math.Abs(d.Y))
            return new Vector2(Math.Sign(d.X), 0f);
        if (Math.Abs(d.Y) > 0f)
            return new Vector2(0f, Math.Sign(d.Y));
        return Vector2.Zero;
    }

}