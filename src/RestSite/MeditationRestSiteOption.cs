using ChaosHeidemarie.Cards.Rare;
using ChaosHeidemarie.Cards.Uncommon;
using ChaosHeidemarie.Cards.Upgrade.SwordRain;
using ChaosHeidemarie.Cards.Upgrade.ThreadLight;
using ChaosHeidemarie.Cards.Upgrade.HeroAll;
using ChaosHeidemarie.Cards.Upgrade.EffulgentExpansion;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.RestSite;

public class MeditationRestSiteOption : ModRestSiteOptionTemplate
{
    public MeditationRestSiteOption(Player owner) : base(owner)
    {
    }

    public override string OptionId => "MEDITATION";

    public override RestSiteOptionAssetProfile AssetProfile =>
        new("res://ArtWorks/images/ui/rest_site/meditation_rest_site_option.png");

    private const int MeditationGoldCost = 80;

    public override bool IsEnabled => Owner.Gold >= MeditationGoldCost &&
                                      PileType.Deck.GetPile(Owner).Cards
                                          .Any(c => IsMeditationCard(c) && c.IsTransformable);

    private static bool IsMeditationCard(CardModel card)
    {
        return card is
            SwordRainCard
            or ThreadLightCard
            or HeroAllCard
            or EffulgentExpansionCard;
    }

    public override async Task<bool> OnSelect()
    {
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 0, 1)
        {
            Cancelable = true,
            RequireManualConfirmation = true
        };
        var selectedCards =
            await CardSelectCmd.FromDeckGeneric(Owner, prefs, c => IsMeditationCard(c) && c.IsTransformable);
        var original = selectedCards.FirstOrDefault();
        if (original == null)
            return false;
        var candidates = BuildCandidates(original).TakeRandom(3, original.Owner.RunState.Rng.CombatCardSelection)
            .ToList();
        if (candidates.Count == 0)
            return false;

        var selectedBranches =
            await CardSelectCmd.FromSimpleGrid(new BlockingPlayerChoiceContext(), candidates, Owner, prefs);
        var picked = selectedBranches.FirstOrDefault();
        if (picked == null)
            return false;

        await CardCmd.Transform(original, picked);
        await PlayerCmd.LoseGold(80, Owner);

        return true;
    }

    private List<CardModel> BuildCandidates(CardModel original)
    {
        var runState = Owner.RunState;

        return original switch
        {
            SwordRainCard =>
            [
                runState.CreateCard<SwordRainCardA>(Owner),
                runState.CreateCard<SwordRainCardB>(Owner),
                runState.CreateCard<SwordRainCardC>(Owner),
                runState.CreateCard<SwordRainCardD>(Owner)
            ],

            ThreadLightCard =>
            [
                runState.CreateCard<ThreadLightCardA>(Owner),
                runState.CreateCard<ThreadLightCardB>(Owner),
                runState.CreateCard<ThreadLightCardC>(Owner),
                runState.CreateCard<ThreadLightCardD>(Owner)
            ],

            HeroAllCard =>
            [
                runState.CreateCard<HeroAllUpgradeCardA>(Owner),
                runState.CreateCard<HeroAllUpgradeCardB>(Owner),
                runState.CreateCard<HeroAllUpgradeCardC>(Owner),
                runState.CreateCard<HeroAllUpgradeCardD>(Owner)
            ],

            EffulgentExpansionCard =>
            [
                runState.CreateCard<EffulgentExpansionCardA>(Owner),
                runState.CreateCard<EffulgentExpansionCardB>(Owner),
                runState.CreateCard<EffulgentExpansionCardC>(Owner),
                runState.CreateCard<EffulgentExpansionCardD>(Owner)
            ]
        };
    }
}