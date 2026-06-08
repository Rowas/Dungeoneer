using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.GameSessions;
using Dungeoneer.GameObjects.Helpers;
using Dungeoneer.GameObjects.Pickups;
using Dungeoneer.GameObjects.Player;
using Dungeoneer.Maps;
using Dungeoneer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeoneer.Scenes;

public class GameScene : Scene
{
    private enum GameState
    {
        Playing,
        Paused,
        Combat
    }

    private PlayerCharacter _playerCharacter;
    private List<ActorBase> _actors = new();
    private List<PropBase> _props = new();
    private DungeonMap _dungeonMap;
    private string _level;

    private GameSceneHudUI _hud;

    private Vector2 _cameraPos = Vector2.Zero;
    private float CAMERA_SMOOTH_SPEED = 1.5f;
    private bool _snapCamera = true;
    const float UiTopPaddingPx = 64f * 2f;

    private float worldScale = 1.0f;

    private float _saturation = 1.0f;
    private Effect _grayscaleEffect;
    private const float FADE_SPEED = 0.02f;

    private GameState _state;

    private GameSession _currentSession;
    private readonly GameSession _loadedSession;
    private readonly PlayerSessionState _playerSession;

    private const int VisionRadiusTiles = 10;
    private bool[,] _visibleNow;
    private bool[,] _explored;
    private Point _lastFovOrigin = new Point(int.MinValue, int.MinValue);

    private void OnResumeButtonClicked(object sender, EventArgs args)
    {
        _state = GameState.Playing;
    }

    private void OnQuitButtonClicked(object sender, EventArgs args)
    {
        Core.ChangeScene(new TitleScene());
    }

    private void OnSaveLoadClicked(object sender, EventArgs args)
    {
        Core.ChangeScene(new SaveLoadScene(true, _currentSession));
    }

    public GameScene(string level, GameSession session = null, PlayerSessionState playerSession = null)
    {
        _loadedSession = session;
        _level = level;
        _playerSession = playerSession;
    }

    public override void LoadContent()
    {
        _dungeonMap = new DungeonMap(64);
        _dungeonMap.LoadContent(Content, "Images/DungeonAtlas");

        if (_loadedSession == null)
        {
            _dungeonMap.LoadMap(Content, _level);

            if (_playerSession == null)
            {
                _playerCharacter = LoadEntities.CreatePlayer(_dungeonMap, GameAssets.GameObjectAtlas, CanActorMoveTo, GetBlockingActorAtWorldPos);
            }
            else
            {
                GameSession nextLevel = new();
                nextLevel.Player = _playerSession;
                nextLevel.Player.Position = _dungeonMap.PlayerStart;
                _playerCharacter = LoadEntities.CreatePlayer(nextLevel, GameAssets.GameObjectAtlas, CanActorMoveTo, GetBlockingActorAtWorldPos);
                _playerCharacter.SkillCooldowns = _playerSession.SkillCooldowns;
            }

            _actors = LoadEntities.ParseActors(_dungeonMap, GameAssets.GameObjectAtlas, CanActorMoveTo, GetBlockingActorAtWorldPos, _level);
            _props = LoadEntities.ParseProps(_dungeonMap, GameAssets.GameObjectAtlas);
        }
        else
        {
            _dungeonMap.LoadMap(Content, _loadedSession.Level);

            _cameraPos = _loadedSession.CameraPosition;

            _props = LoadEntities.ParseProps(_loadedSession, GameAssets.GameObjectAtlas);
            _actors = LoadEntities.ParseActors(_loadedSession, GameAssets.GameObjectAtlas, CanActorMoveTo, GetBlockingActorAtWorldPos, _loadedSession.Level);
            _playerCharacter = LoadEntities.CreatePlayer(_loadedSession, GameAssets.GameObjectAtlas, CanActorMoveTo, GetBlockingActorAtWorldPos);
        }

        Exploration();

        if (_loadedSession != null)
            _explored = GameSessionExtensions.UnpackExplored(_loadedSession.ExploredTiles, _dungeonMap.Columns, _dungeonMap.Rows);

        _playerCharacter.BlockedByActor += (self, blocker) =>
        {
            if (blocker != _playerCharacter)
            {
                _state = GameState.Combat;
                StartCombat(blocker);
            }
        };

        _currentSession = GameSessionExtensions.ParseGameSession(_playerCharacter, _actors, _props, _level, _explored, _dungeonMap.Columns, _dungeonMap.Rows);

        _playerCharacter.RestoreCollectedItems(_currentSession.Player.CollectedEquipment);

        _grayscaleEffect = Content.Load<Effect>("effects/grayscaleEffect");
    }

    public void Exploration()
    {
        _visibleNow = new bool[_dungeonMap.Columns, _dungeonMap.Rows];
        _explored = new bool[_dungeonMap.Columns, _dungeonMap.Rows];
        _lastFovOrigin = new Point(int.MinValue, int.MinValue);
    }

