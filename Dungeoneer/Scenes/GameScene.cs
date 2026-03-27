using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.HelperMethods;
using Dungeoneer.GameObjects.Player;
using Dungeoneer.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using System.Collections.Generic;

namespace Dungeoneer.Scenes;

public class GameScene : Scene
{
    private PlayerCharacter _playerCharacter;

    List<ActorBase> _actors = new();
    List<PropBase> _props = new();

    // The font to use to render normal text.
    private SpriteFont _font;

    // The font used to render the title text.
    private SpriteFont _font5x;

    private DungeonMap _dungeonMap;

    private Vector2 _cameraPos = Vector2.Zero;

    private float worldScale = 1.0f; // ex 2x

    private float CAMERA_SMOOTH_SPEED = 1.5f;

    public override void LoadContent()
    {
        // Load the font for the standard text.
        _font = Core.Content.Load<SpriteFont>("fonts/04B_30");

        // Load the font for the title text.
        _font5x = Content.Load<SpriteFont>("fonts/04B_30_5x");

        _dungeonMap = new DungeonMap(64);
        _dungeonMap.LoadContent(Content, "Images/DungeonAtlas");
        _dungeonMap.LoadMap(Content, "LevelFiles/Level1_w_Boss.txt");

        TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/GameObjectAtlas.xml");

        AnimatedSprite pcSpriteIdle = atlas.CreateAnimatedSprite("slime-idle-pink-animation");
        pcSpriteIdle.Scale = Vector2.One;
        AnimatedSprite pcSpriteMove = atlas.CreateAnimatedSprite("slime-move-pink-animation");
        pcSpriteMove.Scale = Vector2.One;

        _playerCharacter = new PlayerCharacter(
            pcSpriteIdle,
            pcSpriteMove,
            _dungeonMap.PlayerStart.X,
            _dungeonMap.PlayerStart.Y,
            CanActorMoveTo
        );

        _actors = LoadEntities.ParseActors(_dungeonMap, atlas, CanActorMoveTo);
        _props = LoadEntities.ParseProps(_dungeonMap, atlas);

    }

    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        //Core.ExitOnEscape = false;
    }

    public override void Update(GameTime gameTime)
    {
        foreach (var actor in _actors)
        {
            actor.Update(gameTime);
        }

        foreach (var prop in _props)
        {
            prop.Update(gameTime);
            if (prop.CanInteract && prop.TryInteract(_playerCharacter))
            {
                //No additional logic needed here for now
                //But this is where you would add any special behavior that should happen when the player interacts with a prop.
            }
        }

        _playerCharacter.Update(gameTime);

        _cameraPos = Camera.CameraLoc(Core.GraphicsDevice.Viewport, _playerCharacter, _dungeonMap,
                                        _cameraPos, gameTime, worldScale, CAMERA_SMOOTH_SPEED);
    }

    public override void Draw(GameTime gameTime)
    {
        Matrix cameraTransform =
            Matrix.CreateTranslation(-_cameraPos.X, -_cameraPos.Y, 0f) *
            Matrix.CreateScale(worldScale, worldScale, 1f);

        Core.SpriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: cameraTransform
        );

        _dungeonMap.Draw(Core.SpriteBatch);

        _playerCharacter.Draw();

        foreach (var actor in _actors)
        {
            actor.Draw();
        }

        foreach (var prop in _props)
        {
            prop.Draw();
        }

        Core.SpriteBatch.End();
    }

    private Point ToTile(Vector2 worldPos)
    {
        int tx = (int)(worldPos.X / _dungeonMap.TileSize);
        int ty = (int)(worldPos.Y / _dungeonMap.TileSize);
        return new Point(tx, ty);
    }
    private IEnumerable<ActorBase> EnumerateAllActors()
    {
        // Viktigt: ta med player också
        yield return _playerCharacter;
        foreach (var actor in _actors)
            yield return actor;
    }
    private bool CanActorMoveTo(ActorBase self, Vector2 candidateWorldPos)
    {
        // 1) Terräng
        if (!_dungeonMap.IsWalkable(candidateWorldPos))
            return false;
        Point candidateTile = ToTile(candidateWorldPos);
        // 2) Occupancy av andra actors
        foreach (var other in EnumerateAllActors())
        {
            if (ReferenceEquals(other, self))
                continue;
            // blockerad av någons nuvarande tile
            if (ToTile(other.Position) == candidateTile)
                return false;
            // blockerad av tile som någon redan är på väg till
            if (other.IsMoving && ToTile(other.TargetPosition) == candidateTile)
                return false;
        }
        return true;
    }
}