using Dungeoneer.GameObjects.GameSessions;
using Dungeoneer.GameObjects.Helpers;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Managers;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using MonoGameLibrary.Graphics;
using System;


namespace Dungeoneer.UI;

public class SaveLoadHudUI : ContainerRuntime
{
    private ContainerRuntime _buttonColumn;

    private AnimatedButton _loadGameButton;
    private AnimatedButton _confirmLoadButton;

    private AnimatedButton _saveGameButton;
    private AnimatedButton _confirmSaveButton;

    private AnimatedButton _backButton;

    public AnimatedButton _saveSlotButton1;
    public AnimatedButton _saveSlotButton2;
    public AnimatedButton _saveSlotButton3;
    private AnimatedButton _returnButton;

    public event EventHandler ActiveGameBack;
    public event EventHandler NoActiveGameBack;

    public event EventHandler<(string, string)> GameSelectionButton;

    private bool IsGameActive = false;
    private bool IsSavingSelected;

    private Panel _saveGamesPanel;
    private TextRuntime _panelText;

    private SaveGameService _saveLoadService = new();

    public SaveLoadHudUI(bool ActiveGame)
    {
        IsGameActive = ActiveGame;

        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        _buttonColumn = CreateButtonColumn();
        _buttonColumn.Anchor(Gum.Wireframe.Anchor.Left);
        _buttonColumn.XUnits = Gum.Converters.GeneralUnitType.Percentage;
        _buttonColumn.X = 5f;
        _buttonColumn.Y = 0f;
        AddChild(_buttonColumn);

        _loadGameButton = CreateButton("Load Game", Gum.Wireframe.Anchor.Center);
        _loadGameButton.Y = -25f;
        _loadGameButton.IsFocused = true;
        _loadGameButton.Click += (s, e) =>
        {
            _saveGameButton.IsEnabled = false;
            _loadGameButton.IsEnabled = false;
            _backButton.IsEnabled = false;

            IsSavingSelected = false;
            _panelText.Text = "Select a slot to load your game:";
            _saveGamesPanel.IsVisible = true;
            _returnButton.IsFocused = true;
        };
        _buttonColumn.AddChild(_loadGameButton);


        _saveGameButton = CreateButton("Save Game", Gum.Wireframe.Anchor.Center);
        _saveGameButton.Y = 25f;
        _saveGameButton.Click += (s, e) =>
        {
            _saveGameButton.IsEnabled = false;
            _loadGameButton.IsEnabled = false;
            _backButton.IsEnabled = false;

            IsSavingSelected = true;
            _panelText.Text = "Select a slot to save your game:";
            _saveGamesPanel.IsVisible = true;
            _returnButton.IsFocused = true;
        };
        _buttonColumn.AddChild(_saveGameButton);


        _backButton = CreateButton("Back", Gum.Wireframe.Anchor.Center);
        _backButton.Y = 75f;
        _backButton.Click += OnBackButtonClicked;

        _buttonColumn.AddChild(_backButton);

        _saveGamesPanel = CreateSaveLoadPanel(GameAssets.GameObjectAtlas);
        AddChild(_saveGamesPanel.Visual);

        if (ActiveGame == false)
            _saveGameButton.IsVisible = false;
    }

    private Panel CreateSaveLoadPanel(TextureAtlas atlas)
    {
        Panel panel = new Panel();
        panel.Anchor(Gum.Wireframe.Anchor.Center);
        panel.WidthUnits = DimensionUnitType.Absolute;
        panel.HeightUnits = DimensionUnitType.Absolute;
        panel.Width = GumService.Default.CanvasWidth * 0.50f;
        panel.Height = GumService.Default.CanvasHeight * 0.75f;
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

        _panelText = new TextRuntime();
        _panelText.UseCustomFont = true;
        _panelText.CustomFontFile = "fonts/04b_30.fnt";
        _panelText.FontScale = 1.0f;
        _panelText.Anchor(Gum.Wireframe.Anchor.Top);
        _panelText.YUnits = Gum.Converters.GeneralUnitType.Percentage;
        _panelText.Y = 5.0f;
        _panelText.Name = "Text";
        panel.AddChild(_panelText);

        for (int i = 0; i < 3; i++)
        {
            TextRuntime saveSlotText = new TextRuntime();
            var saveDataExists = _saveLoadService.SaveGameExists($"slot{i + 1}");
            if (saveDataExists)
            {
                var saveData = _saveLoadService.LoadGame($"slot{i + 1}");
                saveSlotText.Text = $"Slot {i + 1} - Floor: {saveData.Level}, Current Level: {saveData.Player.CurrentLevel}";
            }
            else
            {
                saveSlotText.Text = $"Slot {i + 1}: Empty";
            }

            saveSlotText.UseCustomFont = true;
            saveSlotText.CustomFontFile = "fonts/04b_30.fnt";
            saveSlotText.FontScale = 0.75f;
            saveSlotText.Anchor(Gum.Wireframe.Anchor.Center);
            saveSlotText.YUnits = Gum.Converters.GeneralUnitType.Percentage;
            saveSlotText.Y = 25.0f + (i * 20.0f);

            switch (i)
            {
                case 0:
                    _saveSlotButton1 = CreateButton("Select", Gum.Wireframe.Anchor.Center);
                    _saveSlotButton1.Y = 25.0f + (i * 20.0f);
                    _saveSlotButton1.Click += (sender, args) => OnGameSelectionButtonClicked("slot1", sender, args);
                    saveSlotText.AddChild(_saveSlotButton1);
                    break;
                case 1:
                    _saveSlotButton2 = CreateButton("Select", Gum.Wireframe.Anchor.Center);
                    _saveSlotButton2.Y = 25.0f + (i * 20.0f);
                    _saveSlotButton2.Click += (sender, args) => OnGameSelectionButtonClicked("slot2", sender, args);
                    saveSlotText.AddChild(_saveSlotButton2);
                    break;
                case 2:
                    _saveSlotButton3 = CreateButton("Select", Gum.Wireframe.Anchor.Center);
                    _saveSlotButton3.Y = 25.0f + (i * 20.0f);
                    _saveSlotButton3.Click += (sender, args) => OnGameSelectionButtonClicked("slot3", sender, args);
                    saveSlotText.AddChild(_saveSlotButton3);
                    break;
            }

            panel.AddChild(saveSlotText);
        }

        _returnButton = CreateButton("Return", Gum.Wireframe.Anchor.Bottom);
        _returnButton.Click += OnReturnButtonClicked;

        panel.AddChild(_returnButton);

        return panel;
    }

    public void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
    }

    public void Draw()
    {
        GumService.Default.Draw();
    }

    private ContainerRuntime CreateButtonColumn()
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

    private void OnGameSelectionButtonClicked(string e, object sender, EventArgs args)
    {
        if (IsSavingSelected)
        {
            GameSelectionButton?.Invoke(this, (e, "save"));
            OnReturnButtonClicked(sender, args);
        }
        else
        {
            GameSelectionButton?.Invoke(this, (e, "load"));
        }
    }

    private void OnBackButtonClicked(object sender, EventArgs args)
    {

        if (IsGameActive == true)
        {
            ActiveGameBack(sender, args);
        }
        else
        {
            NoActiveGameBack(sender, args);
        }
    }

    private void OnReturnButtonClicked(object sender, EventArgs args)
    {
        _saveGameButton.IsEnabled = true;
        _loadGameButton.IsEnabled = true;
        _backButton.IsEnabled = true;

        _saveGamesPanel.IsVisible = false;

        _loadGameButton.IsFocused = true;
    }
}