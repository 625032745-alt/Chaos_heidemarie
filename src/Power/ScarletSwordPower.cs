using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class ScarletSwordPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/ScarletSwordPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/ScarletSwordPower_Big.png"
    );

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
        if (card.Keywords.Contains(LinkKeywords.Link))
        {
            var player = card.Owner;
            var combatState = player.Creature.CombatState;
            if (null == combatState)
                return;
            await DamageCmd.Attack(6m)
                .FromCard(card, null)
                .TargetingRandomOpponents(combatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await PowerCmd.ModifyAmount(choiceContext, this, -1m, null, card);
        }
    }
}