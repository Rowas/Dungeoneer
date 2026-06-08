using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.GameObjects.Player;
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
using System.Collections.Generic;
using System.Linq;
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
    private ContainerRuntime _skillsColumn;

    private AnimatedButton _attackButton;
    private AnimatedButton _skillButton;
    private AnimatedButton _fleeButton;
    private AnimatedButton _endCombatButton;
    private AnimatedButton _backButton;

    private List<AnimatedButton> _skillButtonsList = new();
    private Dictionary<int, int> _skillCooldowns = new();  // skillId -> turns left

    private TextRuntime _combatEndedText;
    //private TextRuntime _skillCooldownLabel;
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

    private const float SkillRowStartY = 20f;   // första raden
    private const float SkillRowStepY = 25f;   // samma som Attack(25), Skills(50), Flee(75)

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
    public int? SelectedSkillId { get; internal set; }


    public CombatHudUI(PlayerCharacter player)
    {
        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        InitializeHpDisplay();

        InitializeCombatLog();

        InitializeCombatCommands();

        InitializeCombatSkills(player);

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

        if (_endOfCombatButtonColumn.Visible)
        {
            _skillsColumn.Visible = false;
            _skillsColumn.IsEnabled = false;
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

            UpdateSkillCooldownUi(encounter.Player.SkillCooldowns);
        }

        UpdateCombatInfoText();

        _minDmg = encounter.Player.MinDamage;
        _maxDmg = encounter.Player.MaxDamage;

        _playerHP.Text = string.Format(s_hpFormat,
            encounter.Player.HealthCurrent, encounter.Player.HealthPool);
        _monsterHP.Text = string.Format(s_hpFormat,
            encounter.Monster.HealthCurrent, encounter.Monster.HealthPool);
        LayoutHpAboveActor(viewport, _playerHpStack, encounter.Player);
        LayoutHpAboveActor(viewport, _monsterHpStack, encounter.Monster);

        UpdateSkillCooldownUi(encounter.Player.SkillCooldowns);
    }


    private void UpdateCombatInfoText()
    {
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
        else if (_skillsColumn.Visible == true)
        {
            foreach (var row in _skillRows)
            {
                if (row.Button.IsFocused == true)
                {
                    _commandInfoText.Text = GetCommandInfo(row.Button.Text);
                    break;
                }
            }
            if (_backButton.IsFocused == true)
            {
                _commandInfoText.Text = GetCommandInfo(_backButton.Text);
            }
        }
        else if (_endCombatButton.IsFocused == true)
        {
            _commandInfoText.Text = GetCommandInfo(_endCombatButton.Text);
        }
        else
        {
            _commandInfoText.Text = " ";
        }
    }

    private void UpdateSkillCooldownUi(Dictionary<int, int> cooldowns)
    {
        if (_endOfCombatButtonColumn.Visible)
            return;

        cooldowns ??= new();
        foreach (var row in _skillRows)
        {
            int cd = cooldowns.TryGetValue(row.SkillId, out int v) ? v : 0;

            if (row.CooldownLabel == null)
                continue; // Defend m.m. — ingen CD-UI

            if (cd > 0)
            {
                bool wasFocused = row.Button.IsFocused;

                row.Button.IsEnabled = false;
                row.Button.IsVisible = false;
                row.Button.IsFocused = false;

                row.CooldownLabel.Visible = true;
                row.CooldownLabel.Text = $"CD: {cd}";  // eller "Bite! (CD: 2)"

                if (wasFocused && _skillsColumn.Visible)
                    FocusFirstUsableSkillMenuItem();
            }
            else
            {
                row.Button.IsEnabled = true;
                row.Button.IsVisible = true;

                row.CooldownLabel.Visible = false;
                row.CooldownLabel.Text = " ";
            }
        }

        if (_skillsColumn.Visible && _skillsColumn.IsEnabled)
        {
            bool anyFocused =
                _skillRows.Any(r => r.Button.IsFocused) ||
                _backButton.IsFocused;
            if (!anyFocused)
                FocusFirstUsableSkillMenuItem();
        }
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
        PushCombatLogLine(combatResult.ToLogLine(encounter), encounter);
    }
    public void PushCombatLogLine(string line, CombatEncounter encounter)
    {
        if (_combatLog2.Text != "")
            _combatLog2.Text = _combatLog1.Text;
        _combatLog1.Text = line;
        if (encounter.Monster.HealthCurrent <= 0)
        {
            _combatLog1.Text += $" {encounter.Monster.ActorName} was defeated!";
            ShowEndCombatUi();
        }
    }

    private void ShowEndCombatUi()
    {
        _combatButtonColumn.Visible = false;
        _combatButtonColumn.IsEnabled = false;

        _skillsColumn.Visible = false;
        _skillsColumn.IsEnabled = false;

        foreach (var row in _skillRows)
            row.Button.IsFocused = false;
        _backButton.IsFocused = false;

        _endOfCombatButtonColumn.Visible = true;
        _endOfCombatButtonColumn.IsEnabled = true;
        _endCombatButton.IsFocused = true;
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
        _skillButton.Text = "Use Skills!";
        _skillButton.Click += HandleSkillWindow;
        _skillButton.Anchor(Gum.Wireframe.Anchor.Center);
        _skillButton.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _skillButton.Y = 50;

        _combatButtonColumn.AddChild(_skillButton);

        _fleeButton = new AnimatedButton(GameAssets.GameObjectAtlas);
        _fleeButton.Text = "Flee!";
        _fleeButton.Click += HandleFlee;
        _fleeButton.Anchor(Gum.Wireframe.Anchor.Center);
        _fleeButton.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _fleeButton.Y = 75;

        _combatButtonColumn.AddChild(_fleeButton);
    }

    private void InitializeCombatSkills(PlayerCharacter player)
    {
        _skillsColumn = CreateColumn();
        _skillsColumn.Visible = false;
        _skillsColumn.IsEnabled = false;
        _combatCommandsPanel.AddChild(_skillsColumn);

        for (int i = 0; i < player.Skills.Count; i++)
        {
            var (text, skillId) = player.Skills[i];
            var btn = new AnimatedButton(GameAssets.GameObjectAtlas);
            btn.Anchor(Gum.Wireframe.Anchor.Center);
            btn.YUnits = Gum.Converters.GeneralUnitType.Percentage;
            btn.Y = SkillRowStartY + i * SkillRowStepY;
            btn.Name = skillId.ToString();
            btn.Text = text;  // alltid "Bite!" — ändra ALDRIG för CD
            btn.Click += HandleSkillUse;
            _skillsColumn.AddChild(btn);
            TextRuntime cdLabel = null;
            if (HasCooldown(skillId))  // se nedan
            {
                cdLabel = InitializeCooldownLabel(btn);
                _skillsColumn.AddChild(cdLabel);
            }
            _skillRows.Add(new SkillRowUi { SkillId = skillId, Button = btn, CooldownLabel = cdLabel });
        }

        _backButton = new AnimatedButton(GameAssets.GameObjectAtlas);
        _backButton.Text = "Back";
        _backButton.Click += HandleSkillWindow;
        _backButton.Anchor(Gum.Wireframe.Anchor.Center);
        _backButton.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _backButton.Y = SkillRowStartY + player.Skills.Count * SkillRowStepY;

        _skillsColumn.AddChild(_backButton);
    }

    private TextRuntime InitializeCooldownLabel(AnimatedButton skill)
    {
        var cooldownLabel = LogLine();
        cooldownLabel.Text = " ";
        cooldownLabel.Visible = false;
        cooldownLabel.Color = Color.Gray;

        cooldownLabel.Anchor(Gum.Wireframe.Anchor.Center);
        cooldownLabel.YUnits = skill.YUnits;
        cooldownLabel.Y = skill.Y;
        cooldownLabel.XUnits = skill.XUnits;
        cooldownLabel.X = skill.X;
        cooldownLabel.HorizontalAlignment = HorizontalAlignment.Center;

        return cooldownLabel;
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
            $"Deals {_minDmg} to {_maxDmg} damage. ",

            "Flee!" => "Escape from the current encounter. ",

            "End Combat" => "End the combat encounter and return to the dungeon. ",

            "Use Skills!" => "Use one of your skills. Each skill has its own effects and cooldowns. ",

            "Defend!" => "Take a defensive stance, doubling your ARM for one turn. Can be used every turn. ",

            "Bite!" => "A powerful attack that deals 200% attack damage. 5% chance to execute. " +
            "Increased to 20% when target has less than 20% HP. Has a 3 turn cooldown. ",

            "Back" => "Return to the default command window. ",

            _ => " "
        };
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

        HandleSkillWindow(sender, e);  // stäng skill-menyn om den är öppen
    }

    private void HandleSkillWindow(object sender, EventArgs e)
    {
        if (_endOfCombatButtonColumn.Visible)
            return;

        _skillsColumn.IsEnabled = !_skillsColumn.IsEnabled;
        _skillsColumn.Visible = !_skillsColumn.Visible;
        _combatButtonColumn.Visible = !_combatButtonColumn.Visible;
        _combatButtonColumn.IsEnabled = !_combatButtonColumn.IsEnabled;
        if (_backButton.IsFocused == true)
        {
            _backButton.IsFocused = false;
            _skillButton.IsFocused = true;
        }
        else
        {
            var firstUsable = _skillRows.FirstOrDefault(r => r.Button.IsVisible && r.Button.IsEnabled);
            (firstUsable?.Button ?? _backButton).IsFocused = true;
        }

        if (Attack || Skill || Defend)
        {
            var btn = (AnimatedButton)sender;

            btn.IsFocused = false;
            _attackButton.IsFocused = true;
        }
    }

    private void HandleSkillUse(object sender, EventArgs e)
    {
        if (IsAttackMade) return;

        var btn = (AnimatedButton)sender;

        SelectedSkillId = int.Parse(btn.Name);

        Skill = true;

        HandleSkillWindow(sender, e);  // stäng skill-menyn om den är öppen
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

    public void ResetTurnInput()
    {
        Attack = false;
        Skill = false;
        Defend = false;
        SelectedSkillId = null;
    }

    private sealed class SkillRowUi
    {
        public int SkillId;
        public AnimatedButton Button;
        public TextRuntime CooldownLabel;  // null om skillen saknar CD
    }

    private readonly List<SkillRowUi> _skillRows = new();

    private static bool HasCooldown(int skillId) => skillId switch
    {
        1 => true,   // Bite
        _ => false   // Defend, framtida no-CD skills
    };

    private void FocusFirstUsableSkillMenuItem()
    {
        var first = _skillRows.FirstOrDefault(r => r.Button.IsVisible && r.Button.IsEnabled);
        if (first != null)
            first.Button.IsFocused = true;
        else
            _backButton.IsFocused = true;
    }
}