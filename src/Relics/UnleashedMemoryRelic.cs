using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Relics;

[RegisterRelic(typeof(HeidemarieRelicPool))]
public class UnleashedMemoryRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/relic/UnleashedMemoryRelic_Small.png",
        IconOutlinePath: "res://ArtWorks/images/relic/UnleashedMemoryRelic_Outline.png",
        BigIconPath: "res://ArtWorks/images/relic/UnleashedMemoryRelic_Big.png");

    private int _lastTriggerTurn;

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<InherentMemoryPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 3m, Owner.Creature,
            null);
        await PowerCmd.Apply<ScarletSwordPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 3m, Owner.Creature,
            null);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature != Owner.Creature) return;
        if (cardPlay.IsAutoPlay) return;
        if (!cardPlay.IsLastInSeries) return;
        if (Owner.PlayerCombatState is { } pcs)
        {
            var turn = pcs.TurnNumber;
            if (_lastTriggerTurn == turn) return;
            _lastTriggerTurn = turn;
            Flash();
            await PowerCmd.Apply<ScarletSwordPower>(ctx, Owner.Creature, 2m, Owner.Creature, null);
        }
    }


    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        if (power is not InherentMemoryPower || power is not ScarletSwordPower) return;
        if (power.Owner != Owner.Creature)
            return;
        if (amount > -5m)
            return;
        await CreatureCmd.Damage(choiceContext, Owner.Creature.CombatState.HittableEnemies, 15, ValueProp.Unpowered,
            Owner.Creature);
        await PlayerCmd.GainEnergy(2, Owner);
    }
}