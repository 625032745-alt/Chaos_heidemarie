using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class QuietRadiancePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/QuietRadiancePower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/QuietRadiancePower_Big.png"
    );

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Type != CardType.Skill)
            return;
        if (Owner.Player == null || card.Owner != Owner.Player)
            return;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, cardPlay);
    }
}