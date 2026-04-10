using Dungeoneer.GameObjects.GameSessions;
using Dungeoneer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using System;

namespace Dungeoneer.Scenes;

public class LevelUpScene : Scene
{
    private LevelUpHudUI _levelUpUI;
    private readonly string _level;
    private readonly GameSession _loadedSession;

    private bool _isLeveledUp = false;
    private LevelUp leveled;

    Random rand = new Random();

    public LevelUpScene(string level, GameSession session)
    {
        _loadedSession = session;
        _level = level;
    }

    public override void LoadContent()
    {

    }

    public override void Initialize()
    {
        base.Initialize();

        if (_isLeveledUp = ResolveLevelUp(_loadedSession))
        {
            ApplyLevelUp(leveled);

            GumService.Default.Root.Children.Clear();
        }
        else
        {
            GumService.Default.Root.Children.Clear();

            Core.ChangeScene(new GameScene(_level, _loadedSession));
        }

        _levelUpUI = new LevelUpHudUI(_loadedSession, leveled);
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        var vp = Core.GraphicsDevice.Viewport;
        var dst = new Rectangle(0, 0, vp.Width, vp.Height);

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        Core.SpriteBatch.End();

        _levelUpUI.Draw();
    }

    public override void Update(GameTime gameTime)
    {
        _levelUpUI.Update(gameTime);
    }

    public bool ResolveLevelUp(GameSession Session)
    {
        var levelIncrease = 0;
        var hpIncrease = 0;
        var minDmgIncrease = 0;
        var maxDmgIncrease = 0;
        var armorIncrease = 0;
        var OldXPRequirement = 0;
        var NewXPRequirement = 0;

        OldXPRequirement = Session.Player.XPToNextLevel;
        if (Session.Player.CurrentXP < OldXPRequirement)
            return false;

        Session.Player.CurrentXP -= Session.Player.XPToNextLevel;
        levelIncrease += 1;

        NewXPRequirement = CalculateXpToNextLevel(Session.Player.CurrentLevel + levelIncrease);

        hpIncrease += rand.Next(1, 5);

        if (rand.NextDouble() > 0.5)
        {
            if (rand.NextDouble() < 0.5)
                minDmgIncrease += 1;
            else
                minDmgIncrease += 2;

            if (Session.Player.MinDamage + minDmgIncrease > Session.Player.MaxDamage + maxDmgIncrease)
            {
                if (rand.NextDouble() < 0.5)
                    maxDmgIncrease += 1;
                else
                    maxDmgIncrease += 2;

                minDmgIncrease = 0;
            }
        }
        else
        {
            if (rand.NextDouble() < 0.5)
                maxDmgIncrease += 1;
            else
                maxDmgIncrease += 2;
        }

        if (rand.NextDouble() > 0.75)
        {
            if (rand.NextDouble() < 0.5)
                armorIncrease += 1;
            else
                armorIncrease += 2;
        }

        leveled = new LevelUp
        {
            LevelIncrease = levelIncrease,
            HealthPoolIncrease = hpIncrease,
            MinDamageIncrease = minDmgIncrease,
            MaxDamageIncrease = maxDmgIncrease,
            ArmorIncrease = armorIncrease,
            OldXPRequirement = OldXPRequirement,
            NewXPRequirement = NewXPRequirement
        };

        return true;
    }

    public void ApplyLevelUp(LevelUp _levelUp)
    {
        _loadedSession.Player.CurrentLevel += _levelUp.LevelIncrease;
        _loadedSession.Player.HealthMax += _levelUp.HealthPoolIncrease;
        _loadedSession.Player.HealthCurrent = _loadedSession.Player.HealthMax;
        _loadedSession.Player.MinDamage += _levelUp.MinDamageIncrease;
        _loadedSession.Player.MaxDamage += _levelUp.MaxDamageIncrease;
        _loadedSession.Player.Armor += _levelUp.ArmorIncrease;
        _loadedSession.Player.XPToNextLevel = _levelUp.NewXPRequirement;
    }

    public int CalculateXpToNextLevel(int PlayerLevel)
    {
        int XPToNextLevel = 0;

        if (PlayerLevel < 10)
            XPToNextLevel = (PlayerLevel + PlayerLevel + 1) * 5;
        if (PlayerLevel >= 10 && PlayerLevel < 20)
            XPToNextLevel = (PlayerLevel + PlayerLevel + 1) * 10;
        if (PlayerLevel >= 20 && PlayerLevel < 30)
            XPToNextLevel = (PlayerLevel + PlayerLevel + 1) * 15;
        return XPToNextLevel;
    }
}

public class LevelUp()
{
    public int LevelIncrease { get; set; }
    public int HealthPoolIncrease { get; set; }
    public int MaxDamageIncrease { get; set; }
    public int MinDamageIncrease { get; set; }
    public int ArmorIncrease { get; set; }
    public int OldXPRequirement { get; set; }
    public int NewXPRequirement { get; set; }
}