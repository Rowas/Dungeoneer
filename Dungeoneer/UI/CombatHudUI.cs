using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.Helpers;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using MonoGameLibrary.Graphics;
using RenderingLibrary.Graphics;
using System;
using static Dungeoneer.GameObjects.Bases.ActorBase;

namespace Dungeoneer.UI;

public class CombatHudUI : ContainerRuntime
{
    private ContainerRuntime _combatButtonColumn;
    private ContainerRuntime _endOfCombatButtonColumn;
    private ContainerRuntime _logColumn;
    private ContainerRuntime _logStack;
    private ContainerRuntime _controlStack;
    private ContainerRuntime _playerHpStack;
    private ContainerRuntime _monsterHpStack;
    private ContainerRuntime _commandInfoStack;

    private AnimatedButton _attackButton;
    private AnimatedButton _skillButton;
    private AnimatedButton _fleeButton;
    private AnimatedButton _endCombatButton;

    private TextRuntime _combatEndedText;
    private TextRuntime _skillCooldownLabel;
    private TextRuntime _commandInfoText;

    private TextRuntime _monsterHP;
    private TextRuntime _playerHP;

    private TextRuntime _combatLog1;
    private TextRuntime _combatLog2;

    private Panel _commandInfoPanel;
    private Panel _combatCommandsPanel;
    private Panel _combatLogPanel;
    private Panel _monsterHpPanel;
    private Panel _playerHpPanel;

    private int _minDmg;
    private int _maxDmg;

    private bool _isFirstUpdate = true;

    private const float HpGapViewportFraction = 0.075f;
    private readonly Vector2 s_fallbackSpriteSize = new(64f, 64f);

    private readonly string s_hpFormat = "HP: {0:D2}/{1:D2}";

    public bool IsAttackMade { get; set; }

    public bool Flee { get; set; } = false;
    public bool Attack { get; set; } = false;
    public bool Defend { get; set; } = false;
    public bool Skill { get; set; } = false;
    public bool EndCombat { get; set; } = false;
    private int _lastSkillCd { get; set; } = -1;

    public CombatHudUI()
    {
        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        InitializeHpDisplay();

        InitializeCombatLog();

        InitializeCombatCommands();

        InitializeCombatCommandsInfo();

        InitializeEndScreen();
    }

    private Panel CreateCommandPanel(TextureAtlas atlas)
    {
        Panel panel = new Panel();
        panel.Anchor(Gum.Wireframe.Anchor.Center);
        panel.WidthUnits = DimensionUnitType.Absolute;
        panel.HeightUnits = DimensionUnitType.Absolute;
        panel.Width = GumService.Default.CanvasWidth * 0.15f;
        panel.Height = GumService.Default.CanvasHeight * 0.20f;
        panel.IsVisible = false;

        TextureRegion backgroundRegion = atlas.GetRegion("panel-background");

        NineSliceRuntime background = new NineSliceRuntime();
        background.Dock(Gum.Wireframe.Dock.Fill);
        background.Texture = backgroundRegion.Texture;
        background.TextureAddress = TextureAddress.Custom;
        background.TextureHeight = backgroundRegion.Height;
        background.TextureWidth = backgroundRegion.Width;
        background.TextureTop = backgroundRegion.SourceRectangle.Top;
        background.TextureLeft = backgroundRegion.SourceRectangle.Left;
        panel.AddChild(background);

        return panel;
    }

    private Panel CreateLogPanel(TextureAtlas atlas)
    {
        Panel panel = new Panel();
        panel.Anchor(Gum.Wireframe.Anchor.Center);
        panel.WidthUnits = DimensionUnitType.Absolute;
        panel.HeightUnits = DimensionUnitType.Absolute;
        panel.Width = GumService.Default.CanvasWidth * 0.55f;
        panel.Height = GumService.Default.CanvasHeight * 0.08f;
        panel.Y = -15f;
        panel.IsVisible = true;

        TextureRegion backgroundRegion = atlas.GetRegion("panel-background");

        NineSliceRuntime background = new NineSliceRuntime();
        background.Dock(Gum.Wireframe.Dock.Fill);
        background.Texture = backgroundRegion.Texture;
        background.TextureAddress = TextureAddress.Custom;
        background.TextureHeight = backgroundRegion.Height;
        background.TextureWidth = backgroundRegion.Width;
        background.TextureTop = backgroundRegion.SourceRectangle.Top;
        background.TextureLeft = backgroundRegion.SourceRectangle.Left;
        panel.AddChild(background);

        return panel;
    }

