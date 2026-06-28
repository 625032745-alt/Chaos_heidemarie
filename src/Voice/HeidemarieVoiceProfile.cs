using System.Collections.Generic;

namespace ChaosHeidemarie.Voice;

internal static class HeidemarieVoiceProfile
{
    public const string VoiceBasePath = "res://ArtWorks/vkrid/heidemarie_v/";

    public const string VoiceSelect = VoiceBasePath + "vo_heidemarie_select_01.ogg";
    public const string VoiceRest = VoiceBasePath + "vo_heidemarie_rest_01.ogg";
    public const string VoiceDefeat = VoiceBasePath + "vo_heidemarie_defeat_01.ogg";
    public const string VoiceHit = VoiceBasePath + "vo_heidemarie_hit_01.ogg";

    public static readonly IReadOnlyList<string> VoiceCombatStartCandidates =
    [
        VoiceBasePath + "vo_heidemarie_start_01.ogg"
    ];

    public static readonly IReadOnlyList<string> VoiceVictoryCandidates =
    [
        VoiceBasePath + "vo_heidemarie_victory_01.ogg"
    ];

    public static readonly IReadOnlyList<string> VoiceAttackCandidates =
    [
        VoiceBasePath + "vo_heidemarie_attack_01.ogg",
        VoiceBasePath + "vo_heidemarie_attack_02.ogg",
        VoiceBasePath + "vo_heidemarie_attack_03.ogg"
    ];

    public static readonly IReadOnlyList<string> VoiceDefendCandidates =
    [
        VoiceBasePath + "vo_heidemarie_defend_01.ogg",
        VoiceBasePath + "vo_heidemarie_defend_02.ogg"
    ];

    public static readonly IReadOnlyList<string> VoiceSkillCandidates =
    [
        VoiceBasePath + "vo_heidemarie_debuff_01.ogg",
        VoiceBasePath + "vo_heidemarie_debuff_02.ogg",
        VoiceBasePath + "vo_heidemarie_buff_02.ogg"
    ];

    public static readonly IReadOnlyList<string> VoicePowerCandidates =
    [
        VoiceBasePath + "vo_heidemarie_buff_01.ogg",
        VoiceBasePath + "vo_heidemarie_buff_02.ogg"
    ];

    public static readonly IReadOnlyList<string> VoicePrewarmCandidates =
    [
        VoiceSelect,
        VoiceRest,
        VoiceDefeat,
        VoiceHit,
        .. VoiceCombatStartCandidates,
        .. VoiceVictoryCandidates,
        .. VoiceAttackCandidates,
        .. VoiceDefendCandidates,
        .. VoiceSkillCandidates,
        .. VoicePowerCandidates
    ];
}
