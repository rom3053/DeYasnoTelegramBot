namespace DeYasnoTelegramBot.Application.Common.Extensions;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> WhereIf<T>(
        this IEnumerable<T> ienumerable,
        bool condition,
        Func<T, bool> predicate)
    {
        return condition ? ienumerable.Where(predicate) : ienumerable;
    }
}
