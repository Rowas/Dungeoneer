using Dungeoneer.GameObjects.Bases;
using Dungeoneer.GameObjects.Monsters;
using Dungeoneer.GameObjects.Pickups;
using Dungeoneer.Maps;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;
using System;
using System.Collections.Generic;

namespace Dungeoneer.GameObjects.HelperMethods;

public static class LoadEntities
{
    public static List<ActorBase> ParseActors(DungeonMap _dungeonMap, TextureAtlas atlas,
                    Func<ActorBase, Vector2, bool> canMoveToWorldPos, Func<ActorBase, Vector2, ActorBase> getBlockingActorAtWorldPos)
    {
        List<ActorBase> _actors = new();

        foreach (var entity in _dungeonMap.Entities)
        {
            switch (entity.Type)
            {
                case 'r':
                    var ratSprite = atlas.CreateAnimatedSprite("plague-rat-idle-animation");
                    ratSprite.Scale = Vector2.One;
                    ratSprite.Animation.Delay = TimeSpan.FromMilliseconds(84);

                    _actors.Add(new Rat(
                        ratSprite,
                        ratSprite,
                        entity.Position.X,
                        entity.Position.Y,
                        canMoveToWorldPos,
                        getBlockingActorAtWorldPos
                    ));
                    break;
                case 'b':
                    var batSprite = atlas.CreateAnimatedSprite("evil-bat-idle-animation");
                    batSprite.Scale = Vector2.One;
                    batSprite.Animation.Delay = TimeSpan.FromMilliseconds(250);
                    _actors.Add(new Bat(
                        batSprite,
                        batSprite,
                        entity.Position.X,
                        entity.Position.Y,
                        canMoveToWorldPos,
                        getBlockingActorAtWorldPos
                    ));
                    break;
                case 'G':
                    var ogreSprite = atlas.CreateAnimatedSprite("brawny-ogre-idle-animation");
                    ogreSprite.Scale = Vector2.One;
                    _actors.Add(new Ogre(
                        ogreSprite,
                        ogreSprite,
                        entity.Position.X,
                        entity.Position.Y,
                        canMoveToWorldPos,
                        getBlockingActorAtWorldPos
                    ));
                    break;
                case 'B':
                    var bossSprite = atlas.CreateAnimatedSprite("grave-revenant-boss-idle-animation");
                    bossSprite.Scale = Vector2.One;
                    _actors.Add(new BossMonster(
                        bossSprite,
                        bossSprite,
                        entity.Position.X,
                        entity.Position.Y,
                        canMoveToWorldPos,
                        getBlockingActorAtWorldPos
                    ));
                    break;
            }
        }

        return _actors;
    }

    public static List<PropBase> ParseProps(DungeonMap _dungeonMap, TextureAtlas atlas)
    {
        List<PropBase> _props = new();
        foreach (var entity in _dungeonMap.Entities)
        {
            switch (entity.Type)
            {
                case 'P':
                    var potionSprite = atlas.CreateSprite("red-potion");
                    potionSprite.Scale = Vector2.One;
                    _props.Add(new HealthPotion(
                        potionSprite,
                        entity.Position.X,
                        entity.Position.Y
                    ));
                    break;
                case 'F':
                    var foodSprite = atlas.CreateSprite("food-bread");
                    foodSprite.Scale = Vector2.One;
                    _props.Add(new Food(
                        foodSprite,
                        entity.Position.X,
                        entity.Position.Y
                    ));
                    break;
                case 'A':
                    var armorSprite = atlas.CreateSprite("tier-1-armor");
                    armorSprite.Scale = Vector2.One;
                    _props.Add(new Armor(
                        armorSprite,
                        entity.Position.X,
                        entity.Position.Y
                    ));
                    break;
                case 'W':
                    var weaponSprite = atlas.CreateSprite("tier-1-sword");
                    weaponSprite.Scale = Vector2.One;
                    _props.Add(new Weapon(
                        weaponSprite,
                        entity.Position.X,
                        entity.Position.Y
                    ));
                    break;
            }
        }
        return _props;
    }
}
