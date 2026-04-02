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
    public char PlayerSymbol { get; private set; }

    public List<(char Type, Vector2 Position)> Entities { get; } = new();

    private static readonly int[] _wallBitmaskToTile = new int[16]
    {
        31,  //  0  □  Isolated — no wall neighbors
        6,  //  1  ╨  North only
        14,  //  2  ╞  East only
        8,  //  3  ╚  North + East
        13,  //  4  ╥  South only
        2,  //  5  ║  North + South (vertical)
        0,  //  6  ╔  East + South >XXXX<
        11,  //  7  ╠  North + East + South (T right)
        5,  //  8  ╡  West only
        9,  //  9  ╝  North + West
        10, // 10  ═  East + West (horizontal) 
        4, // 11  ╩  North + East + West (T up)
        1, // 12  ╗  South + West
        12, // 13  ╣  North + South + West (T left)
        3, // 14  ╦  East + South + West (T down)
        7, // 15  ╬  All four sides (cross)
    };

    private static readonly int[] _combatWallBitmaskToTile = new int[2]
    {
        22, 29
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

    public void LoadMap(ContentManager content, string level)
    {
        string filePath = Path.Combine(content.RootDirectory, $"LevelFiles/{level}.txt");
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
                    PlayerSymbol = c;
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

    public void Draw(SpriteBatch spriteBatch, bool isCombat = false)
    {
        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
            {
                char cell = _grid[x, y];
                if (cell == VOID) continue;

                Vector2 position = new Vector2((float)x * TileSize, (float)y * TileSize);

                if (cell == WALL && isCombat == false)
                {
                    int bitmask = ComputeWallBitmask(x, y);
                    TextureRegion tile = _wallTileset.GetTile(_wallBitmaskToTile[bitmask]);
                    tile.Draw(spriteBatch, position, Color.White);
                }
                else if (cell == WALL && isCombat == true)
                {
                    int bitmask = ComputeWallBitmask(x, y);
                    int combatTileIndex = PickCombatWallTileIndex(bitmask);
                    TextureRegion tile = _wallTileset.GetTile(combatTileIndex);
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

    private static int PickCombatWallTileIndex(int bitmask)
    {
        bool hasNorth = (bitmask & 1) != 0;
        bool hasSouth = (bitmask & 4) != 0;

        if (!hasNorth) return 22; // top cap
        if (!hasSouth) return 29; // bottom cap
        return 22;                // middle
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
}