using Gum.DataTypes;
using Gum.DataTypes.Variables;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.Graphics.Animation;
using Gum.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameGum.GueDeriving;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.UI;

/// <summary>
/// A custom button implementation that inherits from Gum's Button class to provide
/// animated visual feedback when focused.
/// </summary>
public class AnimatedButton : Button
{
    private readonly TextRuntime _textRuntime;

    /// <summary>
    /// Creates a new AnimatedButton instance using graphics from the specified texture atlas.
    /// </summary>
    /// <param name="atlas">The texture atlas containing button graphics and animations</param>
    public AnimatedButton(TextureAtlas atlas)
    {
        ButtonVisual buttonVisual = (ButtonVisual)Visual;
        buttonVisual.Height = 56;
        buttonVisual.HeightUnits = DimensionUnitType.RelativeToChildren;
        buttonVisual.Width = 100f;
        buttonVisual.WidthUnits = DimensionUnitType.RelativeToChildren;

        NineSliceRuntime background = buttonVisual.Background;
        background.Texture = atlas.Texture;
        background.TextureAddress = TextureAddress.Custom;
        background.Color = Microsoft.Xna.Framework.Color.White;
        background.Animate = false;

        _textRuntime = buttonVisual.TextInstance;
        _textRuntime.UseCustomFont = true;
        _textRuntime.CustomFontFile = "fonts/04b_30.fnt";
        _textRuntime.FontScale = 0.5f; // Gum zoom = 1.0f
        _textRuntime.Color = Color.Blue;
        _textRuntime.Anchor(Gum.Wireframe.Anchor.Center);
        _textRuntime.WidthUnits = DimensionUnitType.RelativeToChildren;
        _textRuntime.Text = Text ?? string.Empty;
        _textRuntime.VerticalAlignment = RenderingLibrary.Graphics.VerticalAlignment.Bottom;

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Text))
            {
                _textRuntime.Text = Text ?? string.Empty;
            }
        };

        TextureRegion unfocusedTextureRegion = atlas.GetRegion("unfocused-button");

        AnimationChain unfocusedAnimation = new AnimationChain();
        unfocusedAnimation.Name = nameof(unfocusedAnimation);
        AnimationFrame unfocusedFrame = new AnimationFrame
        {
            TopCoordinate = unfocusedTextureRegion.TopTextureCoordinate,
            BottomCoordinate = unfocusedTextureRegion.BottomTextureCoordinate,
            LeftCoordinate = unfocusedTextureRegion.LeftTextureCoordinate,
            RightCoordinate = unfocusedTextureRegion.RightTextureCoordinate,
            FrameLength = 0.3f,
            Texture = unfocusedTextureRegion.Texture
        };
        unfocusedAnimation.Add(unfocusedFrame);

        Animation focusedAtlasAnimation = atlas.GetAnimation("focused-button-animation");

        AnimationChain focusedAnimation = new AnimationChain();
        focusedAnimation.Name = nameof(focusedAnimation);
        foreach (TextureRegion region in focusedAtlasAnimation.Frames)
        {
            AnimationFrame frame = new AnimationFrame
            {
                TopCoordinate = region.TopTextureCoordinate,
                BottomCoordinate = region.BottomTextureCoordinate,
                LeftCoordinate = region.LeftTextureCoordinate,
                RightCoordinate = region.RightTextureCoordinate,
                FrameLength = (float)focusedAtlasAnimation.Delay.TotalSeconds,
                Texture = region.Texture
            };

            focusedAnimation.Add(frame);
        }

        background.AnimationChains = new AnimationChainList
        {
            unfocusedAnimation,
            focusedAnimation
        };

        buttonVisual.ButtonCategory.ResetAllStates();

        StateSave enabledState = buttonVisual.States.Enabled;
        enabledState.Apply = () =>
        {
            background.CurrentChainName = unfocusedAnimation.Name;
            background.Animate = false;
        };

        StateSave disabledState = buttonVisual.States.Disabled;
        disabledState.Apply = () =>
        {
            background.States.Clear();
            background.CurrentChainName = unfocusedAnimation.Name;
            background.Animate = false;
        };

        StateSave focusedState = buttonVisual.States.Focused;
        focusedState.Apply = () =>
        {
            background.CurrentChainName = focusedAnimation.Name;
            background.Animate = true;
        };

        StateSave highlightedFocused = buttonVisual.States.HighlightedFocused;
        highlightedFocused.Apply = focusedState.Apply;

        StateSave highlighted = buttonVisual.States.Highlighted;
        highlighted.Apply = enabledState.Apply;

        KeyDown += HandleKeyDown;

    }

    /// <summary>
    /// Handles keyboard input for navigation between buttons using left/right keys.
    /// </summary>
    private void HandleKeyDown(object sender, KeyEventArgs e)
    {
        if (!IsEnabled) return;

        if (e.Key == Keys.Left)
        {
            // Left arrow navigates to previous control
            HandleTab(TabDirection.Up, loop: true);
        }
        if (e.Key == Keys.Right)
        {
            // Right arrow navigates to next control
            HandleTab(TabDirection.Down, loop: true);
        }
    }
}