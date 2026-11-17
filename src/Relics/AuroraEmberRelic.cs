using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Relics;

[RegisterRelic(typeof(HeidemarieRelicPool))]
public class AuroraEmberRelic : ModRelicTemplate
{
    private int _lastTurn = -1;

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(12m, ValueProp.Unpowered)];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/relic/AuroraEmberRelic_Small.png",
        IconOutlinePath: "res://ArtWorks/images/relic/AuroraEmberRelic_Outline.png",
        BigIconPath: "res://ArtWorks/images/relic/AuroraEmberRelic_Big.png"
    );

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext,
        PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not InherentMemoryPower || amount >= 0m)
            return;
        if (!ReferenceEquals(power.Owner, Owner.Creature))
            return;

        var turn = Owner.Creature.Player?.PlayerCombatState?.TurnNumber ?? -1;
        if (turn == _lastTurn)
            return;
        _lastTurn = turn;

        Flash();
        var block = Math.Min(DynamicVars.Block.BaseValue, -amount);
        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Unpowered, null);
    }
}