    private Panel CreateHpPanel(TextureAtlas atlas)
    {
        Panel panel = new Panel();
        panel.Anchor(Gum.Wireframe.Anchor.Center);
        panel.WidthUnits = DimensionUnitType.Absolute;
        panel.HeightUnits = DimensionUnitType.Absolute;
        panel.Width = GumService.Default.CanvasWidth * 0.18f;
        panel.Height = GumService.Default.CanvasHeight * 0.08f;
        panel.IsVisible = true;

        TextureRegion backgroundRegion = atlas.GetRegion("panel-background");

        NineSliceRuntime background = new NineSliceRuntime();
        background.Dock(Gum.Wireframe.Dock.Fill);
        background.Texture = backgroundRegion.Texture;
        background.TextureAddress = TextureAddress.Custom;
        background.TextureHeight = backgroundRegion.Height;
        background.TextureWidth = backgroundRegion.Width;
        background.TextureTop = backgroundRegion.SourceRectangle.Top;
        background.TextureLeft = backgroundRegion.SourceRectangle.Left;
        panel.AddChild(background);

        return panel;
    }

    private ContainerRuntime CreateColumn()
    {
        var column = new ContainerRuntime();
        column.Anchor(Gum.Wireframe.Anchor.Center);

        return column;
    }

    private TextRuntime LogLine()
    {
        var line = new TextRuntime();
        line.UseCustomFont = true;
        line.CustomFontFile = "fonts/04b_30.fnt";
        line.FontScale = 0.50f;
        line.Color = Color.White;
        line.WidthUnits = DimensionUnitType.RelativeToChildren;
        line.HorizontalAlignment = HorizontalAlignment.Center;
        line.VerticalAlignment = VerticalAlignment.Center;

        return line;
    }

    private TextRuntime CreateHpText(Color color)
    {
        var text = new TextRuntime();
        text.UseCustomFont = true;
        text.CustomFontFile = "fonts/04b_30.fnt";
        text.FontScale = 1.0f;
        text.Color = color;
        text.WidthUnits = DimensionUnitType.RelativeToChildren;
        text.HorizontalAlignment = HorizontalAlignment.Center;
        text.VerticalAlignment = VerticalAlignment.Center;
        text.Text = string.Format(s_hpFormat, 0, 0);
        text.Anchor(Gum.Wireframe.Anchor.Center);
        return text;
    }

    public void Update(GameTime gameTime)
    {
        if (IsAttackMade)
        {
            _combatCommandsPanel.IsVisible = false;
        }
        else
        {
            _combatCommandsPanel.IsVisible = true;
        }
        GumService.Default.Update(gameTime);
    }

    public void Draw()
    {
        GumService.Default.Draw();
    }

    public void Sync(Viewport viewport, CombatEncounter encounter, CombatActionResult combatResult)
    {
        if (_isFirstUpdate)
        {
            _combatLog1.Text = "You have encountered a " + encounter.Monster.ActorName + "!";
            _isFirstUpdate = false;

            UpdateSkillCooldown(encounter.Player.SkillCD);
        }

        if (_attackButton.IsFocused == true)
        {
            _commandInfoText.Text = GetCommandInfo(_attackButton.Text);
        }
        else if (_skillButton.IsFocused == true)
        {
            _commandInfoText.Text = GetCommandInfo(_skillButton.Text);
        }
        else if (_fleeButton.IsFocused == true)
        {
            _commandInfoText.Text = GetCommandInfo(_fleeButton.Text);
        }
        else
        {
            _commandInfoText.Text = " ";
        }

        _minDmg = encounter.Player.MinDamage;
        _maxDmg = encounter.Player.MaxDamage;

        _playerHP.Text = string.Format(s_hpFormat,
            encounter.Player.HealthCurrent, encounter.Player.HealthPool);
        _monsterHP.Text = string.Format(s_hpFormat,
            encounter.Monster.HealthCurrent, encounter.Monster.HealthPool);
        LayoutHpAboveActor(viewport, _playerHpStack, encounter.Player);
        LayoutHpAboveActor(viewport, _monsterHpStack, encounter.Monster);

        UpdateSkillCooldown(encounter.Player.SkillCD);
    }

