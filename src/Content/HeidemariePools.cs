using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Content;

public sealed class HeidemarieCardPool : TypeListCardPoolModel
{
    public override string Title => "Heidemarie";
    public override string EnergyColorName => "heidemarie";
    public override string CardFrameMaterialPath => "card_frame_colorless";
    public override Color DeckEntryCardColor => new("7A4F9AFF");
    public override bool IsColorless => false;
}

public sealed class HeidemarieRelicPool : TypeListRelicPoolModel
{
    public override string EnergyColorName => "heidemarie";
}

public sealed class HeidemariePotionPool : TypeListPotionPoolModel
{
    public override string EnergyColorName => "heidemarie";
}
