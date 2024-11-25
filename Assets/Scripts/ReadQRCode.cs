using System;
using UnityEngine;
using UnityEngine.Rendering;
using ZXing;

/// <summary>
/// Reads QRCode from standard camera (It does not use the ARFoundation ARKit camera)
/// </summary>
public class ReadQRCode : MonoBehaviour
{
    [SerializeField] OpenFoodFactsTest openFoodFactsTest;
    [SerializeField] private CameraUpdate cam;
    private bool grabQR;
    //private float timeSinceLastCapture;
    //[SerializeField] private float timeBtwCapture = 0.25f;
    
    /// <summary>
    /// Match the Unity camera used for QR Code scanning with the one used by ARFoundation 
    /// </summary>
    
    void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
    }

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        OnPostRender();
    }

    private void Update()
    {
        //timeSinceLastCapture += Time.deltaTime;
        //if (timeSinceLastCapture >= timeBtwCapture)
        if (QRButton.clicked)
        {
            grabQR = true;
            // Debug.Log("Enabling Camera");
            //timeSinceLastCapture = 0;
        }
    }
    /// <summary>
    /// Scan QRCode using ZXing
    /// </summary>
    private void OnPostRender()
    {
        if (grabQR)
        {
            //Reset the grab state
            grabQR = false;

            // Scan Barcode using ZXing 
            try
            {
                IBarcodeReader barcodeReader = new BarcodeReader();
                // decode the current frame from QRCamera
                var result = barcodeReader.Decode(cam.webcamTexture.GetPixels32(), cam.webcamTexture.width, cam.webcamTexture.height);
                if (result != null)
                {
                    Debug.Log("QR Text:" + result.Text);
                    //FoodData foodData = await openFoodFactsTest.FetchProduct(result.Text);
                    //if (foodData != null)
                    //{
                        //UIManager.Instance.OnFoodSelected(foodData);
                    //}
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error when capturing camera texture: " + e);
            }
        }
    }
}
