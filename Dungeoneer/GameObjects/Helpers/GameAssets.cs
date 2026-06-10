using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace Dungeoneer.GameObjects.Helpers;

public static class GameAssets
{
    public static SpriteFont Font { get; private set; }
    public static SpriteFont Font5x { get; private set; }
    public static TextureAtlas GameObjectAtlas { get; private set; }
    public static Texture2D GameOverBackground { get; private set; }
    public static Song TitleScreenBGM { get; private set; }
    public static Song ExplorationMapBGM { get; private set; }
    public static Song RegularCombatBGM { get; private set; }
    public static Song BossCombatBGM { get; private set; }
    public static Song GameOverBGM { get; private set; }
    public static Song LevelUpBGM { get; private set; }
    public static SoundEffect AttackSFX { get; private set; }
    public static SoundEffect BiteConsumeSFX { get; private set; }
    public static SoundEffect DefendSFX { get; private set; }
    public static SoundEffect HealSFX { get; private set; }
    public static SoundEffect PickupWeaponSFX { get; private set; }
    public static SoundEffect PickupArmorSFX { get; private set; }

    public static void Load()
    {
        Font = Core.Content.Load<SpriteFont>("fonts/04B_30");
        Font5x = Core.Content.Load<SpriteFont>("fonts/04B_30_5x");

        GameObjectAtlas = TextureAtlas.FromFile(Core.Content, "images/GameObjectAtlas.xml");

        GameOverBackground = Core.Content.Load<Texture2D>("Images/GameOverBackground");

        TitleScreenBGM = Core.Content.Load<Song>("Music/TitleTheme");
        ExplorationMapBGM = Core.Content.Load<Song>("Music/ExplorationMapTheme");
        RegularCombatBGM = Core.Content.Load<Song>("Music/RegularBattleTheme");
        BossCombatBGM = Core.Content.Load<Song>("Music/BossBattleTheme");
        GameOverBGM = Core.Content.Load<Song>("Music/GameOverTheme");
        LevelUpBGM = Core.Content.Load<Song>("Music/LevelUpTheme");

        AttackSFX = Core.Content.Load<SoundEffect>("SoundEffects/Attack");
        BiteConsumeSFX = Core.Content.Load<SoundEffect>("SoundEffects/BiteConsume");
        DefendSFX = Core.Content.Load<SoundEffect>("SoundEffects/Defend");
        HealSFX = Core.Content.Load<SoundEffect>("SoundEffects/Heal");
        PickupWeaponSFX = Core.Content.Load<SoundEffect>("SoundEffects/PickupWeapon");
        PickupArmorSFX = Core.Content.Load<SoundEffect>("SoundEffects/PickupArmor");
    }
}
