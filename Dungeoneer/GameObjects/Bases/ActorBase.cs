using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.GameObjects.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System;
using System.Collections.Generic;

namespace Dungeoneer.GameObjects.Bases;

/// <summary>
/// Gemensam bas för alla levande/rörliga aktörer på kartan.
/// Hanterar position, tile-move interpolation och animation/draw.
/// </summary>
public abstract class ActorBase
{
    public int EntityId { get; }
    public abstract string ActorName { get; protected set; }
    public AnimatedSprite IdleSprite { get; set; }
    public AnimatedSprite MoveSprite { get; set; }
    public AnimatedSprite ActiveSprite { get; set; }
    public AnimatedSprite AttackSprite { get; set; }
    public virtual float CombatScale { get; set; } = 1f;

    public Vector2 Position { get; protected set; }     // Aktuell tile-position i world space
    public Vector2 Direction { get; protected set; }    // Senast valda riktning
    protected int Heading { get; set; } = 1;            // 1 right, -1 left

    // Interpolation state (för mjuk förflyttning mellan tiles)
    protected Vector2 From { get; set; }
    protected Vector2 To { get; set; }
    protected float MoveProgress01 { get; set; }
    protected TimeSpan MoveAnimRemaining { get; set; }
    protected TimeSpan MoveAnimDuration { get; set; }
    protected TimeSpan AttackAnimRemaining { get; set; }
    protected TimeSpan AttackAnimDuration { get; set; }
    public bool IsMoving => MoveAnimRemaining > TimeSpan.Zero;
    public bool IsAttacking => AttackAnimRemaining > TimeSpan.Zero;
    public Vector2 TargetPosition => To;

    private readonly Func<ActorBase, Vector2, bool> _canMoveToWorldPos;
    private readonly Func<ActorBase, Vector2, ActorBase> _getBlockingActorAtWorldPos;

    public event Action<ActorBase, ActorBase> BlockedByActor;

    private static Random rand = new Random();

    public char MapKind { get; }
    public bool InCombat { get; set; } = false;
    public bool AttackMade { get; set; }
    public virtual bool IsDamaged => HealthCurrent < HealthPool;
    public abstract int HealthPool { get; set; }
    public abstract int HealthCurrent { get; set; }
    public abstract int MinDamage { get; set; }
    public abstract int MaxDamage { get; set; }
    public abstract int Armor { get; set; }
    public virtual int SkillCD { get; set; }
    public List<string> CollectedItemKeys { get; set; } = new();
    public virtual int XPValue { get; set; }
    public virtual double CurrentScaling { get; set; }

