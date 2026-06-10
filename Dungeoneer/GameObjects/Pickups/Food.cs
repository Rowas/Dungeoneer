using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.Helpers;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.GameObjects.Pickups;

public class Food : PropBase
{
    public override string PropName { get; protected set; } = "Bread";
    private int HealValue { get; } = 10;
    public Food(
        Sprite sprite,
        float xPos,
        float yPos,
        int propId,
        char mapKind)
        : base(sprite, new Vector2(xPos, yPos), propId, 'F')
    {

    }

    protected override void OnInteract(ActorBase player)
    {
        GameAssets.HealSFX.Play();
        player.HealthCurrent += HealValue;

        if (player.HealthCurrent > player.HealthPool)
            player.HealthCurrent = player.HealthPool;

        IsCollected = true;
    }
}