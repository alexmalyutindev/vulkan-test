using Newtonsoft.Json;

namespace MtgWeb.Core;

public class Resources
{
    private static HttpClient? _httpClient;

    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    public static void Init(HttpClient? client)
    {
        _httpClient = client;
    }

    public static async Task<Scene> LoadScene(string name)
    {
        var json = await _httpClient!.GetStringAsync($"Resources/Scenes/{name}.scene.json");
        return JsonConvert.DeserializeObject<Scene>(json, JsonSerializerSettings)!;
    }

    public static async Task<Entity> LoadPrefab(string name)
    {
        var json = await _httpClient!.GetStringAsync($"Resources/Prefabs/{name}.prefab.json");
        var prefab = JsonConvert.DeserializeObject<Entity>(json);
        return prefab!;
    }

    public static async Task<(string, string)> LoadShader(String name)
    {
        var vertex = await _httpClient!.GetStringAsync($"Resources/Shaders/{name}.vert");
        var fragment = await _httpClient!.GetStringAsync($"Resources/Shaders/{name}.frag");

        return (vertex, fragment);
    }
}