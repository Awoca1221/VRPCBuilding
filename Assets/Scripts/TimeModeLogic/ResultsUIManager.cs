using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

[Serializable]
public class ResultEntry {
    public float timeSeconds;
    public string dateTime;
    public ResultEntry(float time, DateTime date) {
        timeSeconds = time;
        dateTime = date.ToString("dd.MM.yyyy HH:mm");
    }
}

[Serializable]
public class ResultsList {
    public List<ResultEntry> results = new();
}

public class ResultsUIManager : MonoBehaviour {
    public Transform contentPanel; // Контейнер внутри ScrollRect с Vertical Layout Group
    public GameObject resultItemPrefab; // Префаб одного результата со скриптом ResultItemUI

    private ResultsList timeResults = new();

    void Start() {
        LoadAndDisplayResults();
    }

    private void OnDestroy()
    {
        #if UNITY_EDITOR
        ClearResults();
        #endif
    }

    private void ClearResults()
    {
        PlayerPrefs.DeleteKey("timeResults");
        PlayerPrefs.Save();
        timeResults = new ResultsList();
        RefreshUI();
        Debug.Log("Результаты очищены при закрытии сцены.");
    }

    private void LoadAndDisplayResults() {
        timeResults = LoadResults();
        RefreshUI();
    }

    private void RefreshUI() {
        timeResults.results = timeResults.results.OrderBy(r => r.timeSeconds).ToList();
        foreach (Transform child in contentPanel) {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < timeResults.results.Count; i++) {
            var item = Instantiate(resultItemPrefab, contentPanel);
            var itemUI = item.GetComponent<ResultItemUI>();
            int index = i; // Локальная копия для замыкания
            itemUI.SetData(timeResults.results[i], () => {
                DeleteResultAtIndex(index);
            });
        }
    }

    public void AddResult(float timeSeconds) {
        timeResults = LoadResults();
        timeResults.results.Add(new ResultEntry(timeSeconds, DateTime.Now));
        SaveResults(timeResults);
        RefreshUI();
    }

    public void DeleteResultAtIndex(int index) {
        if (index >= 0 && index < timeResults.results.Count) {
            timeResults.results.RemoveAt(index);
            SaveResults(timeResults);
            RefreshUI();
        }
    }

    public ResultsList LoadResults() {
        if (!PlayerPrefs.HasKey("timeResults")) {
            return new ResultsList();
        }
        string json = PlayerPrefs.GetString("timeResults");
        var loadedResults = JsonUtility.FromJson<ResultsList>(json);
        return loadedResults;
    }

    private void SaveResults(ResultsList results) {
        string json = JsonUtility.ToJson(results);
        PlayerPrefs.SetString("timeResults", json);
        PlayerPrefs.Save();
    }
}
