using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using System.Collections.Generic;
using System.IO;

namespace Dungeoneer.Maps;

public class DungeonMap
{
    public const char WALL = '#';
    public const char FLOOR = '.';
    public const char VOID = ' ';

    private char[,] _grid;
    private Tileset _wallTileset;
    private Tileset _floorTileset;

    public int Columns { get; private set; }
    public int Rows { get; private set; }
    public int TileSize { get; }

    public Vector2 PlayerStart { get; private set; }
    public List<(char Type, Vector2 Position)> Entities { get; } = new();

    public bool DebugDrawTileset { get; set; } = false;

    private static readonly int[] _wallBitmaskToTile = new int[16]
    {
        0,   // (0110) ╔  East + South
        1,   // (1100) ╗  South + West
        2,   // (0101) ║  North + South (vertical)
        3,   // (1110) ╦  East + South + West (T down)
        4,   // (1011) ╩  North + East + West (T up)
        5,   //  (1000) ╡  West only
        6,   //  (0001) ╨  North only
        7,   // (1111) ╬  All four sides (cross)
        8,   // (0011) ╚  North + East
        9,   // (1001) ╝  North + West
        10,  // (1010) ═  East + West (horizontal) 
        11,  // (0111) ╠  North + East + South (T right)
        12,  // (1101) ╣  North + South + West (T left)
        13,  // (0100) ╥  South only
        14,  // (0010) ╞  East only
        31,  // (0000) □  Isolated — no wall neighbors
    };

    private static readonly int[] _floorTileVariants = { 0, 1, 2, 3 };

    public DungeonMap(int tileSize = 64)
    {
        TileSize = tileSize;
    }

    public void LoadContent(ContentManager content, string texturePath)
    {
        Texture2D texture = content.Load<Texture2D>(texturePath);

        TextureRegion floorRegion = new TextureRegion(texture, 0, 0, 512, 384);
        _floorTileset = new Tileset(floorRegion, TileSize, TileSize);

        TextureRegion wallRegion = new TextureRegion(texture, 0, 384, 512, 384);
        _wallTileset = new Tileset(wallRegion, TileSize, TileSize);
    }

    public void LoadMap(ContentManager content, string mapPath)
    {
        string filePath = Path.Combine(content.RootDirectory, mapPath);
        string[] lines;

        using (Stream stream = TitleContainer.OpenStream(filePath))
        using (StreamReader reader = new StreamReader(stream))
        {
            lines = reader.ReadToEnd().TrimEnd().Split('\n');
        }

        Rows = lines.Length;
        Columns = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].TrimEnd('\r');
            if (lines[i].Length > Columns)
                Columns = lines[i].Length;
        }

        _grid = new char[Columns, Rows];
        Entities.Clear();

        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
            {
                char c = x < lines[y].Length ? lines[y][x] : VOID;

                if (c == '@')
                {
                    PlayerStart = new Vector2(x * TileSize, y * TileSize);
                    _grid[x, y] = FLOOR;
                }
                else if (c != WALL && c != FLOOR && c != VOID)
                {
                    Entities.Add((c, new Vector2(x * TileSize, y * TileSize)));
                    _grid[x, y] = FLOOR;
                }
                else
                {
                    _grid[x, y] = c;
                }
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
            {
                char cell = _grid[x, y];
                if (cell == VOID) continue;

                Vector2 position = new Vector2((float)x * TileSize, (float)y * TileSize);

                if (cell == WALL)
                {
                    int bitmask = ComputeWallBitmask(x, y);
                    TextureRegion tile = _wallTileset.GetTile(_wallBitmaskToTile[bitmask]);
                    tile.Draw(spriteBatch, position, Color.White);
                }
                else
                {
                    int variation = ((x * 7) + (y * 13)) % _floorTileVariants.Length;
                    TextureRegion tile = _floorTileset.GetTile(_floorTileVariants[variation]);
                    tile.Draw(spriteBatch, position, Color.White);
                }
            }
        }
    }

    private int ComputeWallBitmask(int x, int y)
    {
        int bitmask = 0;
        if (IsWall(x, y - 1)) bitmask |= 1;
        if (IsWall(x + 1, y)) bitmask |= 2;
        if (IsWall(x, y + 1)) bitmask |= 4;
        if (IsWall(x - 1, y)) bitmask |= 8;
        return bitmask;
    }

    public bool IsWall(int x, int y)
    {
        if (x < 0 || x >= Columns || y < 0 || y >= Rows) return false;
        return _grid[x, y] == WALL;
    }

    public bool IsFloor(int x, int y)
    {
        if (x < 0 || x >= Columns || y < 0 || y >= Rows) return false;
        return _grid[x, y] == FLOOR;
    }

    public bool IsWalkable(Vector2 worldPosition)
    {
        int tileX = (int)(worldPosition.X / TileSize);
        int tileY = (int)(worldPosition.Y / TileSize);
        return IsFloor(tileX, tileY);
    }

    /// <summary>
    /// Debug: ritar ut hela vägg-tilesetet i ett rutnät + index-text.
    /// Körs bäst i ett hörn av skärmen.
    /// </summary>
    public void DrawWallTilesetDebug(SpriteBatch spriteBatch, SpriteFont font, Vector2 origin, int padding = 6)
    {
        for (int i = 0; i < _wallTileset.Count; i++)
        {
            int col = i % _wallTileset.Columns;
            int row = i / _wallTileset.Columns;
            Vector2 pos = origin + new Vector2(col * (TileSize + padding), row * (TileSize + padding));
            _wallTileset.GetTile(i).Draw(spriteBatch, pos, Color.White);
            spriteBatch.DrawString(font, i.ToString(), pos + new Vector2(2, 2), Color.Yellow);
        }
    }
}