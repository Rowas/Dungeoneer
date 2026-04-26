using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.Player;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeoneer.GameObjects.GameSessions;

/// <summary>
/// Långlivat tillstånd för en körning (och framtida sparning).
/// Innehåller ingen grafik, inga ActorBase-referenser.
/// </summary>
public sealed class GameSession
{
    /// <summary>Öka när sparfilsformat ändras.</summary>
    public int SaveVersion { get; set; } = 1;

    // --- Nivå ---
    /// <summary>Relativt Content Root, t.ex. LevelFiles/Level1.txt</summary>
    public string Level { get; set; } = string.Empty;
    public int TileSize { get; set; } = 64;

    // --- Kamera ---
    public Vector2 CameraPosition { get; set; }

    // --- Spelare ---
    public PlayerSessionState Player { get; set; } = new();

    // --- Entiteter (fiender m.m.) ---
    public List<MonsterSessionState> Monsters { get; set; } = new();

    // --- Föremål på marken ---
    public List<PropSessionState> Props { get; set; } = new();

    // --- Hjälp: nästa id vid spawn (om ni inte räknar från kartan) ---
    public int NextEntityId { get; set; } = 1;
    public int TakeNextEntityId() => NextEntityId++;
    public byte[] ExploredTiles { get; set; }
}
public sealed class PlayerSessionState
{
    public int EntityId { get; set; } = 0;
    public char MapKind { get; set; } = '@';
    public Vector2 Position { get; set; }
    public int HealthCurrent { get; set; }
    public int HealthMax { get; set; }
    public List<CollectedItemState> CollectedEquipment { get; set; } = new();
    public int CurrentLevel { get; set; }
    public int CurrentXP { get; set; }
    public int XPToNextLevel { get; set; }
    public int MinDamage { get; set; }
    public int MaxDamage { get; set; }
    public int Armor { get; set; }
    // Lägg till inventory, guld, osv. när det finns.
}
/// <summary>Motsvarar en spawnad fiende i världen.</summary>
public sealed class MonsterSessionState
{
    public int EntityId { get; set; }
    /// <summary>Samma tecken som i kartfilen / Entities, t.ex. 'r', 'B'.</summary>
    public char MapKind { get; set; }
    public Vector2 Position { get; set; }
    public bool IsAlive { get; set; } = true;
    public bool IsDamaged { get; set; } = false;
    public int HealthCurrent { get; set; }
}
public sealed class PropSessionState
{
    public int EntityId { get; set; }
    /// <summary>t.ex. 'P', 'F', 'A', 'W'</summary>
    public char MapKind { get; set; }
    public Vector2 Position { get; set; }
    public bool IsCollected { get; set; }
}

public sealed class ExploredState
{
    public int MapColumns { get; set; }
    public int MapRows { get; set; }
    public byte[] ExploredBits { get; set; } = Array.Empty<byte>();
}

public sealed class CombatOutcome
{
    public int PlayerHealthAfter { get; set; }
    public int MonsterEntityId { get; set; }
    public bool MonsterDefeated { get; set; }
    public int MonsterHealthAfter { get; set; }
    public int XPGained { get; set; }
    // Loot, xp, etc. senare.
}

public sealed class CollectedItemState
{
    public string ItemKey { get; set; } = string.Empty;
}

public static class GameSessionCombatExtensions
{
    public static void ApplyCombatOutcome(this GameSession session, CombatOutcome outcome)
    {
        session.Player.HealthCurrent = outcome.PlayerHealthAfter;
        for (int i = 0; i < session.Monsters.Count; i++)
        {
            if (session.Monsters[i].EntityId != outcome.MonsterEntityId)
                continue;
            if (outcome.MonsterDefeated)
            {
                session.Monsters[i].IsAlive = false;
                session.Player.CurrentXP += outcome.XPGained;
            }
            else
            {
                session.Monsters[i].HealthCurrent = outcome.MonsterHealthAfter;
                session.Monsters[i].IsDamaged = true;
            }
            return;
        }
    }
}

public static class GameSessionExtensions
{
    public static GameSession ParseGameSession(PlayerCharacter _playerCharacter, List<ActorBase> _actors, List<PropBase> _props, string _level,
        bool[,] exploration, int Columns, int Rows)
    {
        GameSession currentSession = new GameSession
        {
            SaveVersion = 1,
            Level = _level,
            TileSize = 64,
            Player = new PlayerSessionState
            {
                Position = _playerCharacter.Position,
                HealthMax = _playerCharacter.HealthPool,
                HealthCurrent = _playerCharacter.HealthCurrent,
                CollectedEquipment = _playerCharacter.CollectedItemKeys.Select(key => new CollectedItemState { ItemKey = key }).ToList(),
                CurrentLevel = _playerCharacter.CurrentLevel,
                CurrentXP = _playerCharacter.CurrentXP,
                XPToNextLevel = _playerCharacter.XPToNextLevel,
                MinDamage = _playerCharacter.MinDamage,
                MaxDamage = _playerCharacter.MaxDamage,
                Armor = _playerCharacter.Armor
            },
            Monsters = new List<MonsterSessionState>(),
            Props = new List<PropSessionState>(),
            NextEntityId = 0,
            ExploredTiles = GameSessionExtensions.PackExplored(exploration, Columns, Rows)
        };

        currentSession.Monsters = _actors
            .Select(a => new MonsterSessionState
            {
                EntityId = a.EntityId,
                MapKind = a.MapKind,
                Position = a.Position,
                IsAlive = true,
            })
            .ToList();

        currentSession.Props = _props
            .Select(p => new PropSessionState
            {
                EntityId = p.PropId,
                MapKind = p.MapKind,
                Position = p.Position,
                IsCollected = p.IsCollected,
            })
            .ToList();

        return currentSession;
    }

    public static byte[] PackExplored(bool[,] explored, int columns, int rows)
    {
        int total = columns * rows;
        var bytes = new byte[(total + 7) / 8];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
                if (explored[x, y])
                {
                    int index = y * columns + x;
                    bytes[index / 8] |= (byte)(1 << (index % 8));
                }
        return bytes;
    }
    public static bool[,] UnpackExplored(byte[]? bytes, int columns, int rows)
    {
        var explored = new bool[columns, rows];
        if (bytes == null || bytes.Length == 0) return explored;
        int total = columns * rows;
        int expected = (total + 7) / 8;
        if (bytes.Length != expected) return explored;
        for (int index = 0; index < total; index++)
        {
            if ((bytes[index / 8] & (1 << (index % 8))) == 0) continue;
            int x = index % columns;
            int y = index / columns;
            explored[x, y] = true;
        }
        return explored;
    }
}