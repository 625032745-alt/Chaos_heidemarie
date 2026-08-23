using ChaosHeidemarie.Cards;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class EffulgentCompressionPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/EffulgentCompressionPower.png",
        BigIconPath: "res://ArtWorks/images/power/EffulgentCompressionPower.png"
    );

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card is not EffulgentCompressionCard)
            return;
        var player = card.Owner;
        var combatState = card.CombatState;
        if (combatState == null)
            return;
        await PowerCmd.ModifyAmount(choiceContext, this, -1m, null, card);
        if (Amount <= 0)
        {
            var targetCard = combatState.CreateCard<LiberationAuroraCard>(player);
            card.RemoveKeyword(RecycleKeywords.Recycle);
            await CardCmd.Transform(card, targetCard);
            await PowerCmd.Remove(this);
        }
    }
}