    private void UpdateSkillCooldown(int cd)
    {
        bool changed = cd != _lastSkillCd;

        if (cd > 0)
        {
            if (_skillButton.IsFocused)
                _attackButton.IsFocused = true;

            _skillButton.IsFocused = false;
            _skillButton.IsEnabled = false;
            _skillButton.IsVisible = false;

            _skillCooldownLabel.Visible = true;

            if (changed)
                _skillCooldownLabel.Text = $"Skill cooldown: {cd}";
        }
        else
        {
            _skillButton.IsEnabled = true;
            _skillCooldownLabel.Visible = false;
            _skillButton.IsVisible = true;

            if (changed)
                _skillCooldownLabel.Text = " ";
        }

        _lastSkillCd = cd;
    }

    private void LayoutHpAboveActor(Viewport vp, ContainerRuntime stack, ActorBase actor)
    {
        Vector2 topLeft = actor.SpriteDrawPosition;
        Vector2 size = actor.SpriteDrawExtents;
        if (size.X <= 0f || size.Y <= 0f)
            size = s_fallbackSpriteSize;
        float centerX = actor.SpriteDrawPosition.X + (64f * actor.CombatScale) * 0.5f;
        float gapPx = vp.Height * HpGapViewportFraction;
        stack.Anchor(Gum.Wireframe.Anchor.TopLeft);
        stack.XOrigin = HorizontalAlignment.Center;
        stack.YOrigin = VerticalAlignment.Bottom;
        stack.X = centerX;
        stack.Y = topLeft.Y - gapPx;
        stack.Y = stack.Y * 1.2f;
    }

    public void PrintCombatLog(CombatActionResult combatResult, CombatEncounter encounter)
    {
        if (_combatLog2.Text != "")
            _combatLog2.Text = _combatLog1.Text;

        if (combatResult.Outcome == CombatOutcomeKind.Rest)
        {
            _combatLog1.Text = $"{GetActorName(encounter, combatResult.ActorEntityId)} and {GetActorName(encounter, combatResult.TargetEntityId)} both took defensive action.";
        }
        if (combatResult.Outcome == CombatOutcomeKind.Blocked)
        {
            _combatLog1.Text = $"{GetActorName(encounter, combatResult.ActorEntityId)} {combatResult.AttackerAction} and was blocked by {GetActorName(encounter, combatResult.TargetEntityId)}!";
        }
        else
        {
            _combatLog1.Text = $"{GetActorName(encounter, combatResult.ActorEntityId)} {combatResult.AttackerAction} and {combatResult.Outcome} {GetActorName(encounter, combatResult.TargetEntityId)} for {combatResult.DamageDealt} damage!";
            if (combatResult.DefenderAction == CombatActionType.Defend)
            {
                _combatLog1.Text += $" {GetActorName(encounter, combatResult.TargetEntityId)} was defending!";
            }
        }

        if (encounter.Monster.HealthCurrent <= 0)
        {
            _combatLog1.Text += $" {GetActorName(encounter, combatResult.TargetEntityId)} was defeated!";
            _combatButtonColumn.Visible = false;
            _combatButtonColumn.IsEnabled = false;

            _endOfCombatButtonColumn.IsEnabled = true;
            _endOfCombatButtonColumn.Visible = true;

            _endCombatButton.IsFocused = true;
        }
    }

