using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using ChaosHeidemarie.Content;
using STS2RitsuLib.Scaffolding.Characters;

namespace ChaosHeidemarie.Characters;

public sealed class Heidemarie : ModCharacterTemplate<HeidemarieCardPool, HeidemarieRelicPool, HeidemariePotionPool>
{
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override Color NameColor => new("8E5AC8FF");
    public override int StartingHp => 72;
    public override int StartingGold => 99;
    public override float AttackAnimDelay => 0.15f;
    public override float CastAnimDelay => 0.25f;
    public override Color EnergyLabelOutlineColor => new("42245FFF");
    public override Color DialogueColor => new("40264F");
    public override VfxColor SpeechBubbleColor => VfxColor.Purple;
    public override Color MapDrawingColor => new("8E5AC8FF");
    public override Color RemoteTargetingLineColor => new("B37BE5FF");
    public override Color RemoteTargetingLineOutline => new("42245FFF");

    public override string? PlaceholderCharacterId => null;
    public override bool RequiresEpochAndTimeline => false;
    public override CharacterAssetProfile AssetProfile => new(
        Scenes: new CharacterSceneAssetSet(
            VisualsPath: "res://ArtWorks/scenes/creature_visuals/heidemarie.tscn",
            EnergyCounterPath: "res://scenes/combat/energy_counters/heidemarie_energy_counter.tscn",
            MerchantAnimPath: "res://ArtWorks/scenes/merchant/characters/heidemarie_merchant.tscn",
            RestSiteAnimPath: "res://ArtWorks/scenes/rest_site/characters/heidemarie_rest_site.tscn"),
        Ui: new CharacterUiAssetSet(
            IconTexturePath: "res://ArtWorks/images/ui/top_panel/character_icon_heidemarie.png",
            IconOutlineTexturePath: "res://ArtWorks/images/ui/top_panel/character_icon_heidemarie_outline.png",
            IconPath: "res://scenes/ui/character_icons/heidemarie_icon.tscn",
            CharacterSelectBgPath: "res://ArtWorks/scenes/screens/char_select/char_select_bg_heidemarie.tscn",
            CharacterSelectIconPath: "res://ArtWorks/images/packed/character_select/char_select_heidemarie.png",
            CharacterSelectLockedIconPath: "res://ArtWorks/images/packed/character_select/char_select_heidemarie_locked.png",
            CharacterSelectTransitionPath: "res://materials/transitions/heidemarie_transition_mat.tres",
            MapMarkerPath: "res://images/packed/map/icons/map_marker_heidemarie.png"),
        Vfx: new CharacterVfxAssetSet(
            TrailPath: "res://scenes/vfx/card_trail_heidemarie.tscn"),
        Spine: new CharacterSpineAssetSet(
            CombatSkeletonDataPath: "res://ArtWorks/modspine/characters/heidemarie/heidemarie_skel_data.tres"),
        Audio: new CharacterAudioAssetSet(
            CharacterSelectSfx: "event:/sfx/characters/heidemarie/heidemarie_select",
            CharacterTransitionSfx: "event:/sfx/ui/wipe_heidemarie",
            AttackSfx: "event:/sfx/characters/heidemarie/heidemarie_attack",
            CastSfx: "event:/sfx/characters/heidemarie/heidemarie_cast",
            DeathSfx: "event:/sfx/characters/heidemarie/heidemarie_die"));

    public override List<string> GetArchitectAttackVfx()
    {
        return
        [
            "vfx/vfx_attack_slash",
            "vfx/vfx_attack_blunt",
            "vfx/vfx_heavy_blunt"
        ];
    }
}
