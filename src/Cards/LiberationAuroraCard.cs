using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards;

[RegisterCard(typeof(HeidemarieCardPool))]
public class LiberationAuroraCard : ModCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20, ValueProp.Move)];
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust,UniqueKeyword.Unique];

    public LiberationAuroraCard() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        var player = card.Owner;
        var playerCombatState = player.PlayerCombatState;
        if (playerCombatState == null)
            return;
        var cardsToExhaust = playerCombatState.Hand.Cards
            .Where(c => c is EffulgentBladeCard && c != card)
            .ToList();
        int exhaustedCount = 0;
        foreach (var targetCard in cardsToExhaust)
        {
            await CardCmd.Exhaust(choiceContext, targetCard);
            exhaustedCount++;
        }

        var baseValue = DynamicVars.Damage.BaseValue;
        await DamageCmd.Attack(baseValue + exhaustedCount * 5)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        var effulgentCompressionCard = card.CombatState.CreateCard<EffulgentCompressionCard>(player);
        await CardCmd.Transform(card, effulgentCompressionCard);
    }
}