    private void InitializeCombatLog()
    {
        _logStack = new ContainerRuntime();
        _logStack.Anchor(Gum.Wireframe.Anchor.Center);
        _logStack.Y = 30f;
        _logStack.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _logStack.WidthUnits = DimensionUnitType.RelativeToChildren;
        _logStack.HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(_logStack);

        _combatLogPanel = CreateLogPanel(GameAssets.GameObjectAtlas);
        _logStack.AddChild(_combatLogPanel);

        _logColumn = CreateColumn();
        _combatLogPanel.AddChild(_logColumn);

        _combatLog1 = LogLine();
        _combatLog1.Anchor(Gum.Wireframe.Anchor.Center);
        _combatLog1.Y = -20f;
        _logColumn.AddChild(_combatLog1);

        _combatLog2 = LogLine();
        _combatLog2.Anchor(Gum.Wireframe.Anchor.Center);
        _combatLog2.Y = 10f;
        _combatLog2.Text = " ";
        _logColumn.AddChild(_combatLog2);
    }

    private void InitializeHpDisplay()
    {
        _playerHpStack = new ContainerRuntime();
        _playerHpStack.Anchor(Gum.Wireframe.Anchor.Center);
        _playerHpStack.WidthUnits = DimensionUnitType.RelativeToChildren;
        _playerHpStack.HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(_playerHpStack);

        _monsterHpStack = new ContainerRuntime();
        _monsterHpStack.Anchor(Gum.Wireframe.Anchor.Center);
        _monsterHpStack.WidthUnits = DimensionUnitType.RelativeToChildren;
        _monsterHpStack.HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(_monsterHpStack);

        _monsterHpPanel = CreateHpPanel(GameAssets.GameObjectAtlas);
        _monsterHpStack.AddChild(_monsterHpPanel);

        _playerHpPanel = CreateHpPanel(GameAssets.GameObjectAtlas);
        _playerHpStack.AddChild(_playerHpPanel);

        _playerHP = CreateHpText(Color.Green);
        _playerHpPanel.AddChild(_playerHP);

        _monsterHP = CreateHpText(Color.Red);
        _monsterHpPanel.AddChild(_monsterHP);
    }

    private void InitializeCombatCommands()
    {
        _controlStack = new ContainerRuntime();
        _controlStack.Anchor(Gum.Wireframe.Anchor.Center);
        _controlStack.WidthUnits = DimensionUnitType.RelativeToChildren;
        _controlStack.HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(_controlStack);

        _combatButtonColumn = CreateColumn();
        _controlStack.AddChild(_combatButtonColumn);

        _combatCommandsPanel = CreateCommandPanel(GameAssets.GameObjectAtlas);
        _combatCommandsPanel.AddChild(_combatButtonColumn);

        _controlStack.AddChild(_combatCommandsPanel.Visual);

        _attackButton = new AnimatedButton(GameAssets.GameObjectAtlas);
        _attackButton.Text = "Attack!";
        _attackButton.Click += HandleAttack;
        _attackButton.Anchor(Gum.Wireframe.Anchor.Center);
        _attackButton.IsFocused = true;
        _attackButton.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _attackButton.Y = 25;

        _combatButtonColumn.AddChild(_attackButton);

        _skillButton = new AnimatedButton(GameAssets.GameObjectAtlas);
        _skillButton.Text = "Bite!";
        _skillButton.Click += HandleSkill;
        _skillButton.Anchor(Gum.Wireframe.Anchor.Center);
        _skillButton.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _skillButton.Y = 50;

        _combatButtonColumn.AddChild(_skillButton);

        _skillCooldownLabel = LogLine();
        _skillCooldownLabel.Text = " ";
        _skillCooldownLabel.Visible = false;

        _skillCooldownLabel.Anchor(Gum.Wireframe.Anchor.Center);
        _skillCooldownLabel.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _skillCooldownLabel.Y = _skillButton.Y;
        _skillCooldownLabel.XUnits = _skillButton.XUnits;
        _skillCooldownLabel.X = _skillButton.X;
        _skillCooldownLabel.HorizontalAlignment = HorizontalAlignment.Center;

        _combatButtonColumn.AddChild(_skillCooldownLabel);

        _fleeButton = new AnimatedButton(GameAssets.GameObjectAtlas);
        _fleeButton.Text = "Flee!";
        _fleeButton.Click += HandleFlee;
        _fleeButton.Anchor(Gum.Wireframe.Anchor.Center);
        _fleeButton.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _fleeButton.Y = 75;

        _combatButtonColumn.AddChild(_fleeButton);
    }

