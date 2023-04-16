using System.Collections;

namespace MtgWeb.Core;

public static class ComponentsBucket
{
    public static void Add<T>(T component) where T : Component => 
        ComponentsBucket<T>.Add(component);

    public static void Remove<T>(T component) where T : Component => 
        ComponentsBucket<T>.Remove(component);
}

public static class ComponentsBucket<T> where T : Component
{
    public static IEnumerable<T> Bucket => _bucket;
    private static T[] _bucket;
    private static int _count;

    static ComponentsBucket()
    {
        _bucket = Array.Empty<T>();
        _count = 0;
    }

    public static void Add(T component)
    {
        if (_bucket.Length >= _count)
        {
            _count = _bucket.Length + 1;
            Array.Resize(ref _bucket, _count);
        }
        else
        {
            _count++;
        }
        _bucket[_count - 1] = component;
    }

    public static void Remove(T component)
    {
        var index = Array.IndexOf(_bucket, component);
        if (index > 0)
        {
            _count--;
            _bucket[index] = _bucket[_count];
            _bucket[_count] = null;
        }
    }
}