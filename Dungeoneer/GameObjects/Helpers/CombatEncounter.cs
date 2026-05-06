using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.GameSessions;
using Dungeoneer.GameObjects.Player;
using Dungeoneer.Maps;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.GameObjects.Helpers;

public sealed class CombatEncounter
{
    public PlayerCharacter Player { get; }
    public ActorBase Monster { get; }
    public DungeonMap Map { get; }
    public TextureAtlas Atlas { get; }
    public GameSession Session { get; }

    public CombatEncounter(PlayerCharacter player, ActorBase monster, DungeonMap map, TextureAtlas atlas, GameSession PreviousSession)
    {
        Player = player;
        Monster = monster;
        Map = map;
        Atlas = atlas;
        Session = PreviousSession;
    }
}
