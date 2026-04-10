using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.GameSessions;
using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.GameObjects.Player;
using Dungeoneer.Maps;
using Dungeoneer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using System.Collections.Generic;
using System.Linq;

namespace Dungeoneer.Scenes;

public class GameScene : Scene
{
    private enum GameState
    {
        Playing,
        Paused,
        Combat,
        GameOver
    }

    private PlayerCharacter _playerCharacter;
    private List<ActorBase> _actors = new();
    private List<PropBase> _props = new();
    private DungeonMap _dungeonMap;
    private string _level;


    private GameHudUI _hud;

    private Vector2 _cameraPos = Vector2.Zero;
    private float CAMERA_SMOOTH_SPEED = 1.5f;
    private bool _snapCamera = true;
    const float UiTopPaddingPx = 64f * 2f;

    private float worldScale = 1.0f;

    private GameState _state;

    private double _combatRemainingMs;

    private GameSession _currentSession;
    private readonly GameSession _loadedSession;

    public GameScene(string level, GameSession session = null)
    {
        _loadedSession = session;
        _level = level;
    }

    public override void LoadContent()
    {
        _dungeonMap = new DungeonMap(64);
        _dungeonMap.LoadContent(Content, "Images/DungeonAtlas");

        if (_loadedSession == null)
        {
            _dungeonMap.LoadMap(Content, _level);

            _playerCharacter = LoadEntities.CreatePlayer(_dungeonMap, GameAssets.GameObjectAtlas, CanActorMoveTo, GetBlockingActorAtWorldPos);

            _actors = LoadEntities.ParseActors(_dungeonMap, GameAssets.GameObjectAtlas, CanActorMoveTo, GetBlockingActorAtWorldPos);
            _props = LoadEntities.ParseProps(_dungeonMap, GameAssets.GameObjectAtlas);
        }
        else
        {
            _dungeonMap.LoadMap(Content, _loadedSession.Level);

            _cameraPos = _loadedSession.CameraPosition;

            _props = LoadEntities.ParseProps(_loadedSession, GameAssets.GameObjectAtlas);
            _actors = LoadEntities.ParseActors(_loadedSession, GameAssets.GameObjectAtlas, CanActorMoveTo, GetBlockingActorAtWorldPos);
            _playerCharacter = LoadEntities.CreatePlayer(_loadedSession, GameAssets.GameObjectAtlas, CanActorMoveTo, GetBlockingActorAtWorldPos);
        }

        _playerCharacter.BlockedByActor += (self, blocker) =>
        {
            if (blocker != _playerCharacter)
            {
                _state = GameState.Combat;
                StartCombat(blocker);
            }
        };

        _currentSession = GameSessionExtensions.ParseGameSession(_playerCharacter, _actors, _props, _level);

        _playerCharacter.RestoreCollectedItems(_currentSession.Player.CollectedEquipment);
    }

    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        GumService.Default.Root.Children.Clear();
        _hud = new GameHudUI();

        //Core.ExitOnEscape = false;
    }

    public override void Update(GameTime gameTime)
    {
        if (_state == GameState.Combat)
        {
            _combatRemainingMs -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_combatRemainingMs <= 0)
                _state = GameState.Playing;
            return;
        }

        foreach (var actor in _actors)
        {
            actor.Update(gameTime);
        }

        foreach (var prop in _props)
        {
            prop.Update(gameTime);
            if (prop.CanInteract && prop.TryInteract(_playerCharacter))
            {

            }

            if (prop.IsCollected)
            {
                if (prop.MapKind == 'W')
                {
                    _hud.CreateInventoryItem("tier-1-sword", _hud._itemContainer);
                }

                if (prop.MapKind == 'A')
                {
                    _hud.CreateInventoryItem("tier-1-armor", _hud._itemContainer);
                }
            }
        }

        foreach (var itemKey in _playerCharacter.CollectedItemKeys)
        {
            if (_hud._itemContainer.Children.All(child => child.Name != itemKey))
            {
                _hud.CreateInventoryItem(itemKey, _hud._itemContainer);
            }
        }

        _playerCharacter.Update(gameTime);

        _cameraPos = Camera.CameraLoc(Core.GraphicsDevice.Viewport, _playerCharacter, _dungeonMap,
                                        _cameraPos, gameTime, worldScale, CAMERA_SMOOTH_SPEED, _snapCamera, UiTopPaddingPx);

        _snapCamera = false;

        _hud.SetHp(_playerCharacter.HealthCurrent, _playerCharacter.HealthPool);
        _hud.SetXp(_playerCharacter.CurrentXP, _playerCharacter.XPToNextLevel);
        _hud.SetStats(_playerCharacter.MinDamage, _playerCharacter.MaxDamage, _playerCharacter.Armor, _playerCharacter.CurrentLevel);
        _hud.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        Matrix cameraTransform =
            Matrix.CreateTranslation(-_cameraPos.X, -_cameraPos.Y + UiTopPaddingPx, 0f) *
            Matrix.CreateScale(worldScale, worldScale, 1f);

        var gd = Core.GraphicsDevice;
        var prevScissor = gd.ScissorRectangle;
        gd.ScissorRectangle = new Rectangle(
            0,
            (int)UiTopPaddingPx,
            gd.Viewport.Width,
            gd.Viewport.Height - (int)UiTopPaddingPx
        );
        Core.SpriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: cameraTransform,
            rasterizerState: new RasterizerState { ScissorTestEnable = true }
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

        gd.ScissorRectangle = prevScissor;

        _hud.Draw();
    }

    private Point ToTile(Vector2 worldPos)
    {
        int tx = (int)(worldPos.X / _dungeonMap.TileSize);
        int ty = (int)(worldPos.Y / _dungeonMap.TileSize);
        return new Point(tx, ty);
    }
    private IEnumerable<ActorBase> EnumerateAllActors()
    {
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

    private ActorBase GetBlockingActorAtWorldPos(ActorBase self, Vector2 candidateWorldPos)
    {
        Point candidateTile = ToTile(candidateWorldPos);

        foreach (var other in EnumerateAllActors())
        {
            if (ReferenceEquals(other, self)) continue;

            if (ToTile(other.Position) == candidateTile)
                return other;

            if (other.IsMoving && ToTile(other.TargetPosition) == candidateTile)
                return other;
        }

        return null;
    }

    private void StartCombat(ActorBase monster)
    {
        _currentSession = GameSessionExtensions.ParseGameSession(_playerCharacter, _actors, _props, _level);
        Core.ChangeScene(new CombatScene(new CombatEncounter(_playerCharacter, monster, _dungeonMap, GameAssets.GameObjectAtlas, _currentSession)));
    }
}