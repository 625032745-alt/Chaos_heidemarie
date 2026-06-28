using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Relics;

public sealed class HeidemarieStarterRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/ui/top_panel/character_icon_heidemarie.png",
        IconOutlinePath: "res://ArtWorks/images/ui/top_panel/character_icon_heidemarie_outline.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(4m)
    ];

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (Owner.Creature.IsDead)
            return;

        Flash();
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, true);
    }
}
