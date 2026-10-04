using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Cards.Upgrade.HeroAll;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class HeroAllCard : TransformAtTurnStartCardBase
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RestKeyword.REST];

    public HeroAllCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this)
            return;
        var drawnCards = await CardPileCmd.Draw(choiceContext, 3, Owner);
        LinkForTurn(drawnCards);
    }

    protected override void OnUpgrade()
    {
        CardCmd.ApplyKeyword(this, LinkKeywords.Link);
    }


    protected override IReadOnlyList<Func<ICombatState, Player, CardModel>> CandidateCardFactories =>
    [
        static (combatState, player) => combatState.CreateCard<HeroAllUpgradeCardA>(player),
        static (combatState, player) => combatState.CreateCard<HeroAllUpgradeCardB>(player),
        static (combatState, player) => combatState.CreateCard<HeroAllUpgradeCardC>(player),
        static (combatState, player) => combatState.CreateCard<HeroAllUpgradeCardD>(player)
    ];
}