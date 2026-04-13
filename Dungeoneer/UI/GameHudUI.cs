using Dungeoneer.GameObjects.Helpers;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Managers;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using MonoGameLibrary.Graphics;
using System;
using System.Linq;


namespace Dungeoneer.UI;

public class GameHudUI : ContainerRuntime
{
    public const float TopBarHeightPx = 64f * 2f;

    private readonly string s_hpFormat = "HP: {0:D2}/{1:D2}";
    private readonly string s_xpFormat = "XP: {0:D2}/{1:D2}";
    private readonly string s_intentoryFormat = "Inventory:";

    private ContainerRuntime _topBar;
    private ContainerRuntime _leftStatContainer;
    private ContainerRuntime _rightStatsPanel;
    public ContainerRuntime _itemContainer;

    private Panel _pausePanel;
    private AnimatedButton _resumeButton;

    public event EventHandler ResumeButtonClick;
    public event EventHandler QuitButtonClick;

    private TextRuntime _hpText;
    private TextRuntime _xpText;
    private TextRuntime _inventoryText;
    private TextRuntime _dmgText;
    private TextRuntime _armorText;
    private TextRuntime _levelText;

    public GameHudUI()
    {
        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        _topBar = CreateTopBar();
        AddChild(_topBar);

        _leftStatContainer = StatsContainer("left");
        _topBar.AddChild(_leftStatContainer);

        _itemContainer = ItemContainer();
        _topBar.AddChild(_itemContainer);

        _inventoryText = CreateInventoryText();
        _itemContainer.AddChild(_inventoryText);

        _hpText = CreateLeftText("hp");
        _leftStatContainer.AddChild(_hpText);

        _xpText = CreateLeftText("xp");
        _leftStatContainer.AddChild(_xpText);

        _rightStatsPanel = StatsContainer("right");
        _topBar.AddChild(_rightStatsPanel);

        _levelText = CreateRightText("lvl", "LVL: 01", y: 10f);
        _dmgText = CreateRightText("dmg", "DMG: 00-00", y: 40f);
        _armorText = CreateRightText("arm", "ARM: 00", y: 70f);

        _rightStatsPanel.AddChild(_dmgText);
        _rightStatsPanel.AddChild(_armorText);
        _rightStatsPanel.AddChild(_levelText);

        _pausePanel = CreatePausePanel(GameAssets.GameObjectAtlas);
        AddChild(_pausePanel.Visual);
    }

    private ContainerRuntime CreateTopBar()
    {
        var top = new ContainerRuntime();
        top.Dock(Gum.Wireframe.Dock.Top);
        top.HeightUnits = DimensionUnitType.Absolute;
        top.Height = TopBarHeightPx;

        return top;
    }

    private ContainerRuntime StatsContainer(string location)
    {
        var stats = new ContainerRuntime();
        switch (location)
        {
            case "left":
                stats.Anchor(Gum.Wireframe.Anchor.TopLeft);
                stats.X = 20f;
                break;
            case "right":
                stats.Anchor(Gum.Wireframe.Anchor.TopRight);
                stats.X = -20f;
                break;
            default:
                stats.Anchor(Gum.Wireframe.Anchor.TopLeft);
                break;
        }

        return stats;
    }

    private ContainerRuntime ItemContainer()
    {
        var item = new ContainerRuntime();
        item.Anchor(Gum.Wireframe.Anchor.TopLeft);
        item.X = 400f;

        return item;
    }

    private TextRuntime CreateLeftText(string textField)
    {
        var text = new TextRuntime();
        text.WidthUnits = DimensionUnitType.RelativeToChildren;
        text.X = 20f;
        text.UseCustomFont = true;
        text.CustomFontFile = "fonts/04b_30.fnt";
        text.Color = Color.Red;
        text.FontScale = 1.20f;

        switch (textField)
        {
            case "hp":
                text.Anchor(Gum.Wireframe.Anchor.TopLeft);
                text.Color = Color.Red;
                text.Text = string.Format(s_hpFormat, 0, 0);
                text.Y = 10f;
                break;
            case "xp":
                text.Anchor(Gum.Wireframe.Anchor.BottomLeft);
                text.Color = Color.White;
                text.Text = string.Format(s_xpFormat, 0, 0);
                text.Y = -30f;
                break;
        }

        return text;
    }

    private TextRuntime CreateRightText(string textField, string initialText, float y)
    {
        var text = new TextRuntime();
        text.WidthUnits = DimensionUnitType.RelativeToChildren;
        text.X = -20f;
        text.UseCustomFont = true;
        text.CustomFontFile = "fonts/04b_30.fnt";
        text.Color = Color.White;
        text.FontScale = 0.75f;

        switch (textField)
        {
            case "dmg":
                text.Anchor(Gum.Wireframe.Anchor.TopRight);
                text.Text = initialText;
                text.Y = y;
                break;
            case "arm":
                text.Anchor(Gum.Wireframe.Anchor.TopRight);
                text.Text = initialText;
                text.Y = y;
                break;
            case "lvl":
                text.Anchor(Gum.Wireframe.Anchor.TopRight);
                text.Text = initialText;
                text.Y = y;
                break;
        }

        return text;
    }

