using Dungeoneer.GameObjects.Bases;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.GameObjects.Pickups;

public class HealthPotion : PropBase
{
    public override string PropName { get; protected set; } = "Red Potion";
    private int HealValue { get; } = 30;
    public HealthPotion(
        Sprite sprite,
        float xPos,
        float yPos,
        int propId,
        char mapKind)
        : base(sprite, new Vector2(xPos, yPos), propId, 'P')
    {

    }

    protected override void OnInteract(ActorBase player)
    {
        player.HealthCurrent += HealValue;

        if (player.HealthCurrent > player.HealthPool)
            player.HealthCurrent = player.HealthPool;

        IsCollected = true;
    }
}