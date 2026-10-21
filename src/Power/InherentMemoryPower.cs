using ChaosHeidemarie.Keywords;
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
public class InherentMemoryPower : ModPowerTemplate
{
    private const decimal BaseMaxAmount = 10m;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/InherentMemoryPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/InherentMemoryPower_Big.png"
    );
    
    private decimal GetMaxAmount()
    {
        var polarDayPower = Owner.GetPower<PolarDayPower>();
        return BaseMaxAmount + (polarDayPower?.Amount ?? 0m);
    }
    
    private decimal GetRemainingAmount()
    {
        return Math.Max(0m, GetMaxAmount() - Amount);
    }
    
    private async Task AddAmountWithLimit(PlayerChoiceContext choiceContext, decimal gainAmount, CardModel? cardSource = null)
    {
        if (gainAmount <= 0m)
            return;

        var actualGain = Math.Min(gainAmount, GetRemainingAmount());
        if (actualGain <= 0m)
            return;
        await PowerCmd.ModifyAmount(choiceContext, this, actualGain, Owner, cardSource);
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        await ApplyPower(choiceContext, card);
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        await ApplyPower(choiceContext, card);
    }

    private async Task ApplyPower(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card.Owner != Owner.Player) return;
        if (!card.Keywords.Contains(LinkKeywords.Link))
            return;
        if (GetRemainingAmount() <= 0m)
            return;
        Flash();
        await AddAmountWithLimit(choiceContext, 1m, card);
    }
    

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (!props.IsPoweredAttack())
            return 0m;

        if (dealer != Owner)
            return 0m;

        if (cardSource == null)
            return 0m;

        return Amount * 2m;
    }
}