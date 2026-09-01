using ChaosHeidemarie.Cards;
using ChaosHeidemarie.Cards.Token;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class EffulgentExpansionCardAPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/EffulgentExpansionCardAPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/EffulgentExpansionCardAPower_Big.png"
    );

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (cardSource is not EffulgentBladeCard)
        {
            return 0m;
        }

        return 2m;
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card is EffulgentBladeCard)
        {
            Flash();
            await PowerCmd.ModifyAmount(choiceContext, this, -1m, null, card);
        }
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card is EffulgentBladeCard)
        {
            Flash();
            await PowerCmd.ModifyAmount(choiceContext, this, -1m, null, card);
        }
    }
}