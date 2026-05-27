using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.GameSessions;
using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.Maps;
using Dungeoneer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using System;
using static Dungeoneer.GameObjects.Bases.ActorBase;

namespace Dungeoneer.Scenes;

public class CombatScene : Scene
{
    private readonly CombatEncounter _encounter;

    private CombatHudUI _hudUI;

    private DungeonMap _combatMap;

    private CombatOutcome outcome;

    Random Rand = new();

    double actionRoll;
    CombatActionResult CombatResult { get; set; }

    private string PreviousLevel { get; set; }

    public CombatScene(CombatEncounter encounter)
    {
        _encounter = encounter;
        PreviousLevel = _encounter.Session.Level;
    }

    public override void Initialize()
    {
        base.Initialize();

        GumService.Default.Root.Children.Clear();

        _hudUI = new CombatHudUI(_encounter.Player);

        Core.ExitOnEscape = false;
    }
    public override void LoadContent()
    {
        _combatMap = new DungeonMap(64);
        _combatMap.LoadContent(Content, "Images/DungeonAtlas");
        _combatMap.LoadMap(Core.Content, "CombatScene");

        Viewport vp = Core.GraphicsDevice.Viewport;

        _encounter.Player.InCombat = true;
        _encounter.Monster.InCombat = true;

        _encounter.Player.MoveToCombatLocation(_encounter.Player, vp);
        _encounter.Monster.MoveToCombatLocation(_encounter.Monster, vp);
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.Black);

        Core.SpriteBatch.Begin(
            samplerState: SamplerState.PointClamp
        );

        _combatMap.Draw(Core.SpriteBatch);

        _encounter.Player.Draw(true);
        _encounter.Monster.Draw(true);

        Core.SpriteBatch.End();

        _hudUI.Draw();
    }

    public override void Update(GameTime gameTime)
    {
        if (_hudUI.Flee == true)
        {
            CombatOutcome outcome = new()
            {
                PlayerHealthAfter = _encounter.Player.HealthCurrent,
                MonsterEntityId = _encounter.Monster.EntityId,
                MonsterHealthAfter = _encounter.Monster.HealthCurrent,
                MonsterDefeated = false
            };

            if (_encounter.Player.SkillCD > 0)
                _encounter.Session.Player.skillCD = _encounter.Player.SkillCD;

            _encounter.Session.ApplyCombatOutcome(outcome);

            Core.ChangeScene(new GameScene(_encounter.Session.Level, _encounter.Session));
        }

        if (_hudUI.Skill == true)
        {
            if (_encounter.Player.SkillCD > 0)
                return;

            actionRoll = Rand.NextDouble();

            if (actionRoll > 0.5)
            {
                CombatResult = _encounter.Player.Attack(_encounter.Monster, true, true);

                GetCombatOutcome(_encounter.Monster, CombatResult);

                _hudUI.PrintCombatLog(CombatResult, _encounter);
            }
            else
            {
                CombatResult = _encounter.Player.Attack(_encounter.Monster, false, true);
                GetCombatOutcome(_encounter.Monster, CombatResult);

                _hudUI.PrintCombatLog(CombatResult, _encounter);

                if (_encounter.Monster.HealthCurrent > 0)
                {
                    CombatResult = _encounter.Monster.Attack(_encounter.Player, false);

                    GetCombatOutcome(_encounter.Player, CombatResult);

                    _hudUI.PrintCombatLog(CombatResult, _encounter);
                }
            }
        }

        if (_hudUI.Attack == true)
        {
            actionRoll = Rand.NextDouble();

            if (actionRoll > 0.5)
            {
                CombatResult = _encounter.Player.Attack(_encounter.Monster, true);

                GetCombatOutcome(_encounter.Monster, CombatResult);

                _hudUI.PrintCombatLog(CombatResult, _encounter);
            }
            else
            {
                CombatResult = _encounter.Player.Attack(_encounter.Monster, false);
                GetCombatOutcome(_encounter.Monster, CombatResult);

                _hudUI.PrintCombatLog(CombatResult, _encounter);

                if (_encounter.Monster.HealthCurrent > 0)
                {
                    CombatResult = _encounter.Monster.Attack(_encounter.Player, false);

                    GetCombatOutcome(_encounter.Player, CombatResult);

                    _hudUI.PrintCombatLog(CombatResult, _encounter);
                }
            }


            if (_encounter.Player.SkillCD > 0)
                _encounter.Player.SkillCD--;
        }

        if (_hudUI.EndCombat)
            EndCombat();

        _encounter.Player.Update(gameTime);
        _encounter.Monster.Update(gameTime);

        _hudUI.Sync(Core.GraphicsDevice.Viewport, _encounter, CombatResult);

        CombatResult = null;
        _hudUI.Defend = false;
        _hudUI.Attack = false;
        _hudUI.Skill = false;

        _hudUI.IsAttackMade = _encounter.Player.IsAttacking;
        _hudUI.Update(gameTime);
    }

    private void GetCombatOutcome(ActorBase ActorToCheck, CombatActionResult CombatResult)
    {
        if (CombatResult.DamageDealt > 0)
            ActorToCheck.HealthCurrent -= CombatResult.DamageDealt;

        if (ActorToCheck.HealthCurrent > 0)
            return;

        if (ActorToCheck.ActorName == _encounter.Player.ActorName)
        {
            Core.ChangeScene(new GameOverScene());
        }
    }

    private void EndCombat()
    {
        outcome = new CombatOutcome()
        {
            PlayerHealthAfter = _encounter.Player.HealthCurrent,
            MonsterEntityId = _encounter.Monster.EntityId,
            MonsterDefeated = true,
            XPGained = _encounter.Monster.XPValue,
        };

        _encounter.Session.ApplyCombatOutcome(outcome);

        Core.ChangeScene(new LevelUpScene(PreviousLevel, _encounter.Session));
    }
}