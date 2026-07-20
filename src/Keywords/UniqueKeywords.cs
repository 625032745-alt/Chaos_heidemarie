using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ChaosHeidemarie.Keywords;

[RegisterOwnedCardKeyword(nameof(Unique), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class UniqueKeyword
{
    public static readonly CardKeyword Unique = ModContentRegistry.GetQualifiedKeywordId(ModInfo.Id, nameof(Unique)).GetModCardKeyword();
}