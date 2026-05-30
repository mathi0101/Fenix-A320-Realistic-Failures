using System.Reflection;
using System.Text.Json;

namespace RealFenixFailures.Domain.Services;

public static class EmbeddedJsonLoader {
    // Busca el recurso embebido por sufijo de nombre de archivo (p. ej. "training_presets.json")
    public static Stream? LoadFromEmbeddedJson(string fileName) {
        if (string.IsNullOrEmpty(fileName))
            throw new NullReferenceException("EmbebedJson filename not received");
        var asm = Assembly.GetExecutingAssembly();
        var resourceName = asm.GetManifestResourceNames()
                              .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
                           ?? throw new FileNotFoundException($"Embedded resource '{fileName}' not found. Available: {string.Join(", ", asm.GetManifestResourceNames())}");

        var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Resource stream '{resourceName}' not found.");

        return stream;
    }

    public async static Task<T?> LoadFromEmbeddedJson<T>(string fileName, CancellationToken ct) {
        if (string.IsNullOrEmpty(fileName))
            throw new NullReferenceException("EmbebedJson filename not received");
        var asm = Assembly.GetExecutingAssembly();
        var resourceName = asm.GetManifestResourceNames()
                              .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
                           ?? throw new FileNotFoundException($"Embedded resource '{fileName}' not found. Available: {string.Join(", ", asm.GetManifestResourceNames())}");

        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Resource stream '{resourceName}' not found.");

        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            // Permite que en JSON los enums puedan venir como string. Si vienen como número, System.Text.Json los parsea también.
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) }
        };

        return await JsonSerializer.DeserializeAsync<T>(stream, options, ct)!;
    }
}