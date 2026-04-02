using Dungeoneer.GameObjects.Helpers;
using Gum.DataTypes;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using System.Linq;

namespace Dungeoneer.UI;

public class GameHudUI : ContainerRuntime
{
    public const float TopBarHeightPx = 64f * 2f;

    private readonly string s_hpFormat = "HP: {0:D2}/{1:D2}";
    private readonly string s_xpFormat = "XP: {0:D2}/{1:D2}";
    private readonly string s_intentoryFormat = "Inventory:";

    private ContainerRuntime _topBar;
    private ContainerRuntime _statContainer;
    public ContainerRuntime _itemContainer;

    private TextRuntime _hpText;
    private TextRuntime _xpText;
    private TextRuntime _inventoryText;

    private SpriteRuntime _inventoryWeapon;
    private SpriteRuntime _inventoryArmor;

    public GameHudUI()
    {
        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        _topBar = CreateTopBar();
        AddChild(_topBar);

        _statContainer = StatsContainer();
        _topBar.AddChild(_statContainer);

        _itemContainer = ItemContainer();
        _topBar.AddChild(_itemContainer);

        _inventoryText = CreateInventoryText();
        _itemContainer.AddChild(_inventoryText);

        _hpText = CreateHpText();
        _statContainer.AddChild(_hpText);

        _xpText = CreateXpText();
        _statContainer.AddChild(_xpText);

        //CreateInventoryItem("tier-1-sword", "first", _itemContainer);
        //_inventoryArmor = CreateInventoryItem("tier-1-armor", "second");


        //_itemContainer.AddChild(_inventoryArmor);
    }

    private ContainerRuntime CreateTopBar()
    {
        var top = new ContainerRuntime();
        top.Dock(Gum.Wireframe.Dock.Top);
        top.HeightUnits = DimensionUnitType.Absolute;
        top.Height = TopBarHeightPx;

        return top;
    }

    private ContainerRuntime StatsContainer()
    {
        var stats = new ContainerRuntime();
        stats.Anchor(Gum.Wireframe.Anchor.TopLeft);

        return stats;
    }

    private ContainerRuntime ItemContainer()
    {
        var item = new ContainerRuntime();
        item.Anchor(Gum.Wireframe.Anchor.TopLeft);
        item.X = 400f;

        return item;
    }

    private TextRuntime CreateHpText()
    {
        var text = new TextRuntime();
        text.Anchor(Gum.Wireframe.Anchor.TopLeft);
        text.WidthUnits = DimensionUnitType.RelativeToChildren;
        text.X = 20f;
        text.Y = 10f;
        text.UseCustomFont = true;
        text.CustomFontFile = "fonts/04b_30.fnt";
        text.Color = Color.Red;
        text.FontScale = 1.20f;
        text.Text = string.Format(s_hpFormat, 0, 0);
        return text;
    }

    private TextRuntime CreateXpText()
    {
        var text = new TextRuntime();
        text.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        text.WidthUnits = DimensionUnitType.RelativeToChildren;
        text.X = 20f;
        text.Y = -30f;
        text.UseCustomFont = true;
        text.CustomFontFile = "fonts/04b_30.fnt";
        text.Color = Color.White;
        text.FontScale = 1.20f;
        text.Text = string.Format(s_xpFormat, 0, 0);
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

    public void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
    }

    public void Draw()
    {
        GumService.Default.Draw();
    }
}