    public override void Initialize()
    {
        base.Initialize();

        InitializeUI();

        Core.ExitOnEscape = false;
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();

        _hud = new GameSceneHudUI();

        _hud.ResumeButtonClick += OnResumeButtonClicked;
        _hud.QuitButtonClick += OnQuitButtonClicked;

        _hud.SaveLoadClick += OnSaveLoadClicked;
    }

    private void TogglePause()
    {
        if (_state == GameState.Paused)
        {
            _hud.HidePausePanel();

            _state = GameState.Playing;
        }
        else
        {
            _currentSession = GameSessionExtensions.ParseGameSession(_playerCharacter, _actors, _props, _level, _explored, _dungeonMap.Columns, _dungeonMap.Rows);

            if (_loadedSession == null)
                _hud.ShowPausePanel(_currentSession.Level);
            else
                _hud.ShowPausePanel(_loadedSession.Level);

            _state = GameState.Paused;

            _saturation = 1.0f;
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (_state != GameState.Playing)
        {
            _saturation = Math.Max(0.0f, _saturation - FADE_SPEED);
        }

        if (GameController.Pause())
        {
            TogglePause();
        }

        if (_state == GameState.Paused)
        {
            _hud.Update(gameTime);
            return;
        }

        foreach (var actor in _actors)
        {
            actor.Update(gameTime);
        }

        // Hiskelig lösning som behöver förbättras.
        foreach (var prop in _props)
        {
            prop.Update(gameTime);

            if (prop.CanInteract && prop.TryInteract(_playerCharacter))
            {
                if (prop.MapKind == 'E')
                {
                    var exitStairs = prop as ExitStairs;
                    if (exitStairs != null)
                    {
                        switch (_level)
                        {
                            case "level1":
                                _level = "level2";
                                break;
                            case "level2":
                                _level = "level3";
                                break;
                            case "level3":
                                _level = "level4";
                                break;
                            case "level4":
                                _level = "level5";
                                break;
                            default:
                                _level = "level1";
                                break;
                        }

                        exitStairs.OnInteract(_level, _currentSession.Player);
                    }
                }
            }

            if (prop.IsCollected)
            {
                if (prop.MapKind == 'W')
                {
                    _currentSession.Player.CollectedEquipment.Add(new CollectedItemState { ItemKey = "tier-1-sword" });
                    _hud.CreateInventoryItem("tier-1-sword", _hud._itemContainer);
                }

                if (prop.MapKind == 'A')
                {
                    _currentSession.Player.CollectedEquipment.Add(new CollectedItemState { ItemKey = "tier-1-armor" });
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

        Point origin = ToTile(_playerCharacter.Position);

        if (origin != _lastFovOrigin)
        {
            _visibleNow = LOS.ComputeVisible(_dungeonMap, origin, VisionRadiusTiles);

            for (int y = 0; y < _dungeonMap.Rows; y++)
                for (int x = 0; x < _dungeonMap.Columns; x++)
                    _explored[x, y] |= _visibleNow[x, y];

            _lastFovOrigin = origin;

            _currentSession.Player.Position = _playerCharacter.Position;
            _currentSession.CameraPosition = _cameraPos;
            _currentSession.ExploredTiles = GameSessionExtensions.PackExplored(_explored, _dungeonMap.Columns, _dungeonMap.Rows);
        }

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

        _dungeonMap.Draw(Core.SpriteBatch, _visibleNow, _explored);

        _playerCharacter.Draw();

        foreach (var actor in _actors)
        {
            var t = ToTile(actor.Position);
            if (_visibleNow != null && t.X >= 0 && t.X < _dungeonMap.Columns && t.Y >= 0 && t.Y < _dungeonMap.Rows && _visibleNow[t.X, t.Y])
                actor.Draw();
        }
        foreach (var prop in _props)
        {
            var t = ToTile(prop.Position);
            if (_visibleNow != null && t.X >= 0 && t.X < _dungeonMap.Columns && t.Y >= 0 && t.Y < _dungeonMap.Rows && _visibleNow[t.X, t.Y])
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
        if (!_dungeonMap.IsWalkable(candidateWorldPos))
            return false;
        Point candidateTile = ToTile(candidateWorldPos);

        foreach (var other in EnumerateAllActors())
        {
            if (ReferenceEquals(other, self))
                continue;

            if (ToTile(other.Position) == candidateTile)
                return false;

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

    public bool IsAttackAnimationPlaying()
    {
        return _playerCharacter.AttackMade;
    }

    private void StartCombat(ActorBase monster)
    {
        _currentSession = GameSessionExtensions.ParseGameSession(_playerCharacter, _actors, _props, _level, _explored, _dungeonMap.Columns, _dungeonMap.Rows);
        Core.ChangeScene(new CombatScene(new CombatEncounter(_playerCharacter, monster, _dungeonMap, GameAssets.GameObjectAtlas, _currentSession)));
    }
}