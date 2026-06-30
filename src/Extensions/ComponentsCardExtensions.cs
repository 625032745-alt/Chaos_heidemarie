namespace ChaosHeidemarie.Extensions;

public static class ComponentsCardExtensions
{
    public static bool HasComponent<T>(this IComponentsCardModel? card)
        where T : class, ICardComponent
    {
        return card?.GetComponent<T>() != null;
    }

    public static bool HasComponent<T>(this CardModel? card)
        where T : class, ICardComponent
    {
        return card is IComponentsCardModel componentsCardModel && componentsCardModel.GetComponent<T>() != null;
    }

    public static ICardComponent? TryAddComponent<T>(this CardModel? card, T component)
        where T : class, ICardComponent
    {
        if (card is not IComponentsCardModel componentsCardModel)
            return null;

        return componentsCardModel.AddComponent(component);
    }

    public static ICardComponent? TryRemoveComponent<T>(this CardModel? card)
        where T : class, ICardComponent
    {
        if (card is not IComponentsCardModel componentsCardModel)
            return null;

        return componentsCardModel.RemoveComponent<T>();
    }
}
