using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class ScarletArrayPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: "res://ArtWorks/images/power/ScarletArrayPower_Small.png",
            BigIconPath: "res://ArtWorks/images/power/ScarletArrayPower_Big.png"
        );

    private bool _activatedThisTurn;
    private int _extraAttackCount;


    public async Task OnScarletSwordAttack(PlayerChoiceContext choiceContext, ScarletSwordPower swordPower,
        CardModel? card)
    {
        if (!_activatedThisTurn)
        {
            if (swordPower.Amount < 2m)
                return;
            _activatedThisTurn = true;
            _extraAttackCount = Amount;
            Flash();
            await swordPower.ConsumeAsync(choiceContext, 2m, Owner, card);
            return;
        }

        if (_extraAttackCount <= 0)
            return;
        _extraAttackCount--;
        Flash();
        await swordPower.Damage(choiceContext, card, isExtraAttack: true);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner))
        {
            return Task.CompletedTask;
        }

        _activatedThisTurn = false;
        _extraAttackCount = 0;

        return Task.CompletedTask;
    }
}