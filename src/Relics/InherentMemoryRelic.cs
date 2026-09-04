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
public class InherentMemoryRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/relic/InherentMemoryRelic_Small.png",
        IconOutlinePath: "res://ArtWorks/images/relic/InherentMemoryRelic_Outline.png",
        BigIconPath: "res://ArtWorks/images/relic/InherentMemoryRelic_Big.png");

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            Flash();
            await PowerCmd.Apply<InherentMemoryPower>(choiceContext, Owner.Creature,
                1m, Owner.Creature, null);
        }
    }
}