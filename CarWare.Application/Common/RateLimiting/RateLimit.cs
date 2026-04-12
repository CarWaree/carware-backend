public static class RateLimitHelper
{
    public static string BuildKey(string type, string identifier)
        => $"{type}:{identifier.ToLower()}";
}