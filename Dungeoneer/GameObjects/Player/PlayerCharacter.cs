using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.GameSessions;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;
using System;
using System.Collections.Generic;

namespace Dungeoneer.GameObjects.Player;

public class PlayerCharacter : ActorBase
{
    public override string ActorName { get; protected set; } = "Player";
    public override int HealthPool { get; set; } = 20;
    public override int HealthCurrent { get; set; } = 20;
    public override int MinDamage { get; set; } = 2;
    public override int MaxDamage { get; set; } = 6;
    public override int Armor { get; set; } = 1;
    public int CurrentLevel { get; set; } = 1;
    public int CurrentXP { get; set; } = 0;
    public int XPToNextLevel { get; set; }

    private readonly Queue<Vector2> _inputBuffer = new(0);

    public PlayerCharacter(
        AnimatedSprite spriteIdle,
        AnimatedSprite spriteMove,
        float xPos,
        float yPos,
        Func<ActorBase, Vector2, bool> canMoveToWorldPos,
        Func<ActorBase, Vector2, ActorBase> getBlockingActorAtWorldPos,
        int _entityId,
        char mapKind)
        : base(spriteIdle, spriteMove, new Vector2(xPos, yPos), canMoveToWorldPos, getBlockingActorAtWorldPos, _entityId, '@')
    {
    }

    public override void Update(GameTime gameTime)
    {
        if (!InCombat)
            HandleInput();

        base.Update(gameTime);
    }

    protected override Vector2? GetDesiredDirection(GameTime gameTime)
    {
        if (InCombat)
            return null;

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

    public void PlayerCombatUpdate(GameSession session)
    {
        HealthCurrent = session.Player.HealthCurrent;
        ApplyEquipmentBonuses(session.Player.CollectedEquipment);
        CurrentLevel = session.Player.CurrentLevel;
        CurrentXP = session.Player.CurrentXP;
    }

    public void CalculateXpToNextLevel()
    {
        if (CurrentLevel < 10)
            XPToNextLevel = (CurrentLevel + CurrentLevel + 1) * 5;
        if (CurrentLevel >= 10 && CurrentLevel < 20)
            XPToNextLevel = (CurrentLevel + CurrentLevel + 1) * 10;
        if (CurrentLevel >= 20 && CurrentLevel < 30)
            XPToNextLevel = (CurrentLevel + CurrentLevel + 1) * 15;
    }

    private void ApplyEquipmentBonuses(List<PropBase> collectedEquipment)
    {
        foreach (var eq in collectedEquipment)
        {
            switch (eq.PropName)
            {
                case "Weapon-T1":
                    MinDamage += eq.DamageBoostValue;
                    MaxDamage += eq.DamageBoostValue;
                    break;
                case "Armor-T1":
                    Armor += eq.ArmorBoosValue;
                    break;
            }
        }

        CollectedEquipment = collectedEquipment;
    }
}
