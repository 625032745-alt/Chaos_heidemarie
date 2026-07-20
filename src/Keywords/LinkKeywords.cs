using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ChaosHeidemarie.Keywords;

[RegisterOwnedCardKeyword(nameof(Link), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class LinkKeywords
{
    public static readonly CardKeyword Link = ModContentRegistry.GetQualifiedKeywordId(ModInfo.Id, nameof(Link)).GetModCardKeyword();
}