using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Dungeoneer.Maps;

public static class LOS
{
    public static bool[,] ComputeVisible(DungeonMap map, Point origin, int radius)
    {
        var visible = new bool[map.Columns, map.Rows];

        if (!InBounds(map, origin.X, origin.Y))
            return visible;

        visible[origin.X, origin.Y] = true;

        int r2 = radius * radius;

        int minX = origin.X - radius;
        int maxX = origin.X + radius;
        int minY = origin.Y - radius;
        int maxY = origin.Y + radius;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (!InBounds(map, x, y))
                    continue;

                int dx = x - origin.X;
                int dy = y - origin.Y;
                if (dx * dx + dy * dy > r2)
                    continue;

                CastRay(map, origin.X, origin.Y, x, y, visible);
            }
        }

        return visible;
    }

    private static void CastRay(DungeonMap map, int x0, int y0, int x1, int y1, bool[,] visible)
    {
        foreach (var p in BresenhamLine(x0, y0, x1, y1))
        {
            if (!InBounds(map, p.X, p.Y))
                break;

            visible[p.X, p.Y] = true;

            if (map.IsWall(p.X, p.Y))
                break; // väggen syns men blockerar vidare
        }
    }

    // Bresenham: ger alla tiles på linjen (inkl start + mål)
    private static IEnumerable<Point> BresenhamLine(int x0, int y0, int x1, int y1)
    {
        int dx = System.Math.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -System.Math.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        int x = x0;
        int y = y0;

        while (true)
        {
            yield return new Point(x, y);

            if (x == x1 && y == y1)
                break;

            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    private static bool InBounds(DungeonMap map, int x, int y)
        => x >= 0 && x < map.Columns && y >= 0 && y < map.Rows;
}