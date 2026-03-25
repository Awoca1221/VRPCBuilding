using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class SaveService
{
    // Объект для блокировки доступа к файловой системе
    private static readonly object _fileLock = new();

    // Кэш: key -> данные
    private static readonly Dictionary<string, object> _cache = new();
    
    // параметр передаёт key сохранения
    public static UnityAction<string> onSave;

    public static void Save(string key, object data)
    {
        lock (_fileLock)
        {
            string path = BuildPath(key);
            string json = JsonUtility.ToJson(data);

            using var fileStream = new StreamWriter(path);
            fileStream.Write(json);

            _cache[key] = data;
        }
        
        onSave?.Invoke(key);
    }

    public static T Load<T>(string key) where T : new()
    {
        lock (_fileLock)
        {
            if (TryGetFromCache(key, out T cached))
            {
                return cached;
            }

            string path = BuildPath(key);
            if (!File.Exists(path))
            {
                var emptyData = new T();
                Save(key, emptyData);
                return emptyData;
            }

            try
            {
                using var fileStream = new StreamReader(path);
                var json = fileStream.ReadToEnd();
                var data = JsonUtility.FromJson<T>(json);
                return data ?? new T();
            }
            catch (IOException)
            {
                Debug.Log($"Не удалось загрузить {path}");
                return new T();
            }
        }
    }

    private static bool TryGetFromCache<T>(string key, out T result)
    {
        if (_cache.TryGetValue(key, out object cached) && cached is T typed)
        {
            result = typed;
            return true;
        }
        
        if (_cache.ContainsKey(key))
            _cache.Remove(key);
        
        result = default;
        return false;
    }

    private static string BuildPath(string key)
    {
        return Path.Combine(Application.persistentDataPath, key);
    }
}
