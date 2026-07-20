using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ChaosHeidemarie.Keywords;

[RegisterOwnedCardKeyword(nameof(Recycle), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class RecycleKeywords
{
    public static readonly CardKeyword Recycle = ModContentRegistry.GetQualifiedKeywordId(ModInfo.Id, nameof(Recycle)).GetModCardKeyword();
}