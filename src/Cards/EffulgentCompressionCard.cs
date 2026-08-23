using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards;

[RegisterCard(typeof(HeidemarieCardPool))]
public class EffulgentCompressionCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link, CardKeyword.Unplayable,RecycleKeywords.Recycle,UniqueKeyword.Unique];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("NJCount", 3m)];

    public EffulgentCompressionCard() : base(-1, CardType.Skill, CardRarity.Quest, TargetType.AnyEnemy)
    {
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (Owner.Creature.GetPower<EffulgentCompressionPower>() != null)
            return;
        var njCount = DynamicVars["NJCount"].BaseValue;
        await PowerCmd.Apply<EffulgentCompressionPower>(choiceContext, Owner.Creature,
            njCount, Owner.Creature, this);
    }
}
