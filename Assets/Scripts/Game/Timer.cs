using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    private float timerLength;
    private float currentTime;
    private bool isTimerUp;
    private bool invertOutput;
    public Timer(float timerLength, bool invertOutput)
    {
        this.timerLength = timerLength;
        this.currentTime = timerLength;
        this.isTimerUp = invertOutput;
        this.invertOutput = invertOutput;
    }
    public void Tick()
    {
        if (currentTime <= 0)
        {
            currentTime = 0;
            isTimerUp = true;
        }
        else
        {
            currentTime -= Time.deltaTime;
        }
    }
    public void SetTimerLength(float timerLength)
    {
        this.timerLength = timerLength;
    }
    public void ResetTimer()
    {
        isTimerUp = false;
        currentTime = timerLength;
    }
    public bool Status()
    {
        return invertOutput ? !isTimerUp : isTimerUp;
    }
    public float GetCurrentTime()
    {
        return currentTime;
    }

    public float GetTimerPercentage()
    {
        if(currentTime < 0)
        {
            return 0;
        }
        return 1f - (currentTime / timerLength);
    }
}