    private void InitializeCombatCommandsInfo()
    {
        _commandInfoStack = new ContainerRuntime();
        _commandInfoStack.Anchor(Gum.Wireframe.Anchor.Center);
        _commandInfoStack.Y = 80f;
        _commandInfoStack.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _commandInfoStack.WidthUnits = DimensionUnitType.RelativeToChildren;
        _commandInfoStack.HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(_commandInfoStack);

        _commandInfoPanel = CreateCommandPanel(GameAssets.GameObjectAtlas);
        _commandInfoPanel.IsVisible = true;
        _commandInfoPanel.Width = GumService.Default.CanvasWidth * 0.50f;
        _commandInfoPanel.Height = GumService.Default.CanvasHeight * 0.10f;
        _commandInfoStack.AddChild(_commandInfoPanel);

        _commandInfoText = LogLine();
        _commandInfoText.Text = " ";
        _commandInfoText.WidthUnits = DimensionUnitType.RelativeToParent;
        _commandInfoText.MaxWidth = _commandInfoPanel.Width * 0.9f;
        _commandInfoText.Anchor(Gum.Wireframe.Anchor.Center);
        _commandInfoPanel.AddChild(_commandInfoText);
    }

    private void InitializeEndScreen()
    {
        _endOfCombatButtonColumn = CreateColumn();
        _combatCommandsPanel.AddChild(_endOfCombatButtonColumn);

        _endCombatButton = new AnimatedButton(GameAssets.GameObjectAtlas);
        _endCombatButton.Text = "End Combat";
        _endCombatButton.Click += HandleEndCombat;
        _endCombatButton.Anchor(Gum.Wireframe.Anchor.Center);
        _endCombatButton.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _endCombatButton.Y = 75;

        _endOfCombatButtonColumn.AddChild(_endCombatButton);

        _combatEndedText = LogLine();
        _combatEndedText.Anchor(Gum.Wireframe.Anchor.Center);
        _combatEndedText.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _combatEndedText.Text = "Combat is Over!";
        _combatEndedText.Y = _skillButton.Y;
        _combatEndedText.XUnits = _skillButton.XUnits;
        _combatEndedText.X = _skillButton.X;
        _combatEndedText.HorizontalAlignment = HorizontalAlignment.Center;

        _endOfCombatButtonColumn.AddChild(_combatEndedText);

        _endOfCombatButtonColumn.Visible = false;
        _endOfCombatButtonColumn.IsEnabled = false;
    }

    private string GetCommandInfo(string command)
    {
        return command switch
        {
            "Attack!" => "A basic attack with no cooldown. Can be used every turn. " +
            $"Deals {_minDmg} to {_maxDmg} damage.",
            "Bite!" => "A powerful attack that deals 200% attack damage. 5% chance to execute. " +
            "Increased to 20% when target has less than 20% HP. Has a 3 turn cooldown.",
            "Flee!" => "Escape from the current encounter.",
            _ => " "
        };
    }

    private string GetActorName(CombatEncounter encounter, int entityId)
    {
        if (encounter.Player.EntityId == entityId)
            return encounter.Player.ActorName;
        else
            return encounter.Monster.ActorName;
    }

    private void HandleFlee(object sender, EventArgs e)
    {
        if (IsAttackMade)
            return;

        Flee = true;
    }


    private void HandleDefend(object sender, EventArgs e)
    {
        if (IsAttackMade)
            return;

        Defend = true;
    }

    private void HandleSkill(object sender, EventArgs e)
    {
        if (IsAttackMade)
            return;

        Skill = true;
    }

    private void HandleAttack(object sender, EventArgs e)
    {
        if (IsAttackMade)
            return;

        Attack = true;
    }

    private void HandleEndCombat(object sender, EventArgs e)
    {
        EndCombat = true;
    }
}