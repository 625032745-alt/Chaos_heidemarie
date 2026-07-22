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
public class SwordRainCardD : ModCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(30, ValueProp.Move)];
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/SwordRainCard.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust,CardKeyword.Retain,ConcludeKeywords.Conclude];
    
    public SwordRainCardD() : base(3, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        var player = card.Owner;
        var playerCombatState = player.PlayerCombatState;
        var cards = playerCombatState.Hand.Cards.Where(c => c is LiberationAuroraCard).ToList();
        foreach (var cardModel in cards)
        {
           await CardCmd.Exhaust(choiceContext,cardModel);
        }
        CardPile pile = PileType.Exhaust.GetPile(Owner);
        var cardsCount = pile.Cards.Count(c => c is EffulgentBladeCard);
        var baseValue = DynamicVars.Damage.BaseValue;
        await DamageCmd.Attack(baseValue + cardsCount * 3)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(card.CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }
}
