namespace Webefinity.Extensions;

public static class PropertySetExtensions
{
    public static bool TryGetValue<T>(this IDictionary<string, object?> properties, string key, out T? value, T? defaultValue = default(T))
    {
        if (properties.TryGetValue(key , out var val) && val is not null)
        {
            value = (T)Convert.ChangeType(val, typeof(T));
            return true;
        }

        value = defaultValue;
        return false;
    }
}
