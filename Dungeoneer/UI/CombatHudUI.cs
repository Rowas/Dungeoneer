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

    private AnimatedButton _attackButton;
    private AnimatedButton _skillButton;
    private AnimatedButton _fleeButton;
    private AnimatedButton _endCombatButton;

    private TextRuntime _combatEndedText;
    private TextRuntime _skillCooldownLabel;
    private TextRuntime _monsterHP;
    private TextRuntime _playerHP;

    private TextRuntime _combatLog1;
    private TextRuntime _combatLog2;

    private Panel _combatCommandsPanel;

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

        _playerHP = CreateHpText(Color.Green);
        AddChild(_playerHP);

        _monsterHP = CreateHpText(Color.Red);
        AddChild(_monsterHP);

        var controlsStack = new ContainerRuntime();
        controlsStack.Anchor(Gum.Wireframe.Anchor.Center);
        controlsStack.WidthUnits = DimensionUnitType.RelativeToChildren;
        controlsStack.HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(controlsStack);

        var LogStack = new ContainerRuntime();
        LogStack.Anchor(Gum.Wireframe.Anchor.Center);
        LogStack.Y = 30f;
        LogStack.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        LogStack.WidthUnits = DimensionUnitType.RelativeToChildren;
        LogStack.HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(LogStack);

        _combatLog1 = LogLine();
        _combatLog1.Anchor(Gum.Wireframe.Anchor.Center);
        _combatLog1.Y = -30f;
        LogStack.AddChild(_combatLog1);

        _combatLog2 = LogLine();
        _combatLog2.Anchor(Gum.Wireframe.Anchor.Center);
        _combatLog2.Y = 0f;
        _combatLog2.Text = " ";
        LogStack.AddChild(_combatLog2);

        _combatButtonColumn = CreateButtonColumn();
        controlsStack.AddChild(_combatButtonColumn);

        _combatCommandsPanel = CreateCommandPanel(GameAssets.GameObjectAtlas);
        _combatCommandsPanel.AddChild(_combatButtonColumn);

        controlsStack.AddChild(_combatCommandsPanel.Visual);

        // Attack-Button
        _attackButton = new AnimatedButton(GameAssets.GameObjectAtlas);
        _attackButton.Text = "Attack!";
        _attackButton.Click += HandleAttack;
        _attackButton.Anchor(Gum.Wireframe.Anchor.Center);
        _attackButton.IsFocused = true;
        _attackButton.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _attackButton.Y = 25;

        _combatButtonColumn.AddChild(_attackButton);

        // Use-Skill-Button
        _skillButton = new AnimatedButton(GameAssets.GameObjectAtlas);
        _skillButton.Text = "Bite!";
        _skillButton.Click += HandleSkill;
        _skillButton.Anchor(Gum.Wireframe.Anchor.Center);
        _skillButton.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _skillButton.Y = 50;

        _combatButtonColumn.AddChild(_skillButton);

        _skillCooldownLabel = LogLine(); // eller en egen CreateSmallText()
        _skillCooldownLabel.Text = " ";  // tomt initialt
        _skillCooldownLabel.Visible = false;

        // Lägg den på samma Y som skill-knappen (50%) men finjustera med pixel-offset så den hamnar “på knappen”
        _skillCooldownLabel.Anchor(Gum.Wireframe.Anchor.Center);
        _skillCooldownLabel.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _skillCooldownLabel.Y = _skillButton.Y;          // 50
        _skillCooldownLabel.XUnits = _skillButton.XUnits; // om du använder XUnits
        _skillCooldownLabel.X = _skillButton.X;          // om du använder X
        _skillCooldownLabel.HorizontalAlignment = HorizontalAlignment.Center;

        // Lägg till efter knappen så den ritas ovanpå (Z-order via add-order)
        _combatButtonColumn.AddChild(_skillCooldownLabel);

        // Flee-Button
        _fleeButton = new AnimatedButton(GameAssets.GameObjectAtlas);
        _fleeButton.Text = "Flee!";
        _fleeButton.Click += HandleFlee;
        _fleeButton.Anchor(Gum.Wireframe.Anchor.Center);
        _fleeButton.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _fleeButton.Y = 75;

        _combatButtonColumn.AddChild(_fleeButton);

        _endOfCombatButtonColumn = CreateButtonColumn();
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
        _combatEndedText.Y = _skillButton.Y;          // 50
        _combatEndedText.XUnits = _skillButton.XUnits; // om du använder XUnits
        _combatEndedText.X = _skillButton.X;          // om du använder X
        _combatEndedText.HorizontalAlignment = HorizontalAlignment.Center;

        _endOfCombatButtonColumn.AddChild(_combatEndedText);

        _endOfCombatButtonColumn.Visible = false;
        _endOfCombatButtonColumn.IsEnabled = false;
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

    private ContainerRuntime CreateButtonColumn()
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

        _playerHP.Text = string.Format(s_hpFormat,
            encounter.Player.HealthCurrent, encounter.Player.HealthPool);
        _monsterHP.Text = string.Format(s_hpFormat,
            encounter.Monster.HealthCurrent, encounter.Monster.HealthPool);
        LayoutHpAboveActor(viewport, _playerHP, encounter.Player);
        LayoutHpAboveActor(viewport, _monsterHP, encounter.Monster);

        UpdateSkillCooldown(encounter.Player.SkillCD);
    }

    private void UpdateSkillCooldown(int cd)
    {
        // Uppdatera label-text endast om värdet ändras (valfritt men bra)
        bool changed = cd != _lastSkillCd;

        if (cd > 0)
        {
            // Se till att knappen inte går att välja
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

    private void LayoutHpAboveActor(Viewport vp, TextRuntime label, ActorBase actor)
    {
        Vector2 topLeft = actor.SpriteDrawPosition;
        Vector2 size = actor.SpriteDrawExtents;
        if (size.X <= 0f || size.Y <= 0f)
            size = s_fallbackSpriteSize;
        float centerX = actor.SpriteDrawPosition.X + (64f * actor.CombatScale) * 0.5f;
        float gapPx = vp.Height * HpGapViewportFraction;
        label.Anchor(Gum.Wireframe.Anchor.TopLeft);
        label.XOrigin = HorizontalAlignment.Center;
        label.YOrigin = VerticalAlignment.Bottom;
        label.X = centerX;
        label.Y = topLeft.Y - gapPx;
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