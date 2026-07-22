using ChaosHeidemarie.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.SwordRain;

[RegisterCard(typeof(HeidemarieCardPool))]
public class SwordRainCardB : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/SwordRainCard.png");

    public SwordRainCardB() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardPile pile = PileType.Exhaust.GetPile(Owner);
        var cards = pile.Cards.Where(c => c is EffulgentBladeCard).ToList();
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (i >= 5) break;
            Creature target = Owner.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
            await CardCmd.AutoPlay(choiceContext, card, target);
        }
    }
}
