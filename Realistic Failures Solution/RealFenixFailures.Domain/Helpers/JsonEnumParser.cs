using System.Text.Json;

namespace RealFenixFailures.Domain.Helpers;

public static class JsonEnumParser {
    // Soporta números o strings (case-insensitive)
    public static TEnum ParseEnum<TEnum>(JsonElement element, TEnum defaultValue = default) where TEnum : struct, Enum {
        try {
            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var intVal)) {
                if (Enum.IsDefined(typeof(TEnum), intVal))
                    return (TEnum)Enum.ToObject(typeof(TEnum), intVal);
            } else if (element.ValueKind == JsonValueKind.String) {
                var s = element.GetString()!;
                if (int.TryParse(s, out var numeric)) // si el string contiene número
                {
                    if (Enum.IsDefined(typeof(TEnum), numeric))
                        return (TEnum)Enum.ToObject(typeof(TEnum), numeric);
                }

                if (Enum.TryParse<TEnum>(s, true, out var parsed))
                    return parsed;
            }
        } catch {
            // ignorar y devolver default
        }

        return defaultValue;
    }
}