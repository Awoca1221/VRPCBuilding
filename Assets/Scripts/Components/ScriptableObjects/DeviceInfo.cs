using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine.AddressableAssets;
using NaughtyAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class IncludeInDictAttribute : Attribute { }

public class DeviceInfo : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField, ReadOnly] private string itemID;
    public string ItemID => itemID;
    [field: SerializeField] public AssetReferenceGameObject Prefab { get; private set; }
    [field: SerializeField] public string Name { get; protected set; } = "";
    public virtual ComponentType ComponentType => ComponentType.NotSelected;

    public Dictionary<string, string> ToDict()
    {
        var dict = new Dictionary<string, string>();
        var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetCustomAttribute<IncludeInDictAttribute>() != null);
        
        foreach (var p in props)
        {
            var v = p.GetValue(this);
            dict[p.Name] = v is Array a ? $"[{string.Join(", ", a.Cast<object>())}]" : v.ToString();
        }
        return dict;
    }
    
    // Вызывается перед сереализацией (сохранения ассета)
    public void OnBeforeSerialize()
    {
        if (string.IsNullOrWhiteSpace(itemID))
        {
            itemID = Guid.NewGuid().ToString();
        }
    }

    // Вызывается после десериализации (загрузки ассета)
    public void OnAfterDeserialize()
    {
        // пусто, интерфейс просит реализацию метода
    }
}
