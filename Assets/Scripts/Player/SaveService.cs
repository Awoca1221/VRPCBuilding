using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class SaveService
{
    // Объект для блокировки доступа к файловой системе
    private static readonly object _fileLock = new();
    
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
        }
        
        onSave?.Invoke(key);
    }

    public static T Load<T>(string key) where T : new()
    {
        lock (_fileLock)
        {
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

    private static string BuildPath(string key)
    {
        return Path.Combine(Application.persistentDataPath, key);
    }
}
