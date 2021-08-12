using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCounter : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    private int totalScore = 0;
    private int comboScore = 0;
    private int flipCounter;
    private float spinCounter;
    private int comboCounter = 0;
    private float previosFrameRotation;
    private float currentFrameRotation;
    private float rotationAngle;
    private bool waitFullRotation = false;
    private float previosFrameSpin;
    private float currentFrameSpin;
    private float spinAngle;
    private bool waitFullSpin = false;
    private bool startRotation = true;


    public void CountFlips(Transform obj)
    {
        if (startRotation)
        {
            previosFrameRotation = obj.transform.rotation.eulerAngles.x;
            previosFrameSpin = obj.transform.rotation.eulerAngles.y;
            startRotation = false;
        }
        currentFrameRotation = obj.transform.rotation.eulerAngles.x;
        currentFrameSpin = obj.transform.rotation.eulerAngles.y;
        rotationAngle += Quaternion.Angle(Quaternion.Euler(currentFrameRotation, 0, 0),  Quaternion.Euler(previosFrameRotation, 0, 0));
        float angle = Quaternion.Angle(Quaternion.Euler(0, currentFrameSpin, 0), Quaternion.Euler(0, previosFrameSpin, 0));
        if (angle < 170) spinAngle += angle;
        previosFrameRotation = currentFrameRotation;
        previosFrameSpin = currentFrameSpin;
        if (!waitFullRotation && rotationAngle >= 300) 
        {
            flipCounter++;
            waitFullRotation = true;
        }
        if (rotationAngle >= 360) 
        {
            rotationAngle = 0;
            waitFullRotation = false;
        }
        if (!waitFullSpin && spinAngle >= 170)
        {
            spinCounter += 0.5f;
            waitFullSpin = true;
        }
        if (spinAngle >= 360)
        {
            spinAngle = 0;
            spinCounter += 0.5f;
            waitFullSpin = false;
        }
    }

    public void CountCombo()
    {
        comboCounter++;
        comboScore += flipCounter * flipCounter * 100 + (int)(spinCounter * 100);
        scoreText.text = "Score: " + totalScore + $"\n{comboScore} X {comboCounter}";
        ResetFlipsCounter();
    }

    public void AddResult()
    {
        if (comboCounter == 0)
        {
            totalScore += flipCounter * flipCounter * 100 + (int)(spinCounter * 100);
            scoreText.text = "Score: " + totalScore;
            ResetFlipsCounter();
        }
        else
        {
            StartCoroutine(ResetComboCounterCoroutine());
        }
    }

    public void AddResultDead()
    {
        comboCounter = 0;
        ResetFlipsCounter();
        scoreText.text = "Score: " + totalScore;
    }

    private IEnumerator ResetComboCounterCoroutine()
    {
        CountCombo();
        totalScore += comboScore * comboCounter;
        comboCounter = 0;
        comboScore = 0;
        yield return new WaitForSeconds(1);
        scoreText.text = "Score: " + totalScore;
    }

    private void ResetFlipsCounter()
    {
        flipCounter = 0;
        previosFrameRotation = 0;
        currentFrameRotation = 0;
        rotationAngle = 0;
        waitFullRotation = false;
        spinCounter = 0;
        previosFrameSpin = 0;
        currentFrameSpin = 0;
        spinAngle = 0;
        waitFullSpin = false;
        startRotation = true;
    }

    public void ShowFinalResult()
    {
        scoreText.rectTransform.anchoredPosition = Vector3.Lerp(scoreText.rectTransform.anchoredPosition, new Vector3(-370, -250, 0), Time.deltaTime);
        scoreText.rectTransform.localScale = Vector3.Lerp(scoreText.rectTransform.localScale, Vector3.one * 1.5f, Time.deltaTime);
    }
}
