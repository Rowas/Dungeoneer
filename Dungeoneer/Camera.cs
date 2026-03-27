using Dungeoneer.GameObjects.Player;
using Dungeoneer.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Dungeoneer;

static class Camera
{
    public static Vector2 CameraLoc(Viewport vp, PlayerCharacter character, DungeonMap map, Vector2 _cameraPos, GameTime gameTime, float worldScale, float CAMERA_SMOOTH_SPEED)
    {
        float viewWorldWidth = vp.Width / worldScale;
        float viewWorldHeight = vp.Height / worldScale;

        // Player center (sprite 64x64)
        Vector2 playerCenter = character.Position + new Vector2(32f, 32f);

        // Kamera-target så spelaren hamnar i skärmens mitt
        var _cameraTarget = playerCenter - new Vector2(viewWorldWidth * 0.5f, viewWorldHeight * 0.5f);

        // Clamp till kartans bounds
        float mapWidth = map.Columns * map.TileSize;
        float mapHeight = map.Rows * map.TileSize;

        float maxX = MathF.Max(0f, mapWidth - viewWorldWidth);
        float maxY = MathF.Max(0f, mapHeight - viewWorldHeight);

        _cameraTarget.X = MathHelper.Clamp(_cameraTarget.X, 0f, maxX);
        _cameraTarget.Y = MathHelper.Clamp(_cameraTarget.Y, 0f, maxY);

        // Smooth follow (framerate-oberoende)
        float t = 1f - MathF.Exp(-CAMERA_SMOOTH_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds);

        return Vector2.Lerp(_cameraPos, _cameraTarget, t);
    }

}
