using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.GameSessions;
using Dungeoneer.Scenes;
using Microsoft.Xna.Framework;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.GameObjects.Pickups;

public class ExitStairs : PropBase
{
    public override string PropName { get; protected set; } = "exit-stairs";

    public ExitStairs(
        Sprite sprite,
        float xPos,
        float yPos,
        int propId,
        char mapKind)
        : base(sprite, new Vector2(xPos, yPos), propId, 'E')
    {

    }

    protected override void OnInteract(ActorBase player)
    {
        // Do nothing, we need the player session state to change the scene, so we will use the TryInteract overload that takes the player session state
    }

    public void OnInteract(string NextLevel, PlayerSessionState playerState)
    {
        if (NextLevel == "level4")
            Core.ChangeScene(new EndOfVersionScene());

        Core.ChangeScene(new GameScene(NextLevel, null, playerState));
    }
}