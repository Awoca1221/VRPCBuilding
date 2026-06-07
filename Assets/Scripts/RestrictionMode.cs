using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using Random = UnityEngine.Random;

[Serializable]
public class RestrictModeEntry
{
    public string type;
    public uint restrValue;
    public uint buildValue;
    public uint points;
    public string dateTime;
    public RestrictModeEntry(RestrictionMode.ModeType newType, uint newRestrValue, uint newBuildValue, uint newPoints, DateTime newDate) {
        type = newType.ToString();
        restrValue = newRestrValue;
        buildValue = newBuildValue;
        points = newPoints;
        dateTime = newDate.ToString("dd.MM.yyyy HH:mm");
    }
}

[Serializable]
public class RestrictModeList
{
    public List<RestrictModeEntry> results = new();
}

public class RestrictionMode : MonoBehaviour
{
    public enum ModeType
    {
        Performance,
        Price
    }

    public PCBuildManager PCBuild;
    public TextMeshProUGUI restrictionText;
    public Button completeButton;
    public FadeUI ErrorText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI resultPointsText;
    public Transform resultsListUI;
    public GameObject resultPrefab;
    public ModeType modeType { get; private set; } = ModeType.Performance;
    public bool isInProgress { get; private set; } = false;
    public bool isRestrictionPassed { get; private set; } = false;
    public UnityEvent OnModeComplete;

    private RestrictModeList resultsList;
    private uint restrictionValue = 0;
    private uint buildValue = 0;
    private uint score = 0;
    private readonly uint minPerformance = 26000;
    private readonly uint maxPerformance = 80000;
    private readonly uint minPrice = 50000;
    private readonly uint maxPrice = 120000;
    private readonly string saveFile = "restriction_mode_results";
    private readonly float difficultyMult = 0.05f;

    void Start()
    {
        PCBuild.OnOverallStatusUpdated += OnStatusChanged;
        UpdateResultsListUI();
    }

    void OnDestroy()
    {
        if (PCBuild != null)
            PCBuild.OnOverallStatusUpdated -= OnStatusChanged;
    }

    private void UpdateResultsListUI()
    {
        foreach (Transform child in resultsListUI)
        {
            Destroy(child.gameObject);
        }

        resultsList = SaveService.Load<RestrictModeList>(saveFile);
        for (int i = 0; i < resultsList.results.Count; i++)
        {
            var item = Instantiate(resultPrefab, resultsListUI);
            var itemLogic = item.GetComponent<RestrModeResult>();
            int index = i;
            itemLogic.SetData(resultsList.results[i], () => {
                DeleteResultAtIndex(index);
            });
        }
    }

    private void DeleteResultAtIndex(int index)
    {
        if (index >= 0 && index < resultsList.results.Count)
        {
            resultsList.results.RemoveAt(index);
            SaveService.Save(saveFile, resultsList);
            UpdateResultsListUI();
        }
    }

    public void SetPerformanceMode()
    {
        if (!isInProgress) modeType = ModeType.Performance;
    }

    public void SetPriceMode()
    {
        if (!isInProgress) modeType = ModeType.Price;
    }

    public void StartMode()
    {
        if (isInProgress && restrictionText != null) return;

        isInProgress = true;
        switch (modeType)
        {
            case ModeType.Performance:
                restrictionValue = (uint)Random.Range(minPerformance, maxPerformance);
                restrictionText.text = $"Итоговая производительность сборки должна быть <color=#ADD8E6>больше</color>, чем: {restrictionValue}.\n\nТекущий счёт: 0";
                break;
            case ModeType.Price:
                restrictionValue = (uint)Random.Range(minPrice, maxPrice);
                restrictionText.text = $"Итоговая цена сборки должна быть <color=#ADD8E6>меньше</color>, чем: {restrictionValue}.\n\nТекущий счёт: 0";
                break;
        }
    }

    public void StopMode()
    {
        isInProgress = false;
        isRestrictionPassed = false;
        buildValue = 0;
        score = 0;
    }

    public void CompleteMode()
    {
        if (!isInProgress || PCBuild.PCStatus != PCBuildManager.Status.Working) return;
        if (!isRestrictionPassed)
        {
            ErrorText.ShowText();
            return;
        }

        RestrictModeList data = SaveService.Load<RestrictModeList>(saveFile);
        data.results.Add(new RestrictModeEntry(modeType, restrictionValue, buildValue, score, DateTime.Now));
        SaveService.Save(saveFile, data);
        UpdateResultsListUI();
        resultText.text = $"Ограничение: {modeType}\nТребуемое значение: {restrictionValue}\nЗначение сборки: {buildValue}";
        resultPointsText.text = $"Итоговый счёт: {score}";
        OnModeComplete?.Invoke();
    }

    public void UpdateScore()
    {
        if (!isInProgress) return;
        if (PCBuild.PCStatus != PCBuildManager.Status.Working) score = 0;

        //uint buildValue;
        uint perfectResult;
        uint worstResult;
        switch (modeType)
        {
            case ModeType.Performance:
                buildValue = PCBuild.GetOverallPerformance();
                perfectResult = (uint)(restrictionValue + restrictionValue * difficultyMult);
                worstResult = restrictionValue * 2;
                if (buildValue <= restrictionValue)
                {
                    isRestrictionPassed = false;
                    score = 0;
                    break;
                }
                Debug.Log("прошло проверку");
                isRestrictionPassed = true;
                if (buildValue <= perfectResult)
                {
                    score = 1000;
                    break;
                }
                Debug.Log("не идеально");
                if (buildValue >= worstResult)
                {
                    score = 0;
                    break;
                }
                Debug.Log("не очень плохо");
                score = (uint)((float)(worstResult - buildValue) / (worstResult - perfectResult) * 1000);
                break;
            case ModeType.Price:
                buildValue = PCBuild.GetPrice();
                perfectResult = (uint)(restrictionValue - restrictionValue * difficultyMult);
                worstResult = restrictionValue / 2;
                if (buildValue >= restrictionValue)
                {
                    isRestrictionPassed = false;
                    score = 0;
                    break;
                }

                isRestrictionPassed = true;
                if (buildValue >= perfectResult)
                {
                    score = 1000;
                    break;
                }

                if (buildValue <= worstResult)
                {
                    score = 0;
                    break;
                }

                score = (uint)((float)(worstResult - buildValue) / (worstResult - perfectResult) * 1000);
                break;
        }
        Debug.Log("обновляем UI");
        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        switch (modeType)
        {
            case ModeType.Performance:
                restrictionText.text = $"Итоговая производительность сборки должна быть <color=#ADD8E6>больше</color>, чем: {restrictionValue}.\n\nТекущий счёт: {score}";
                break;
            case ModeType.Price:
                restrictionText.text = $"Итоговая цена сборки должна быть <color=#ADD8E6>меньше</color>, чем: {restrictionValue}.\n\nТекущий счёт: {score}";
                break;
        }
    }

    private void OnStatusChanged()
    {
        UpdateScore();
        if (PCBuild.PCStatus == PCBuildManager.Status.Working)
        {
            completeButton.interactable = true;
        } else {
            completeButton.interactable = false;
        }
    }
}
