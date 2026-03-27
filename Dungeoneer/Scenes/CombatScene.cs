using Dungeoneer.GameObjects.HelperMethods;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;

namespace Dungeoneer.Scenes;

public class CombatScene : Scene
{
    private readonly CombatEncounter _encounter;

    private float worldScale = 1.0f; // ex 2x

    private Vector2 _cameraPos = Vector2.Zero;

    private double _combatRemainingMs;

    public CombatScene(CombatEncounter encounter)
    {
        _encounter = encounter;
    }
    public override void LoadContent()
    {
        // använd _encounter.Player och _encounter.Monster för sprites/stats/UI
        _combatRemainingMs = 5000;
    }

    public override void Draw(GameTime gameTime)
    {
        Matrix cameraTransform =
            Matrix.CreateTranslation(-_cameraPos.X, -_cameraPos.Y, 0f) *
            Matrix.CreateScale(worldScale, worldScale, 1f);

        Core.SpriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: cameraTransform
        );

        _encounter.Player.Draw();
        _encounter.Monster.Draw();

        Core.SpriteBatch.End();
    }

    public override void Update(GameTime gameTime)
    {
        _combatRemainingMs -= gameTime.ElapsedGameTime.TotalMilliseconds;
        if (_combatRemainingMs <= 0)
        {
            Core.ChangeScene(new GameScene());
        }

        _encounter.Player.Update(gameTime);
        _encounter.Monster.Update(gameTime);
    }
}
