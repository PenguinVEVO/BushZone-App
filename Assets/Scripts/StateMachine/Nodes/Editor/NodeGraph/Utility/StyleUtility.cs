using UnityEngine;
using UnityEngine.UIElements;

public static class StyleUtility
{
    public static bool TryGetCustomStyle<T>(CustomStyleResolvedEvent evt, string ussVarName, ref T value)
    {
        if (typeof(T) == typeof(Color))
        {
            Color color = default;
            bool success = TryGetColor(evt, ussVarName, ref color);
            value = (T)(object)color;
            return success;
        }

        if (typeof(T) == typeof(float))
        {
            float f = default;
            bool success = TryGetFloat(evt, ussVarName, ref f);
            value = (T)(object)f;
            return success;
        }

        if (typeof(T) == typeof(int))
        {
            int i = default;
            bool success = TryGetInt(evt, ussVarName, ref i);
            value = (T)(object)i;
            return success;
        }

        Debug.LogWarning($"[StyleUtility] Unsupported custom style type: {typeof(T).Name} for property '{ussVarName}'");
        return false;
    }

    public static bool TryGetColor(CustomStyleResolvedEvent evt, string ussVarName, ref Color value)
    {
        var prop = new CustomStyleProperty<Color>(ussVarName);
        if (evt.customStyle.TryGetValue(prop, out var result))
        {
            value = result;
            return true;
        }
        return false;
    }

    public static bool TryGetFloat(CustomStyleResolvedEvent evt, string ussVarName, ref float value)
    {
        var prop = new CustomStyleProperty<float>(ussVarName);
        if (evt.customStyle.TryGetValue(prop, out var result))
        {
            value = result;
            return true;
        }
        return false;
    }

    public static bool TryGetInt(CustomStyleResolvedEvent evt, string ussVarName, ref int value)
    {
        var prop = new CustomStyleProperty<int>(ussVarName);
        if (evt.customStyle.TryGetValue(prop, out var result))
        {
            value = result;
            return true;
        }
        return false;
    }
}
