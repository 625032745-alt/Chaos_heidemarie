using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Relics;

[RegisterRelic(typeof(Content.HeidemarieRelicPool))]
[RegisterCharacterStarterRelic(typeof(Characters.Heidemarie))]
public sealed class HeidemarieStarterRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/ui/top_panel/character_icon_heidemarie.png",
        IconOutlinePath: "res://ArtWorks/images/ui/top_panel/character_icon_heidemarie_outline.png");

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        var power = Owner.Creature.GetPower<ScarletSwordPower>();
        if (power != null && power.Amount <= 8)
            return;
        await PowerCmd.Apply<ScarletSwordPower>(choiceContext, Owner.Creature,
            5m, Owner.Creature,null);
    }
}
