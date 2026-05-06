using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.GameSessions;
using Dungeoneer.GameObjects.Monsters;
using Dungeoneer.GameObjects.Pickups;
using Dungeoneer.GameObjects.Player;
using Dungeoneer.Maps;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;
using System;
using System.Collections.Generic;

namespace Dungeoneer.GameObjects.Helpers;

public static class LoadEntities
{
    // -----------------------------------
    // Player parsing and creation methods
    // -----------------------------------

    public static PlayerCharacter CreatePlayer(DungeonMap dungeonMap,
        TextureAtlas atlas, Func<ActorBase, Vector2, bool> canMoveToWorldPos,
        Func<ActorBase, Vector2, ActorBase> getBlockingActorAtWorldPos)
    {
        AnimatedSprite pcSpriteIdle = GameAssets.GameObjectAtlas.CreateAnimatedSprite("slime-idle-pink-animation");
        pcSpriteIdle.Scale = Vector2.One;
        AnimatedSprite pcSpriteMove = GameAssets.GameObjectAtlas.CreateAnimatedSprite("slime-move-pink-animation");
        pcSpriteMove.Scale = Vector2.One;

        return new PlayerCharacter(pcSpriteIdle, pcSpriteMove,
            dungeonMap.PlayerStart.X, dungeonMap.PlayerStart.Y,
            canMoveToWorldPos, getBlockingActorAtWorldPos, 0, dungeonMap.PlayerSymbol);
    }

    public static PlayerCharacter CreatePlayer(GameSession session,
        TextureAtlas atlas, Func<ActorBase, Vector2, bool> canMoveToWorldPos,
        Func<ActorBase, Vector2, ActorBase> getBlockingActorAtWorldPos)
    {
        AnimatedSprite pcSpriteIdle = GameAssets.GameObjectAtlas.CreateAnimatedSprite("slime-idle-pink-animation");
        pcSpriteIdle.Scale = Vector2.One;
        AnimatedSprite pcSpriteMove = GameAssets.GameObjectAtlas.CreateAnimatedSprite("slime-move-pink-animation");
        pcSpriteMove.Scale = Vector2.One;

        var player = new PlayerCharacter(pcSpriteIdle, pcSpriteMove,
            session.Player.Position.X, session.Player.Position.Y,
            canMoveToWorldPos, getBlockingActorAtWorldPos, session.Player.EntityId, session.Player.MapKind);

        player.PlayerCombatUpdate(session);

        return player;
    }

    // -----------------------------------
    // Actor parsing and creation methods
    // -----------------------------------

    public static List<ActorBase> ParseActors(DungeonMap dungeonMap,
    TextureAtlas atlas, Func<ActorBase, Vector2, bool> canMoveToWorldPos,
    Func<ActorBase, Vector2, ActorBase> getBlockingActorAtWorldPos, string currentLevel)
    {
        var list = new List<ActorBase>();
        foreach (var entity in dungeonMap.Entities)
        {
            int actorId = list.Count + 1;

            var actor = CreateMonster(
                entity.Type,
                entity.Position.X,
                entity.Position.Y,
                actorId,
                atlas,
                canMoveToWorldPos,
                getBlockingActorAtWorldPos);


            if (actor != null)
            {
                actor.ProgressionScaling(currentLevel);
                list.Add(actor);
            }
        }
        return list;
    }

    public static List<ActorBase> ParseActors(GameSession session,
    TextureAtlas atlas, Func<ActorBase, Vector2, bool> canMoveToWorldPos,
    Func<ActorBase, Vector2, ActorBase> getBlockingActorAtWorldPos, string currentLevel)
    {
        var list = new List<ActorBase>();
        foreach (var m in session.Monsters)
        {
            if (!m.IsAlive) continue;

            var actor = CreateMonster(
                mapKind: m.MapKind,
                x: m.Position.X,
                y: m.Position.Y,
                entityId: m.EntityId,
                atlas: atlas,
                canMoveToWorldPos: canMoveToWorldPos,
                getBlockingActorAtWorldPos: getBlockingActorAtWorldPos);

            if (actor != null)
            {
                if (m.IsDamaged)
                    actor.HealthCurrent = m.HealthCurrent;

                actor.ProgressionScaling(currentLevel);
                list.Add(actor);
            }
        }
        return list;
    }

    private static ActorBase CreateMonster(
    char mapKind,
    float x,
    float y,
    int entityId,
    TextureAtlas atlas,
    Func<ActorBase, Vector2, bool> canMoveToWorldPos,
    Func<ActorBase, Vector2, ActorBase> getBlockingActorAtWorldPos)
    {
        ActorBase actor = mapKind switch
        {
            'r' => CreateRat(atlas, x, y, canMoveToWorldPos, getBlockingActorAtWorldPos, entityId),
            'b' => CreateBat(atlas, x, y, canMoveToWorldPos, getBlockingActorAtWorldPos, entityId),
            'G' => CreateOgre(atlas, x, y, canMoveToWorldPos, getBlockingActorAtWorldPos, entityId),
            'B' => CreateBoss(atlas, x, y, canMoveToWorldPos, getBlockingActorAtWorldPos, entityId),
            _ => null
        };

        return actor;
    }

