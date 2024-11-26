using System;
using UnityEngine;
using ZXing;
using ZXing.Common;

public class WebcamBarcodeDecoder : MonoBehaviour
{
    [SerializeField] OpenFoodFactsTest openFoodFactsTest;
    [SerializeField] private CameraUpdate cam;
    [SerializeField] private GameObject barcodeIndicator;
    private IBarcodeReader barcodeReader;
    private float updateInterval = 0.25f; // 4 times a second
    private float lastUpdateTime;

    void Start()
    {
        // Initialize the barcode reader
        barcodeReader = new BarcodeReader
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                TryHarder = true
            }
        };
        lastUpdateTime = Time.time;
    }

    void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            lastUpdateTime = Time.time;

            if (cam.webcamTexture != null && cam.webcamTexture.isPlaying)
            {
                try
                {
                    // Get the webcam frame as a Color32 array
                    Color32[] frame = cam.webcamTexture.GetPixels32();

                    // Decode the barcode
                    var result = barcodeReader.Decode(frame, cam.webcamTexture.width, cam.webcamTexture.height);

                    if (result != null)
                    {
                        barcodeIndicator.SetActive(true);
                        Debug.Log($"Decoded Text: {result.Text}");
                        Debug.Log($"Barcode Format: {result.BarcodeFormat}");
                        FetchProduct(result.Text);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error decoding barcode: {ex.Message}");
                }
            }
        }
    }

    private async void FetchProduct(string code)
    {
        FoodData foodData = await openFoodFactsTest.FetchProduct(code);
        if (foodData != null)
        {
            Debug.Log(foodData.Name);
            Debug.Log(foodData.Calories);
            Debug.Log(foodData.Protein);
            Debug.Log(foodData.Fats);
            Debug.Log(foodData.Carbs);
            UIManager.Instance.OnFoodSelected(foodData);
        }
        barcodeIndicator.SetActive(false);
    }
}
