namespace GGHubShared.Extensions
{
    public static class EnumExtensions
    {
        public static bool In<T>(this T value, params T[] values)
        {
            return values.Contains(value);
        }
    }
}
