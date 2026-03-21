using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Button))]
public class BuildElement : MonoBehaviour
{
    public TMP_Text description;
    public GameObject select;
    public HoldButton deleteButton;
    public Button selectButton;


    public void SetSelect(bool status)
    {
        select.SetActive(status);
    }

    public void SetData(Build entry, Action OnSelect, Action OnDelete)
    {
        description.text = FormatBuild(entry);
        selectButton.onClick.AddListener(() => OnSelect?.Invoke());
        deleteButton.onFinishEvent.AddListener(() => OnDelete?.Invoke());
    }

    private static string FormatBuild(Build build)
    {
        // Группируем устройства по type
        var groupedByType = build.devices
            .Where(d => !string.IsNullOrEmpty(d.type) && !string.IsNullOrEmpty(d.name))
            .GroupBy(d => d.type)
            .ToList();

        var lines = new List<string>();

        foreach (var group in groupedByType)
        {
            var type = group.Key;
            var nameGroups = group
                .GroupBy(d => d.name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();

            var nameParts = new List<string>();
            
            foreach (var nameGroup in nameGroups)
            {
                if (nameGroup.Count == 1)
                    nameParts.Add(nameGroup.Name);
                else
                    nameParts.Add($"{nameGroup.Name}(x{nameGroup.Count})");
            }

            string line = $"<color=#ADD8E6>{type}:</color> {string.Join(", ", nameParts)}";
            lines.Add(line);
        }

        return string.Join("\n", lines);
    }
}
