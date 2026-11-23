using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class ScarletSalvagePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/ScarletSalvagePower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/ScarletSalvagePower_Big.png"
    );

    private int _lastTriggerTurn = -1;

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        // 只处理自己丢弃的卡牌
        if (Owner.Player == null || card.Owner != Owner.Player)
            return;

        // 连接牌不触发
        if (card.Keywords.Contains(LinkKeywords.Link))
            return;

        var combatState = Owner.Player.PlayerCombatState;
        if (combatState == null)
            return;

        var currentTurn = combatState.TurnNumber;

        // 每回合只能触发一次
        if (_lastTriggerTurn == currentTurn)
            return;

        // 先记录触发，避免抽牌引发的其他事件重复触发
        _lastTriggerTurn = currentTurn;

        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
    }
}