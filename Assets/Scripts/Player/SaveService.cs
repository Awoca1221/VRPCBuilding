using System.IO;
using UnityEngine;

public class SaveService
{
    public static void Save(string key, object data)
    {
        string path = BuildPath(key);
        string json = JsonUtility.ToJson(data);

        using var fileStream = new StreamWriter(path);
        fileStream.Write(json);
    }

    public static T Load<T>(string key)
    {
        string path = BuildPath(key);
        if (!File.Exists(path))
        {
            File.Create(path).Dispose();  // Создаёт пустой файл
            return default;
        }

        using var fileStream = new StreamReader(path);
        var json = fileStream.ReadToEnd();
        var data = JsonUtility.FromJson<T>(json);
        return data;
    }

    private static string BuildPath(string key)
    {
        return Path.Combine(Application.persistentDataPath, key);
    }
}
