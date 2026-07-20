using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ChaosHeidemarie.Keywords;

[RegisterOwnedCardKeyword(nameof(REST), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class RestKeyword
{
    public static readonly CardKeyword REST = ModContentRegistry.GetQualifiedKeywordId(ModInfo.Id, nameof(REST)).GetModCardKeyword();
}