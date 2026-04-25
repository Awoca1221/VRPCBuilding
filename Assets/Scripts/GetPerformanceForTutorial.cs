using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PCBuildManager))]
public class GetPerformanceForTutorial : MonoBehaviour
{
    public CPUInfo bestCPU;
    public GPUInfo bestGPU;
    public RAMInfo bestRAM;
    public TextMeshProUGUI scoreText;
    public UnityEvent OnScoreComplete;

    private PCBuildManager PCBuild;
    private uint requiredScore = 0;
    private uint score = 0;
    private readonly float adjustmentMult = 1.2f;
    private readonly float frequencyMult = 0.5f;
    private readonly uint memoryMult = 500;

    void Start()
    {
        PCBuild = GetComponent<PCBuildManager>();
        PCBuild.OnOverallStatusUpdated += UpdateUI;

        requiredScore += bestCPU.Performance;
        requiredScore += (uint)(bestGPU.Performance * adjustmentMult) + (bestGPU.MemoryAmountGB * memoryMult);
        requiredScore += (uint)(bestRAM.FrequencyMhz * frequencyMult) + (bestRAM.MemoryAmountGB * memoryMult);
        scoreText.SetText($"Текущий счёт: {score}\n\n Требуемый счёт: {requiredScore}");
    }

    private void UpdateUI()
    {
        UpdateScore();
        scoreText.SetText($"Текущий счёт: {score}\n\n Требуемый счёт: {requiredScore}");
        if (score >= requiredScore)
            OnScoreComplete?.Invoke();
    }

    private void UpdateScore()
    {
        var devices = PCBuild.ConnectedDevices;
        DeviceInfo deviceInfo;
        score = 0;
        foreach (var device in devices)
        {
            deviceInfo = device.GetComponent<AttachObjectDevice>().deviceInfo;
            switch (deviceInfo)
            {
                case CPUInfo:
                    var cpuInfo = (CPUInfo)deviceInfo;
                    score += cpuInfo.Performance;
                    break;
                case GPUInfo:
                    var gpuInfo = (GPUInfo)deviceInfo;
                    score += (uint)(gpuInfo.Performance * adjustmentMult) + (gpuInfo.MemoryAmountGB * memoryMult);
                    break;
                case RAMInfo:
                    var ramInfo = (RAMInfo)deviceInfo;
                    score += (uint)(ramInfo.FrequencyMhz * frequencyMult) + (ramInfo.MemoryAmountGB * memoryMult);
                    break;
            }
        }
    }
}
