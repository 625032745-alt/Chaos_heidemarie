using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Cards.Token;
using ChaosHeidemarie.Cards.Upgrade.SwordRain;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
[RegisterCharacterStarterCard(typeof(Characters.Heidemarie))]
public class SwordRainCard : TransformAtTurnStartCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move)];
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link];


    public SwordRainCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override void OnUpgrade()
    {
        CardCmd.ApplyKeyword(this, RestKeyword.REST);
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
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        var count = card.IsUpgraded ? 2 : 1;
        for (var i = 0; i < count; i++)
        {
            var newCard = combatState.CreateCard<EffulgentBladeCard>(player);
            await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Hand, player);
        }
    }

    protected override Type[] GetCandidateCardTypes()
    {
        return
        [
            typeof(SwordRainCardA),
            typeof(SwordRainCardB),
            typeof(SwordRainCardC),
            typeof(SwordRainCardD)
        ];
    }
}
