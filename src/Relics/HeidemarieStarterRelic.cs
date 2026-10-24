using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Relics;

[RegisterRelic(typeof(Content.HeidemarieRelicPool))]
[RegisterCharacterStarterRelic(typeof(Characters.Heidemarie))]
public sealed class HeidemarieStarterRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    private int _lastTriggerTurn;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/ui/top_panel/character_icon_heidemarie.png",
        IconOutlinePath: "res://ArtWorks/images/ui/top_panel/character_icon_heidemarie_outline.png");

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<InherentMemoryPower>(
            new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, null);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature != Owner.Creature) return;
        if (cardPlay.IsAutoPlay) return;
        if (!cardPlay.IsLastInSeries) return;
        if (!cardPlay.Card.Keywords.Contains(LinkKeywords.Link)) return;
        if (Owner.PlayerCombatState is { } pcs)
        {
            var turn = pcs.TurnNumber;
            if (_lastTriggerTurn == turn) return;
            _lastTriggerTurn = turn;
            var power = Owner.Creature.GetPower<InherentMemoryPower>();
            if (power != null)
            {
                Flash();
                await PowerCmd.Apply<InherentMemoryPower>(ctx, Owner.Creature, 1m, Owner.Creature, null);
            }
        }
    }
}