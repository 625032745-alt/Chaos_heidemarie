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

    // 上一次触发的回合
    private int _lastTriggerTurn = -1;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        // 只响应技能牌
        if (card.Type != CardType.Skill)
            return;
        // 只响应能力拥有者打出的牌
        if (Owner.Player == null || card.Owner != Owner.Player)
            return;
        var combatState = Owner.Player.PlayerCombatState;
        if (combatState == null)
            return;
        var currentTurn = combatState.TurnNumber;
        // 每回合只能触发一次
        if (_lastTriggerTurn == currentTurn)
            return;
        // 先记录回合，避免重复触发
        _lastTriggerTurn = currentTurn;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null);
    }
}