    protected ActorBase(
        AnimatedSprite idleSprite,
        AnimatedSprite moveSprite,
        Vector2 startPosition,
        Func<ActorBase, Vector2, bool> canMoveToWorldPos,
        Func<ActorBase, Vector2, ActorBase> getBlockingActorAtWorldPos,
        int _entityId,
        char mapKind)
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
        EntityId = _entityId;
        MapKind = mapKind;
    }

    public virtual void Update(GameTime gameTime)
    {
        TickAltAnimation(gameTime);
        SelectActiveSprite();

        if (!InCombat)
        {
            // Underklass avgör NÄR/VART den vill röra sig (input, AI etc.)
            if (CanStartNewStep())
            {
                Vector2? desiredDirection = GetDesiredDirection(gameTime, rand);
                if (desiredDirection.HasValue)
                {
                    TryStartStep(desiredDirection.Value);
                }
            }

            UpdateSpriteFacing();
        }

        ActiveSprite?.Update(gameTime);
    }

    public virtual void Draw(bool _isCombat = false)
    {
        if (ActiveSprite == null)
            ActiveSprite = IdleSprite ?? AttackSprite;

        ActiveSprite?.Draw(Core.SpriteBatch, GetDrawPosition());
    }

    public void ProgressionScaling(string currentLevel)
    {
        double scalingFactor = currentLevel switch
        {
            "level1" => 1.0,
            "level2" => 1.2,
            "level3" => 1.5,
            "level4" => 1.8,
            _ => 1.0
        };

        CurrentScaling = scalingFactor;

        if (!IsDamaged)
            HealthCurrent = (int)(HealthCurrent * scalingFactor);

        HealthPool = (int)(HealthPool * scalingFactor);
        MinDamage = (int)(MinDamage * scalingFactor);
        MaxDamage = (int)(MaxDamage * scalingFactor);
        Armor = (int)(Armor * scalingFactor);
        XPValue = (int)(XPValue * scalingFactor);
    }

    public Vector2 SpriteDrawPosition => GetDrawPosition();
    public Vector2 SpriteDrawExtents =>
        ActiveSprite != null ? new Vector2(ActiveSprite.Width, ActiveSprite.Height) : Vector2.Zero;

    protected virtual Vector2 GetDrawPosition()
    {
        if (MoveAnimRemaining > TimeSpan.Zero)
            return Vector2.Lerp(From, To, MoveProgress01);

        return Position;
    }

    protected virtual bool CanStartNewStep()
        => MoveAnimRemaining <= TimeSpan.Zero;

    protected abstract Vector2? GetDesiredDirection(GameTime gameTime, Random rand);
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

    protected virtual void TickAltAnimation(GameTime gameTime)
    {
        if (AttackAnimRemaining > TimeSpan.Zero && InCombat)
        {
            AttackAnimRemaining -= gameTime.ElapsedGameTime;
            return;
        }

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
        if (InCombat && AttackMade)
        {
            if (ActiveSprite == null)
                ActiveSprite = IdleSprite;

            ActiveSprite = (AttackAnimRemaining > TimeSpan.Zero) ? AttackSprite : IdleSprite;

            if (AttackAnimRemaining <= TimeSpan.Zero)
                AttackMade = false;

            return;
        }

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

    public virtual void MoveToCombatLocation(ActorBase target, Viewport vp)
    {
        Vector2 playerPos = new Vector2(vp.Width * 0.25f, vp.Height * 0.50f);
        Vector2 monsterPos = new Vector2(vp.Width * 0.75f, vp.Height * 0.50f);

        target.MoveAnimRemaining = TimeSpan.Zero;
        target.MoveProgress01 = 1f;

        if (target.GetType() == typeof(PlayerCharacter))
        {
            target.Position = playerPos;
            target.To = playerPos;
            target.From = playerPos;
            target.Heading = 1;
            target.CombatScale = 1f;

            target.ActiveSprite.Scale = new Vector2(target.CombatScale, target.CombatScale);

            target.IdleSprite.Scale = new Vector2(target.CombatScale, target.CombatScale);

            target.AttackSprite = GameAssets.GameObjectAtlas.CreateAnimatedSprite("slime-eat-pink-animation");
            target.AttackSprite.Scale = new Vector2(target.CombatScale, target.CombatScale);
        }
        else
        {
            target.Position = monsterPos;
            target.To = monsterPos;
            target.From = monsterPos;
            target.Heading = -1;

            target.ActiveSprite.Scale = new Vector2(target.CombatScale, target.CombatScale);
            target.IdleSprite.Scale = new Vector2(target.CombatScale, target.CombatScale);
        }

        UpdateSpriteFacing();
    }

    private static Vector2 Normalize4(Vector2 d)
    {
        if (Math.Abs(d.X) > Math.Abs(d.Y))
            return new Vector2(Math.Sign(d.X), 0f);
        if (Math.Abs(d.Y) > 0f)
            return new Vector2(0f, Math.Sign(d.Y));
        return Vector2.Zero;
    }

    public virtual CombatActionResult Attack(ActorBase target, bool isTargetDefending, bool skill = false)
    {
        BeginAttackAnimation();

        int attackRoll = RollDamage();
        int damage = isTargetDefending
            ? ComputeDamageVsDefend(attackRoll, target.Armor)
            : attackRoll;

        damage = skill ? damage * 2 : damage;

        if (skill)
        {
            var result = rand.NextDouble();
            if (result >= 0.95)
                damage = 999; // Execute Target

            if (target.HealthCurrent / (double)target.HealthPool <= 0.2 && result >= 0.25)
                damage = 999; // Increased execution chance on low health targets

            if (SkillCD == 0)
                SkillCD = 3;
        }


        var defenderAction = isTargetDefending ? CombatActionType.Defend : CombatActionType.None;
        var outcome = (isTargetDefending && damage <= 0) ? CombatOutcomeKind.Blocked : CombatOutcomeKind.Hit;

        return BuildCombatResult(defenderAction, target.EntityId, outcome, damage);
    }

    private void BeginAttackAnimation()
    {
        AttackMade = true;

        var anim = AttackSprite?.Animation;
        AttackAnimRemaining = (anim != null)
            ? TimeSpan.FromTicks(anim.Delay.Ticks * anim.Frames.Count)
            : TimeSpan.FromMilliseconds(300);
    }

    private int RollDamage()
    {
        return rand.Next(MinDamage, MaxDamage + 1);
    }

    private int ComputeDamageVsDefend(int attackRoll, int targetArmor)
    {
        return (int)Math.Round((attackRoll - targetArmor) * 0.5f);
    }

    private CombatActionResult BuildCombatResult(
        CombatActionType defenderAction,
        int targetEntityId,
        CombatOutcomeKind outcome,
        int damageDealt)
    {
        return new CombatActionResult(
            CombatActionType.Attack,
            EntityId,
            defenderAction,
            targetEntityId,
            outcome,
            damageDealt
        );
    }

    public enum CombatActionType { Attack, Defend, Skill, None }
    public enum CombatOutcomeKind { Hit, Blocked, Rest }
    public sealed record CombatActionResult(
        CombatActionType AttackerAction,
        int ActorEntityId,
        CombatActionType DefenderAction,
        int TargetEntityId,
        CombatOutcomeKind Outcome,
        int DamageDealt
    );
}