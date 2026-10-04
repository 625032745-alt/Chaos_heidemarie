using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Cards.Upgrade.EffulgentExpansion;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Rare;

[RegisterCard(typeof(HeidemarieCardPool))]
public class EffulgentExpansionCard : TransformAtTurnStartCardBase
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [RestKeyword.REST];

    public EffulgentExpansionCard() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.GetPower<EffulgentExpansionPower>() != null)
            return;
        await PowerCmd.Apply<EffulgentExpansionPower>(choiceContext, Owner.Creature,
            1m, Owner.Creature, this);
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card != this)
            return;
        var combatState = card.CombatState;
        if (combatState == null)
            return;
        await CardGenerationHelper.GenerateEffulgentBlades(this, 2);
    }

    protected override void OnUpgrade()
    {
        CardCmd.ApplyKeyword(this, LinkKeywords.Link);
    }
    
    protected override IReadOnlyList<Func<ICombatState, Player, CardModel>> CandidateCardFactories => 
    [
        static (combatState, player) => combatState.CreateCard<EffulgentExpansionCardA>(player),
        static (combatState, player) => combatState.CreateCard<EffulgentExpansionCardB>(player),
        static (combatState, player) => combatState.CreateCard<EffulgentExpansionCardC>(player),
        static (combatState, player) => combatState.CreateCard<EffulgentExpansionCardD>(player)
    ];
}
