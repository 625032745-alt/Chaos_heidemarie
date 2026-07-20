using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ChaosHeidemarie.Keywords;

[RegisterOwnedCardKeyword(nameof(Conclude), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class ConcludeKeywords
{
    public static readonly CardKeyword Conclude = ModContentRegistry.GetQualifiedKeywordId(ModInfo.Id, nameof(Conclude)).GetModCardKeyword();
}