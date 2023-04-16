using MtgWeb.Core.Serialization;
using Newtonsoft.Json;

namespace MtgWeb.Core;

public class Scene : IDisposable
{
    [JsonProperty(ItemConverterType = typeof(EntityConverter))]
    public Entity[] Root;

    public static async Task<Scene> Load(string name)
    {
        var scene = await Resources.LoadScene(name);
        await scene.PostLoad();
        return scene;
    }

    private async Task PostLoad()
    {
        for (var index = 0; index < Root.Length; index++)
        {
            var entity = Root[index];
            if (entity is PrefabEntity prefab)
            {
                Console.WriteLine("Loading prefab: " + prefab.PrefabName);
                Root[index] = await Resources.LoadPrefab(prefab.PrefabName);
            }
        }
    }

    public void Dispose()
    {
        foreach (var entity in Root) entity.Dispose();
    }
}