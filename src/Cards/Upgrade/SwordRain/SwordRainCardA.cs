using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.SwordRain;

[RegisterCard(typeof(HeidemarieCardPool))]
public class SwordRainCardA : ModCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(13, ValueProp.Move)];
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/SwordRainCard.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link, RestKeyword.REST];

    public SwordRainCardA() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card != this)
            return;
        var player = card.Owner;
        var combatState = card.CombatState;
        if (null == combatState)
            return;
        var playerCombatState = card.Owner.PlayerCombatState;
        if (playerCombatState == null)
            return;
        var count = playerCombatState.Hand.Cards.Count(c => c is EffulgentBladeCard);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount(count)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        
        var newCard = combatState.CreateCard<EffulgentBladeCard>(player);
        await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Hand, player);
    }
}
