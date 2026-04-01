using Microsoft.Xna.Framework;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.GameObjects.Bases;

public abstract class PropBase
{
    public int PropId { get; }
    public char MapKind { get; }
    protected Sprite ActiveSprite { get; set; }
    public Vector2 Position { get; protected set; }
    public bool IsCollected { get; protected set; }

    public virtual bool CanInteract => !IsCollected;

    protected PropBase(Sprite sprite, Vector2 position, int propId, char mapKind)
    {
        ActiveSprite = sprite;
        Position = position;
        PropId = propId;
        MapKind = mapKind;
    }

    public virtual void Update(GameTime gameTime)
    {
        // default: statiskt
    }

    public virtual void Draw()
    {
        if (IsCollected) return;
        ActiveSprite?.Draw(Core.SpriteBatch, GetDrawPosition());
    }

    protected virtual Vector2 GetDrawPosition()
    {
        return Position;
    }

    public bool TryInteract(ActorBase actor)
    {
        if (!CanInteract) return false;
        if (actor == null) return false;
        if (!IsActorInRange(actor)) return false;
        OnInteract(actor);
        return true;
    }

    protected virtual bool IsActorInRange(ActorBase actor)
    {
        return actor.Position == Position;
    }

    protected abstract void OnInteract(ActorBase actor);

    protected void Collect()
    {
        IsCollected = true;
    }
}
