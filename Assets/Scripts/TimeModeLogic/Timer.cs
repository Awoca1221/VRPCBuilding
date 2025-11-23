using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public TMP_Text text;
    private bool isRunning = false;
    public ResultsUIManager resultsManager;
    public float seconds = 0;
    public bool isIncrease = true;

    private void setTimerText()
    {
        int totalSeconds = Mathf.FloorToInt(Mathf.Abs(seconds));
        int mins = totalSeconds / 60;
        int secs = totalSeconds % 60;
        if (seconds < 0)
            text.text = string.Format("-{0:00}:{1:00}", mins, secs);
        else
            text.text = string.Format("{0:00}:{1:00}", mins, secs);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (text == null) {
            text = GetComponent<TMP_Text>();
        }
        setTimerText();
    }
    
    public void StartTimer()
    {
        isRunning = true;
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void SaveResult()
    {
        if (resultsManager) {
            resultsManager.AddResult(seconds);
        }
    }

    public void ClearTimer()
    {
        isRunning = false;
        seconds = 0;
        text.text = "00:00";
    }

    public void SetTimer(Timer obj)
    {
        seconds = obj.seconds;
        setTimerText();
    }

    public void SetTimer(float timeInSeconds)
    {
        seconds = timeInSeconds;
        setTimerText();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            if (isIncrease)
                seconds += Time.deltaTime;
            else
                seconds -= Time.deltaTime;
            setTimerText();
        }
    }
}
