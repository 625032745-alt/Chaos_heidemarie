using System.Collections.Generic;
using System.Threading.Tasks;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards;

[RegisterCard(typeof(HeidemarieCardPool))]
public class EffulgentCompressionCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link, CardKeyword.Unplayable,RecycleKeywords.Recycle,UniqueKeyword.Unique];
    private int _count = 0;

    public EffulgentCompressionCard() : base(-1, CardType.Skill, CardRarity.Quest, TargetType.AnyEnemy)
    {
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card != this)
            return;
        _count += 1;
        var player = card.Owner;
        var combatState = card.CombatState;
        if (combatState == null)
            return;
        if (_count >= 3)
        {
            var targetCard = combatState.CreateCard<LiberationAuroraCard>(player);
            card.RemoveKeyword(RecycleKeywords.Recycle);
            await CardCmd.Transform(card, targetCard);
            _count = 0;
        }
    }
}
