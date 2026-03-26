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

public class DeviceInfo : ScriptableObject
{
    [ReadOnly] public string ItemID;
    [field: SerializeField] public AssetReferenceGameObject Prefab { get; protected set; }
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
}
