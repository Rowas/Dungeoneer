using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.Player;
using Dungeoneer.Maps;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.GameObjects.HelperMethods;

public sealed class CombatEncounter
{
    public PlayerCharacter Player { get; }
    public ActorBase Monster { get; }
    public DungeonMap Map { get; }
    public TextureAtlas Atlas { get; }

    public CombatEncounter(PlayerCharacter player, ActorBase monster, DungeonMap map, TextureAtlas atlas)
    {
        Player = player;
        Monster = monster;
        Map = map;
        Atlas = atlas;
    }
}
