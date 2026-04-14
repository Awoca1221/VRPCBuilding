using UnityEditor;
using System;

public class ScriptableObjectIdProcessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            // Пропускаем всё, кроме .asset файлов
            if (!path.EndsWith(".asset")) continue;
            // Загружаем ассет
            var asset = AssetDatabase.LoadAssetAtPath<DeviceInfo>(path);
            if (asset == null) continue;
            // Проверяем, пуст ли ID
            if (string.IsNullOrEmpty(asset.ItemID))
            {
                // Генерируем новый ID
                asset.ItemID = Guid.NewGuid().ToString();
                // Сохраняем изменения
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
