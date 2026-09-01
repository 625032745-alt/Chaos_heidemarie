using ChaosHeidemarie.Cards.Token;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.EffulgentExpansion;

[RegisterCard(typeof(HeidemarieCardPool))]
public class EffulgentExpansionCardA : ModCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("JGCount", 2m)];
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/EffulgentExpansionCard.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [RestKeyword.REST];

    public EffulgentExpansionCardA() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        var player = card.Owner;
        var combatState = card.CombatState;
        for (var i = 0; i < 2; i++)
        {
            var newCard = combatState.CreateCard<EffulgentBladeCard>(player);
            await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Hand, player);
        }
        var jgCount = DynamicVars["JGCount"].BaseValue;
        await PowerCmd.Apply<EffulgentExpansionCardAPower>(choiceContext, Owner.Creature,
            jgCount, Owner.Creature, this);
    }
}