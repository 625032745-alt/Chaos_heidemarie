using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Cards.Upgrade.EffulgentExpansion;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards;

[RegisterCard(typeof(HeidemarieCardPool))]
public class EffulgentExpansionCard : TransformAtTurnStartCardBase
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [RestKeyword.REST];
    public static bool RemainingCharges;

    public EffulgentExpansionCard() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card != this)
            return;
        var player = card.Owner;
        var combatState = card.CombatState;
        if (combatState == null)
            return;
        RemainingCharges = true;
        for (var i = 0; i < 2; i++)
        {
            var newCard = combatState.CreateCard<EffulgentBladeCard>(player);
            await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Hand, player);
        }
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        RemainingCharges = false;
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        CardCmd.ApplyKeyword(this, LinkKeywords.Link);
    }

    protected override Type[] GetCandidateCardTypes()
    {
        return
        [
            typeof(EffulgentExpansionCardB),
            typeof(EffulgentExpansionCardC),
            typeof(EffulgentExpansionCardD)
        ];
    }
}
