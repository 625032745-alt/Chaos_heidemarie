using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class ChainlightTrailPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/ChainlightTrailPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/ChainlightTrailPower_Big.png"
    );

    // 记录上一次触发的回合
    private int _lastTriggerTurn = -1;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        // 只响应自己打出的卡牌
        if (Owner.Player == null || card.Owner != Owner.Player)
            return;
        // 必须是连接牌
        if (!card.Keywords.Contains(LinkKeywords.Link))
            return;
        var combatState = Owner.Player.PlayerCombatState;
        if (combatState == null)
            return;
        var currentTurn = combatState.TurnNumber;
        // 本回合已经触发
        if (_lastTriggerTurn == currentTurn)
            return;
        // 先记录触发回合，避免重复触发
        _lastTriggerTurn = currentTurn;
        Flash();
        await CardPileCmd.Draw(choiceContext, Owner.Player);
    }
}