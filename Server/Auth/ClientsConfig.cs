using System.Text.Json;

namespace Server.Auth;

public record ClientEntry(string apiKey, string role);

public class ClientsConfig
{
    public Dictionary<string, ClientEntry> Clients { get; private set; } = new();

    public ClientsConfig(string path)
    {
        var json = File.Exists(path) ? File.ReadAllText(path) : "{}";

        var parsed = JsonSerializer.Deserialize<Dictionary<string, ClientEntry>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        Clients = parsed ?? new Dictionary<string, ClientEntry>();
    }

    // Note the nullable out to match Dictionary.TryGetValue signature
    public bool TryGet(string clientId, out ClientEntry? entry) =>
        Clients.TryGetValue(clientId, out entry);
}
