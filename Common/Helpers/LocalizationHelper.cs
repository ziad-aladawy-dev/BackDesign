using System.Reflection;
using System.Text.Json;
using HUP.Core.Enums;

namespace HUP.Common.Helpers;

public static class LocalizationHelper
{
    private const string DefaultLanguage = "ar";

    public static T Get<T>(string json, string lang)
    {
        if (string.IsNullOrEmpty(json))
            return default;

        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, T>>(json);

            if (dict != null && dict.TryGetValue(lang, out var value))
                return value;

            if (dict != null && dict.TryGetValue(DefaultLanguage, out var defaultVal))
                return defaultVal;

            return default;
        }
        catch (JsonException)
        {
            // Fallback for non-JSON strings (legacy data or plain text)
            if (typeof(T) == typeof(string))
                return (T)(object)json;
            return default;
        }
    }
    public static string Get(Enum value, string lang)
    {
        if (value == null) return string.Empty;

        var field = value.GetType().GetField(value.ToString());
        var attr = field?.GetCustomAttribute<LocalizedAttribute>();

        if (attr == null)
            return value.ToString();

        return lang switch
        {
            "ar" => attr.Ar,
            _ => attr.En
        };
    }
}