    private static Rat CreateRat(TextureAtlas atlas, float x, float y,
    Func<ActorBase, Vector2, bool> canMove, Func<ActorBase, Vector2, ActorBase> getBlock, int entityId)
    {
        var sprite = atlas.CreateAnimatedSprite("plague-rat-idle-animation");
        sprite.Scale = Vector2.One;
        sprite.Animation.Delay = TimeSpan.FromMilliseconds(84);
        return new Rat(sprite, sprite, x, y, canMove, getBlock, entityId, 'r');
    }

    private static Bat CreateBat(TextureAtlas atlas, float x, float y,
    Func<ActorBase, Vector2, bool> canMove, Func<ActorBase, Vector2, ActorBase> getBlock, int entityId)
    {
        var sprite = atlas.CreateAnimatedSprite("evil-bat-idle-animation");
        sprite.Animation.Delay = TimeSpan.FromMilliseconds(250);
        sprite.Scale = Vector2.One;
        return new Bat(sprite, sprite, x, y, canMove, getBlock, entityId, 'b');
    }

    private static Ogre CreateOgre(TextureAtlas atlas, float x, float y,
    Func<ActorBase, Vector2, bool> canMove, Func<ActorBase, Vector2, ActorBase> getBlock, int entityId)
    {
        var sprite = atlas.CreateAnimatedSprite("brawny-ogre-idle-animation");
        sprite.Scale = Vector2.One;
        return new Ogre(sprite, sprite, x, y, canMove, getBlock, entityId, 'r');
    }

    private static BossMonster CreateBoss(TextureAtlas atlas, float x, float y,
    Func<ActorBase, Vector2, bool> canMove, Func<ActorBase, Vector2, ActorBase> getBlock, int entityId)
    {
        var sprite = atlas.CreateAnimatedSprite("grave-revenant-boss-idle-animation");
        sprite.Scale = Vector2.One;
        return new BossMonster(sprite, sprite, x, y, canMove, getBlock, entityId, 'r');
    }

    // -----------------------------------
    // Prop parsing and creation methods
    // -----------------------------------
    public static List<PropBase> ParseProps(DungeonMap dungeonMap, TextureAtlas atlas)
    {
        var list = new List<PropBase>();

        foreach (var entity in dungeonMap.Entities)
        {
            int propId = list.Count + 1;

            var prop = CreateProp(
                entity.Type,
                entity.Position.X,
                entity.Position.Y,
                propId,
                atlas);

            if (prop != null)
                list.Add(prop);
        }
        return list;
    }

    public static List<PropBase> ParseProps(GameSession session, TextureAtlas atlas)
    {
        var list = new List<PropBase>();

        foreach (var p in session.Props)
        {
            if (p.IsCollected)
            {
                switch (p.MapKind)
                {
                    case 'W':
                        session.Player.CollectedEquipment.Add(new CollectedItemState { ItemKey = "tier-1-sword" });
                        break;
                    case 'A':
                        session.Player.CollectedEquipment.Add(new CollectedItemState { ItemKey = "tier-1-armor" });
                        break;
                    default:
                        break;
                }
                continue;
            }

            var prop = CreateProp(
                mapKind: p.MapKind,
                x: p.Position.X,
                y: p.Position.Y,
                propId: p.EntityId,
                atlas: atlas);

            if (prop != null)
                list.Add(prop);
        }
        return list;
    }

    private static PropBase CreateProp(char mapKind, float x, float y, int propId, TextureAtlas atlas)
    {
        return mapKind switch
        {
            'P' => CreateHealthPotion(atlas, x, y, propId),
            'F' => CreateFood(atlas, x, y, propId),
            'A' => CreateArmor(atlas, x, y, propId),
            'W' => CreateWeapon(atlas, x, y, propId),
            'E' => CreateExitStairs(atlas, x, y, propId),
            _ => null
        };
    }

    private static HealthPotion CreateHealthPotion(TextureAtlas atlas, float x, float y, int propId)
    {
        var sprite = atlas.CreateSprite("red-potion");
        sprite.Scale = Vector2.One;
        return new HealthPotion(sprite, x, y, propId, 'P');
    }

    private static Food CreateFood(TextureAtlas atlas, float x, float y, int propId)
    {
        var sprite = atlas.CreateSprite("food-bread");
        sprite.Scale = Vector2.One;
        return new Food(sprite, x, y, propId, 'F');
    }

    private static Armor CreateArmor(TextureAtlas atlas, float x, float y, int propId)
    {
        var sprite = atlas.CreateSprite("tier-1-armor");
        sprite.Scale = Vector2.One;
        return new Armor(sprite, x, y, propId, 'A');
    }

    private static Weapon CreateWeapon(TextureAtlas atlas, float x, float y, int propId)
    {
        var sprite = atlas.CreateSprite("tier-1-sword");
        sprite.Scale = Vector2.One;
        return new Weapon(sprite, x, y, propId, 'W');
    }

    private static ExitStairs CreateExitStairs(TextureAtlas atlas, float x, float y, int propId)
    {
        var sprite = atlas.CreateSprite("exit-stairs");
        sprite.Scale = Vector2.One;
        return new ExitStairs(sprite, x, y, propId, 'E');
    }
}