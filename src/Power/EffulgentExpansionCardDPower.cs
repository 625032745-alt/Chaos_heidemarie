using ChaosHeidemarie.Cards;
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
public class EffulgentExpansionCardDPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        var player = Owner.Player;
        if (side == CombatSide.Player)
        {
            for (var i = 0; i < 2; i++)
            {
                var newCard = combatState.CreateCard<EffulgentBladeCard>(player);
                await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Hand, player);
            }
            await PowerCmd.Remove(this);
        }
    }
}