using Dungeoneer.GameObjects.HelperMethods;
using MonoGameLibrary.Scenes;

namespace Dungeoneer.Scenes;

public class CombatScene : Scene
{
    private readonly CombatEncounter _encounter;
    public CombatScene(CombatEncounter encounter)
    {
        _encounter = encounter;
    }
    public override void LoadContent()
    {
        // använd _encounter.Player och _encounter.Monster för sprites/stats/UI
    }
}
