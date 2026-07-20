using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
public class EffulgentExpansionCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://images/cards/{GetType().Name}.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [RestKeyword.REST];
    public static bool RemainingCharges;
    private int _handCardCount;

    public EffulgentExpansionCard() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card != this)
            return Task.CompletedTask;
        var player = card.Owner;
        var playerCombatState = player.PlayerCombatState;
        if (null != playerCombatState)
        {
            _handCardCount = playerCombatState.Hand.Cards.Count(c => c.Keywords.Contains(LinkKeywords.Link));
        }

        return Task.CompletedTask;
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
        if (card.IsUpgraded)
        {
            for (var i = 0; i < _handCardCount; i++)
            {
                var newCard = combatState.CreateCard<EffulgentBladeCard>(player);
                await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Hand, player);
            }
            return;
        }

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
        _handCardCount = 0;
        RemainingCharges = false;
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        CardCmd.RemoveKeyword(this, RestKeyword.REST);
        CardCmd.ApplyKeyword(this, LinkKeywords.Link);
    }
}