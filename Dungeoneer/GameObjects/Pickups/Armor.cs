using Dungeoneer.GameObjects.Bases;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.GameObjects.Pickups;

public class Armor : PropBase
{
    public override string PropName { get; protected set; } = "tier-1-armor";
    public bool IsEquipped { get; set; } = false;
    public override int ArmorBoosValue { get; protected set; } = 2;

    public Armor(
        Sprite sprite,
        float xPos,
        float yPos,
        int propId,
        char mapKind)
        : base(sprite, new Vector2(xPos, yPos), propId, 'A')
    {

    }

    protected override void OnInteract(ActorBase player)
    {
        player.CollectedEquipment.Add(this);
        player.Armor += ArmorBoosValue;
        IsCollected = true;
    }
}