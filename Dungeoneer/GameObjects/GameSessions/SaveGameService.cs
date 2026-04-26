using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dungeoneer.GameObjects.GameSessions;

public class SaveGameService
{
    private JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        IncludeFields = false,
        WriteIndented = true,
        PropertyNamingPolicy = null,
        Converters = { new Vector2JsonConverter() }
    };

    public void SaveGame(GameSession gameSession, string saveFileName = "savegame")
    {
        var json = JsonSerializer.Serialize(gameSession, jsonOptions);

        var dir = Path.Combine(AppContext.BaseDirectory, "savegames");
        Directory.CreateDirectory(dir);

        var filePath = Path.Combine(dir, $"{saveFileName}.json");
        File.WriteAllText(filePath, json);
    }

    public GameSession LoadGame(string saveFileName = "savegame")
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "savegames");
        Directory.CreateDirectory(dir);

        var filePath = Path.Combine(dir, $"{saveFileName}.json");

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<GameSession>(json, jsonOptions);
    }

    public bool SaveGameExists(string saveFileName = "savegame")
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "savegames");
        Directory.CreateDirectory(dir);
        var filePath = Path.Combine(dir, $"{saveFileName}.json");
        return File.Exists(filePath);
    }
}

public sealed class Vector2JsonConverter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected object for Vector2.");
        float x = 0f, y = 0f;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new Vector2(x, y);
            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;
            string? name = reader.GetString();
            reader.Read();
            if (string.Equals(name, "X", StringComparison.OrdinalIgnoreCase))
                x = (float)reader.GetDouble();
            else if (string.Equals(name, "Y", StringComparison.OrdinalIgnoreCase))
                y = (float)reader.GetDouble();
            else
                reader.Skip();
        }
        throw new JsonException("Incomplete Vector2.");
    }
    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }
}