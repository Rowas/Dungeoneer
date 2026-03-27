using Dungeoneer.GameObjects.Bases;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.GameObjects.Pickups;

public class Armor : PropBase
{
    public Armor(
        Sprite sprite,
        float xPos,
        float yPos)
        : base(sprite, new Vector2(xPos, yPos))
    {

    }

    protected override void OnInteract(ActorBase player)
    {
        IsCollected = true;
    }
}