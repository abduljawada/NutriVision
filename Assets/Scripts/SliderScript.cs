using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    [SerializeField] private Slider scoreSlider;
    [SerializeField] private Slider iouSlider;
    //[SerializeField] private Slider widthSlider;
    //[SerializeField] private Slider heightSlider;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text iouText;
    //[SerializeField] private TMP_Text widthText;
    //[SerializeField] private TMP_Text heightText;
    [SerializeField] private RunModel runModel;
    //[SerializeField] private RectTransform rectTransform;


    private void Start()
    {
        scoreSlider.onValueChanged.AddListener(delegate {UpdateScore(); });
        iouSlider.onValueChanged.AddListener(delegate {UpdateIou(); });
        //widthSlider.onValueChanged.AddListener(delegate {UpdateWidth(); });
        //heightSlider.onValueChanged.AddListener(delegate {UpdateHeight(); });
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

    //private void UpdateWidth()
    //{
        //rectTransform.sizeDelta = new Vector2(widthSlider.value, heightSlider.value);
        //widthText.text = widthSlider.value.ToString();
    //}

//        private void UpdateHeight()
//    {
//        rectTransform.sizeDelta = new Vector2(widthSlider.value, heightSlider.value);
//        heightText.text = heightSlider.value.ToString();
//    }

}
