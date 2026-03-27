using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.Player;
using Dungeoneer.Maps;

namespace Dungeoneer.GameObjects.HelperMethods;

public sealed class CombatEncounter
{
    public PlayerCharacter Player { get; }
    public ActorBase Monster { get; }
    public DungeonMap Map { get; }

    public CombatEncounter(PlayerCharacter player, ActorBase monster, DungeonMap map)
    {
        Player = player;
        Monster = monster;
        Map = map;
    }
}
