using ChaosHeidemarie.Cards.Common;
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
public class AuroraPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/AuroraPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/AuroraPower_Big.png"
    );

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            var player = Owner.Player;
            if (player == null) return;
            var playerCombatState = player.PlayerCombatState;
            if (playerCombatState == null) return;
            var cards = playerCombatState.DiscardPile.Cards.Where(c => c is AuroraCard).ToList();
            if (cards.Count > 0)
            {
                foreach (var card in cards)
                {
                    await CardPileCmd.Add(card, PileType.Draw);
                }
            }
        }
    }
}