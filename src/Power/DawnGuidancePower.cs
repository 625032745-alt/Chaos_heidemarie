using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class DawnGuidancePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/DawnGuidancePower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/DawnGuidancePower_Big.png"
    );

    private decimal _grantedThisTurn;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Type != CardType.Attack) return;
        if (Owner?.Player == null || card.Owner != Owner.Player) return;

        var before = Owner.GetPower<InherentMemoryPower>()?.Amount ?? 0m;

        await PowerCmd.Apply<InherentMemoryPower>(choiceContext, Owner, Amount, Owner, card);

        // 上限钳制可能少给，按实际差额记账
        var after = Owner.GetPower<InherentMemoryPower>()?.Amount ?? 0m;
        if (after > before) _grantedThisTurn += after - before;
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;

        if (_grantedThisTurn > 0m)
        {
            var memory = Owner.GetPower<InherentMemoryPower>();
            if (memory != null)
                await PowerCmd.ModifyAmount(choiceContext, memory, -_grantedThisTurn, Owner, null);
        }

        _grantedThisTurn = 0m;
        await PowerCmd.Remove(this);
    }
}