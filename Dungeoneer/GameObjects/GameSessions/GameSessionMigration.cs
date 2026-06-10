using System.Collections.Generic;
using System.Linq;

namespace Dungeoneer.GameObjects.GameSessions;

public static class GameSessionMigration
{
    public const int CurrentSaveVersion = 2;

    public static bool Migrate(GameSession session)
    {
        if (session == null)
            return false;

        bool modified = false;

        if (session.Player == null)
        {
            session.Player = new PlayerSessionState();
            modified = true;
        }

        if (session.Player.SkillCooldowns == null)
        {
            session.Player.SkillCooldowns = new Dictionary<int, int>();
            modified = true;
        }

        var skills = session.Player.Skills;
        bool needsSkillRepair =
            skills == null ||
            skills.Count == 0 ||
            skills.Any(s => string.IsNullOrWhiteSpace(s.Name));

        if (needsSkillRepair)
        {
            session.Player.Skills = CreateDefaultSkills(session.Player.CurrentLevel);
            modified = true;
        }
        else
        {
            if (EnsureConsumeForLevel(session.Player))
                modified = true;

            if (NormalizeSkillIds(session.Player.Skills))
                modified = true;
        }

        if (session.SaveVersion < CurrentSaveVersion)
        {
            session.SaveVersion = CurrentSaveVersion;
            modified = true;
        }

        return modified;
    }

    private static List<SkillState> CreateDefaultSkills(int currentLevel)
    {
        var list = new List<SkillState>
        {
            new() { Name = "Defend!", Id = 0 },
            new() { Name = "Bite!", Id = 1 },
        };

        if (currentLevel >= 5)
            list.Add(new() { Name = "Consume!", Id = 2 });

        return list;
    }

    private static bool EnsureConsumeForLevel(PlayerSessionState player)
    {
        if (player.CurrentLevel < 5)
            return false;

        if (player.Skills.Any(s => s.Name == "Consume!"))
            return false;

        player.Skills.Add(new SkillState { Name = "Consume!", Id = player.Skills.Count });
        return true;
    }

    private static bool NormalizeSkillIds(List<SkillState> skills)
    {
        bool modified = false;
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i].Id == i)
                continue;

            skills[i].Id = i;
            modified = true;
        }
        return modified;
    }
}
