using static Dungeoneer.GameObjects.Bases.ActorBase;

namespace Dungeoneer.GameObjects.Helpers;

public static class CombatActionResultExtensions
{
    public static string ToLogLine(this CombatActionResult result, CombatEncounter encounter)
    {
        string actor = GetActorName(encounter, result.ActorEntityId);
        string target = GetActorName(encounter, result.TargetEntityId);

        if (result.SkillName == "Consume" && result.ConsumeSucceeded == true)
            return $"{actor} consumed {target} for {result.DamageDealt} damage and healed {result.HealingDealt} HP!";

        if (result.SkillName == "Consume" && result.ConsumeSucceeded == false)
            return $"{actor} failed to fully consume {target}, dealing {result.DamageDealt} damage and healing {result.HealingDealt} HP!";

        return (result.AttackerAction, result.DefenderAction, result.Outcome) switch
        {
            (CombatActionType.Defend, _, _) =>
                $"{actor} took a defensive stance.",

            (_, _, CombatOutcomeKind.Rest) =>
                $"{actor} and {target} both took defensive action.",

            (CombatActionType.Skill, _, CombatOutcomeKind.Blocked) =>
                $"{actor} used a skill but was blocked by {target}!",

            (CombatActionType.Skill, _, CombatOutcomeKind.Hit) =>
                $"{actor} used a skill and {result.Outcome} {target} for {result.DamageDealt} damage!",

            (_, CombatActionType.Defend, CombatOutcomeKind.Blocked) =>
                $"{actor} {result.AttackerAction} and was blocked by {target}!",

            (_, CombatActionType.Defend, CombatOutcomeKind.Hit) =>
                $"{actor} {result.AttackerAction} and {result.Outcome} {target} for {result.DamageDealt} damage! {target} was defending!",

            (_, _, CombatOutcomeKind.Blocked) =>
                $"{actor} {result.AttackerAction} and was blocked by {target}!",

            (_, _, CombatOutcomeKind.Hit) =>
                $"{actor} {result.AttackerAction} and {result.Outcome} {target} for {result.DamageDealt} damage!",

            _ =>
                $"{actor} acted against {target}."
        };
    }

    private static string GetActorName(CombatEncounter encounter, int entityId) =>
        encounter.Player.EntityId == entityId
            ? encounter.Player.ActorName
            : encounter.Monster.ActorName;
}