using Newtonsoft.Json;

namespace MtgWeb.Core;

public abstract class Component : IDisposable
{
    [JsonIgnore]
    public Entity? Entity;

    public static T Create<T>() where T : Component, new()
    {
        var component = new T();
        ComponentsBucket<T>.Add(component);
        return component;
    }
    
    public static T Create<T>(Entity entity) where T : Component, new()
    {
        var component = new T
        {
            Entity = entity
        };
        ComponentsBucket<T>.Add(component);
        return component;
    }

    public void Init(Entity entity)
    {
        Entity ??= entity;
    }

    public virtual void Start() { }

    public virtual void Update() { }

    public void Dispose()
    {
        ComponentsBucket.Remove(this);
    }
}
