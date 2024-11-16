using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    [SerializeField] private Slider scoreSlider;
    [SerializeField] private Slider iouSlider;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text iouText;
    [SerializeField] private RunModel runModel;


    private void Start()
    {
        scoreSlider.onValueChanged.AddListener(delegate {UpdateScore(); });
        iouSlider.onValueChanged.AddListener(delegate {UpdateIou(); });
    }

    private void UpdateScore()
    {
        runModel.SetScoreThreshold(scoreSlider.value);
        scoreText.text = scoreSlider.value.ToString();
    }

    private void UpdateIou()
    {
        runModel.SetIouThreshold(iouSlider.value);
        iouText.text = iouSlider.value.ToString();
    }

}
