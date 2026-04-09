using Dungeoneer.GameObjects.GameSessions;
using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.UI;
using Gum.DataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using RenderingLibrary.Graphics;
using System;

namespace Dungeoneer.Scenes;

public class LevelUpScene : Scene
{
    private LevelUpHud _mainMenuHud;
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

        _mainMenuHud = new LevelUpHud(_loadedSession, leveled);
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        var vp = Core.GraphicsDevice.Viewport;
        var dst = new Rectangle(0, 0, vp.Width, vp.Height);

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        Core.SpriteBatch.End();

        _mainMenuHud.Draw();
    }

    public override void Update(GameTime gameTime)
    {
        _mainMenuHud.Update(gameTime);
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

public class LevelUpHud : ContainerRuntime
{
    private ContainerRuntime _buttonColumn;

    private AnimatedButton _continueGameButton;

    private TextRuntime _levelIncreaseLine;
    private TextRuntime _hpIncreaseLine;
    private TextRuntime _minDmgIncreaseLine;
    private TextRuntime _maxDmgIncreaseLine;
    private TextRuntime _armorIncreaseLine;
    private TextRuntime _newXPLine;

    private readonly string s_levelFormat = "Level increased to: {0:D2}";
    private readonly string s_hpFormat = "Max HP Increased by: {0:D2}";
    private readonly string s_minDmgFormat = "Min Damage Increased by: {0:D2}";
    private readonly string s_maxDmgFormat = "Max Damage Increased by: {0:D2}";
    private readonly string s_armorFormat = "Armor Increased by: {0:D2}";
    private readonly string s_xpFormat = "New required XP: {0:D2}";



    public LevelUpHud(GameSession session, LevelUp leveled)
    {
        if (leveled == null)
            return;

        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        _buttonColumn = CreateColumn();
        _buttonColumn.Anchor(Gum.Wireframe.Anchor.Bottom);
        _buttonColumn.Y = 0f;
        AddChild(_buttonColumn);

        _continueGameButton = CreateButton("Continue", Gum.Wireframe.Anchor.Center);
        _continueGameButton.Y = -25f;
        _continueGameButton.IsFocused = true;
        _continueGameButton.Click += (s, e) =>
        {
            Core.ChangeScene(new GameScene(session.Level, session));
        };
        _buttonColumn.AddChild(_continueGameButton);

        var LogStack = new ContainerRuntime();
        LogStack.Anchor(Gum.Wireframe.Anchor.Center);
        LogStack.Y = 30f;
        LogStack.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        LogStack.WidthUnits = DimensionUnitType.RelativeToChildren;
        LogStack.HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(LogStack);

        _levelIncreaseLine = CreateTextLine();
        _levelIncreaseLine.Anchor(Gum.Wireframe.Anchor.Center);
        _levelIncreaseLine.Y = -60f;
        _levelIncreaseLine.Text = string.Format(s_levelFormat, session.Player.CurrentLevel);
        LogStack.AddChild(_levelIncreaseLine);

        _hpIncreaseLine = CreateTextLine();
        _hpIncreaseLine.Anchor(Gum.Wireframe.Anchor.Center);
        _hpIncreaseLine.Y = 0f;
        _hpIncreaseLine.Text = string.Format(s_hpFormat, leveled.HealthPoolIncrease);
        LogStack.AddChild(_hpIncreaseLine);

        _minDmgIncreaseLine = CreateTextLine();
        _minDmgIncreaseLine.Anchor(Gum.Wireframe.Anchor.Center);
        _minDmgIncreaseLine.Y = 30f;
        _minDmgIncreaseLine.Text = string.Format(s_minDmgFormat, leveled.MinDamageIncrease);
        LogStack.AddChild(_minDmgIncreaseLine);

        _maxDmgIncreaseLine = CreateTextLine();
        _maxDmgIncreaseLine.Anchor(Gum.Wireframe.Anchor.Center);
        _maxDmgIncreaseLine.Y = 60f;
        _maxDmgIncreaseLine.Text = string.Format(s_maxDmgFormat, leveled.MaxDamageIncrease);
        LogStack.AddChild(_maxDmgIncreaseLine);

        _armorIncreaseLine = CreateTextLine();
        _armorIncreaseLine.Anchor(Gum.Wireframe.Anchor.Center);
        _armorIncreaseLine.Y = 90f;
        _armorIncreaseLine.Text = string.Format(s_armorFormat, leveled.ArmorIncrease);
        LogStack.AddChild(_armorIncreaseLine);

        _newXPLine = CreateTextLine();
        _newXPLine.Anchor(Gum.Wireframe.Anchor.Center);
        _newXPLine.Y = 150f;
        _newXPLine.Text = string.Format(s_xpFormat, leveled.NewXPRequirement);
        LogStack.AddChild(_newXPLine);
    }

    public void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
    }

    public void Draw()
    {
        GumService.Default.Draw();
    }

    private ContainerRuntime CreateColumn()
    {

        var column = new ContainerRuntime();
        column.Anchor(Gum.Wireframe.Anchor.Center);

        return column;
    }

    private AnimatedButton CreateButton(string buttonText, Gum.Wireframe.Anchor location)
    {
        var button = new AnimatedButton(GameAssets.GameObjectAtlas);
        button.Text = buttonText;
        button.Anchor(location);

        return button;
    }

    private TextRuntime CreateTextLine()
    {
        var line = new TextRuntime();
        line.Anchor(Gum.Wireframe.Anchor.TopLeft);
        line.WidthUnits = DimensionUnitType.RelativeToChildren;
        line.HorizontalAlignment = HorizontalAlignment.Center;
        line.VerticalAlignment = VerticalAlignment.Center;
        line.UseCustomFont = true;
        line.CustomFontFile = "fonts/04b_30.fnt";
        line.Color = Color.White;
        line.FontScale = 0.75f;

        return line;
    }
}