using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.GameSessions;
using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.Maps;
using Dungeoneer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
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

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.20f;

        if (MediaPlayer.State == MediaState.Playing)
        {
            MediaPlayer.Stop();
        }

        if (_encounter.Monster.ActorName == "Boss Monster")
        {
            MediaPlayer.Play(GameAssets.BossCombatBGM);
        }
        else
        {
            MediaPlayer.Play(GameAssets.RegularCombatBGM);
        }

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

            _encounter.Session.Player.SkillCooldowns = _encounter.Player.SkillCooldowns ?? new();

            _encounter.Session.ApplyCombatOutcome(outcome);

            Core.ChangeScene(new GameScene(_encounter.Session.Level, _encounter.Session));
        }

        if (_hudUI.Skill && _hudUI.SelectedSkillId is int skillId)
        {
            if (IsSkillOnCooldown(skillId))
            {
                // gör inget — fall through till Sync + reset
            }
            else if (skillId == 0)
            {
                GameAssets.DefendSFX.Play();
                ResolvePlayerDefend();
                SetSkillCooldown(skillId, 0);
            }
            else if (skillId == 1)
            {
                GameAssets.BiteConsumeSFX.Play();
                ResolvePlayerBite();
                SetSkillCooldown(skillId, 3);
            }
            else if (skillId == 2)
            {
                GameAssets.BiteConsumeSFX.Play();
                ResolvePlayerConsume();
                SetSkillCooldown(skillId, 3);
            }

            TickSkillCooldowns();
        }

        if (_hudUI.Attack == true)
        {
            actionRoll = Rand.NextDouble();

            GameAssets.AttackSFX.Play();

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

            TickSkillCooldowns();
        }

        if (_hudUI.EndCombat)
            EndCombat();

        _encounter.Player.Update(gameTime);
        _encounter.Monster.Update(gameTime);

        _hudUI.Sync(Core.GraphicsDevice.Viewport, _encounter, CombatResult);

        _hudUI.ResetTurnInput();

        _hudUI.IsAttackMade = _encounter.Player.IsActionLocked;
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

    private void ResolvePlayerBite()
    {
        actionRoll = Rand.NextDouble();

        if (actionRoll > 0.5)
        {
            CombatResult = _encounter.Player.Attack(_encounter.Monster, true, "Bite");

            GetCombatOutcome(_encounter.Monster, CombatResult);

            _hudUI.PrintCombatLog(CombatResult, _encounter);
        }
        else
        {
            CombatResult = _encounter.Player.Attack(_encounter.Monster, false, "Bite");
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

    private void ResolvePlayerConsume()
    {
        actionRoll = Rand.NextDouble();

        if (actionRoll > 0.5)
        {
            CombatResult = _encounter.Player.Attack(_encounter.Monster, true, "Consume");
            GetCombatOutcome(_encounter.Monster, CombatResult);
            _hudUI.PrintCombatLog(CombatResult, _encounter);
        }
        else
        {
            CombatResult = _encounter.Player.Attack(_encounter.Monster, false, "Consume");
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

    private void ResolvePlayerDefend()
    {
        _encounter.Player.BeginDefendAction();

        var defendResult = new CombatActionResult(
            CombatActionType.Defend,
            _encounter.Player.EntityId,
            CombatActionType.None,
            _encounter.Monster.EntityId,
            CombatOutcomeKind.Rest,
            0);

        _hudUI.PrintCombatLog(defendResult, _encounter);

        actionRoll = Rand.NextDouble();
        if (actionRoll <= 0.5)
            return;

        CombatResult = _encounter.Monster.Attack(_encounter.Player, isTargetDefending: true);
        GetCombatOutcome(_encounter.Player, CombatResult);
        _hudUI.PrintCombatLog(CombatResult, _encounter);
    }

    private bool IsSkillOnCooldown(int skillId)
    {
        var cds = _encounter.Player.SkillCooldowns;
        return cds != null && cds.TryGetValue(skillId, out int cd) && cd > 0;
    }

    private void SetSkillCooldown(int skillId, int turns)
    {
        _encounter.Player.SkillCooldowns ??= new Dictionary<int, int>();
        _encounter.Player.SkillCooldowns[skillId] = turns;
    }

    private void TickSkillCooldowns()
    {
        if (_encounter.Player.SkillCooldowns == null) return;
        foreach (var key in _encounter.Player.SkillCooldowns.Keys.ToList())
            if (_encounter.Player.SkillCooldowns[key] > 0)
                _encounter.Player.SkillCooldowns[key]--;
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