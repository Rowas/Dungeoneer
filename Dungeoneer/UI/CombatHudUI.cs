using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.Helpers;
using Gum.DataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System;
using static Dungeoneer.GameObjects.Bases.ActorBase;

namespace Dungeoneer.UI;

public class CombatHudUI : ContainerRuntime
{
    private ContainerRuntime _buttonColumn;

    private AnimatedButton _attackButton;
    private AnimatedButton _defendButton;
    private AnimatedButton _fleeButton;

    private TextRuntime _monsterHP;
    private TextRuntime _playerHP;

    private TextRuntime _combatLog1;
    private TextRuntime _combatLog2;

    private bool _isFirstUpdate = true;

    private const float HpGapViewportFraction = 0.075f;
    private readonly Vector2 s_fallbackSpriteSize = new(64f, 64f);

    private readonly string s_hpFormat = "HP: {0:D2}/{1:D2}";

    public bool Flee { get; set; } = false;
    public bool Attack { get; set; } = false;
    public bool Defend { get; set; } = false;

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

        _buttonColumn = CreateButtonColumn();
        _buttonColumn.Anchor(Gum.Wireframe.Anchor.Bottom);
        _buttonColumn.Y = 0f;
        controlsStack.AddChild(_buttonColumn);


        // Attack-Button
        _attackButton = new AnimatedButton(GameAssets.GameObjectAtlas);
        _attackButton.Text = "Attack!";
        _attackButton.Click += HandleAttack;
        _attackButton.Anchor(Gum.Wireframe.Anchor.Top);
        _attackButton.IsFocused = true;

        _buttonColumn.AddChild(_attackButton);

        // Defend-Button
        _defendButton = new AnimatedButton(GameAssets.GameObjectAtlas);
        _defendButton.Text = "Defend!";
        _defendButton.Click += HandleDefend;
        _defendButton.Anchor(Gum.Wireframe.Anchor.Center);

        _buttonColumn.AddChild(_defendButton);

        // Flee-Button
        _fleeButton = new AnimatedButton(GameAssets.GameObjectAtlas);
        _fleeButton.Text = "Flee!";
        _fleeButton.Click += HandleFlee;
        _fleeButton.Anchor(Gum.Wireframe.Anchor.Bottom);

        _buttonColumn.AddChild(_fleeButton);
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
        }

        _playerHP.Text = string.Format(s_hpFormat,
            encounter.Player.HealthCurrent, encounter.Player.HealthPool);
        _monsterHP.Text = string.Format(s_hpFormat,
            encounter.Monster.HealthCurrent, encounter.Monster.HealthPool);
        LayoutHpAboveActor(viewport, _playerHP, encounter.Player);
        LayoutHpAboveActor(viewport, _monsterHP, encounter.Monster);
    }

    private void LayoutHpAboveActor(Viewport vp, TextRuntime label, ActorBase actor)
    {
        Vector2 topLeft = actor.SpriteDrawPosition;
        Vector2 size = actor.SpriteDrawExtents;
        if (size.X <= 0f || size.Y <= 0f)
            size = s_fallbackSpriteSize;
        float centerX = topLeft.X + size.X * 0.5f;
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
        Flee = true;
    }


    private void HandleDefend(object sender, EventArgs e)
    {
        Defend = true;
    }

    private void HandleAttack(object sender, EventArgs e)
    {
        Attack = true;
    }


}