    private TextRuntime CreateInventoryText()
    {
        var text = new TextRuntime();
        text.Anchor(Gum.Wireframe.Anchor.TopLeft);
        text.WidthUnits = DimensionUnitType.RelativeToChildren;
        text.X = 20f;
        text.Y = 10f;
        text.UseCustomFont = true;
        text.CustomFontFile = "fonts/04b_30.fnt";
        text.Color = Color.White;
        text.FontScale = 1.20f;
        text.Text = string.Format(s_intentoryFormat);
        return text;
    }

    public void CreateInventoryItem(string regionName, ContainerRuntime container)
    {
        if (container.Children.Any(c => c.Name == regionName))
            return;

        var region = GameAssets.GameObjectAtlas.GetRegion($"{regionName}");

        var icon = new SpriteRuntime();
        icon.Texture = region.Texture;
        icon.TextureAddress = Gum.Managers.TextureAddress.Custom;

        // Pixel-rect (int)
        icon.TextureLeft = region.SourceRectangle.X;
        icon.TextureTop = region.SourceRectangle.Y;

        // De här två heter oftast så här i Gum när Right/Bottom saknas:
        icon.TextureWidth = region.SourceRectangle.Width;
        icon.TextureHeight = region.SourceRectangle.Height;

        // storlek på UI-elementet
        icon.WidthUnits = DimensionUnitType.Absolute;
        icon.HeightUnits = DimensionUnitType.Absolute;
        icon.Width = region.SourceRectangle.Width;
        icon.Height = region.SourceRectangle.Height;

        icon.Anchor(Gum.Wireframe.Anchor.TopLeft);

        icon.X = (container.Children.Count > 1) ? 20f : 64f;
        icon.Y = 50f;

        icon.Name = regionName;

        container.AddChild(icon);
    }

    public void SetHp(int current, int max)
    {
        _hpText.Text = string.Format(s_hpFormat, current, max);
    }

    public void SetXp(int current, int max)
    {
        _xpText.Text = string.Format(s_xpFormat, current, max);
    }

    public void SetStats(int minDamage, int maxDamage, int armor, int level)
    {
        _dmgText.Text = $"DMG: {minDamage:D2}-{maxDamage:D2}";
        _armorText.Text = $"ARM: {armor:D2}";
        _levelText.Text = $"LVL: {level:D2}";
    }

    private Panel CreatePausePanel(TextureAtlas atlas)
    {
        Panel panel = new Panel();
        panel.Anchor(Gum.Wireframe.Anchor.Center);
        panel.WidthUnits = DimensionUnitType.Absolute;
        panel.HeightUnits = DimensionUnitType.Absolute;
        panel.Width = GumService.Default.CanvasWidth * 0.25f;
        panel.Height = GumService.Default.CanvasHeight * 0.25f;
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

        TextRuntime text = new TextRuntime();
        text.Text = "PAUSED";
        text.UseCustomFont = true;
        text.CustomFontFile = "fonts/04b_30.fnt";
        text.FontScale = 1.0f;
        text.Anchor(Gum.Wireframe.Anchor.Top);
        text.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        text.Y = 5.0f;
        panel.AddChild(text);

        _resumeButton = new AnimatedButton(atlas);
        _resumeButton.Text = "RESUME";
        _resumeButton.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        _resumeButton.X = 9.0f;
        _resumeButton.Y = -9.0f;

        _resumeButton.Click += OnResumeButtonClicked;

        panel.AddChild(_resumeButton);

        AnimatedButton quitButton = new AnimatedButton(atlas);
        quitButton.Text = "QUIT";
        quitButton.Anchor(Gum.Wireframe.Anchor.BottomRight);
        quitButton.X = -9.0f;
        quitButton.Y = -9.0f;

        quitButton.Click += OnQuitButtonClicked;

        panel.AddChild(quitButton);

        return panel;
    }

    public void ShowPausePanel(string currentLevel)
    {
        _pausePanel.IsVisible = true;

        _resumeButton.IsFocused = true;
    }

    public void HidePausePanel()
    {
        _pausePanel.IsVisible = false;
    }

    private void OnResumeButtonClicked(object sender, EventArgs args)
    {
        HidePausePanel();

        if (ResumeButtonClick != null)
        {
            ResumeButtonClick(sender, args);
        }
    }

    private void OnQuitButtonClicked(object sender, EventArgs args)
    {
        HidePausePanel();

        if (QuitButtonClick != null)
        {
            QuitButtonClick(sender, args);
        }
    }

    public void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
    }

    public void Draw()
    {
        GumService.Default.Draw();
    }
}
