using Dungeoneer.GameObjects.Bases;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.GameObjects.Pickups;

public class Weapon : PropBase
{
    public override string PropName { get; protected set; } = "tier-1-sword";
    public bool IsEquipped { get; set; } = false;
    public override int DamageBoostValue { get; protected set; } = 5;
    public Weapon(
        Sprite sprite,
        float xPos,
        float yPos,
        int propId,
        char mapKind)
        : base(sprite, new Vector2(xPos, yPos), propId, 'W')
    {

    }

    protected override void OnInteract(ActorBase player)
    {
        player.CollectedItemKeys.Add(PropName);
        player.MinDamage += DamageBoostValue;
        player.MaxDamage += DamageBoostValue;
        IsCollected = true;
    }
}