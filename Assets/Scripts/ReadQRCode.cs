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
    public Camera ARCamera;
    public Camera QRCamera;
    private bool grabQR;
    private float timeSinceLastCapture;
    [SerializeField] private float timeBtwCapture = 0.25f;
    
    /// <summary>
    /// Match the Unity camera used for QR Code scanning with the one used by ARFoundation 
    /// </summary>
    
    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
    }

    private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        OnPreRender();
    }

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        OnPostRender();
    }


    public void OnPreRender()
    {
        QRCamera.projectionMatrix = ARCamera.projectionMatrix;
        QRCamera.fieldOfView = ARCamera.fieldOfView;
        QRCamera.transform.localPosition = Vector3.zero;
        QRCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    public void Start()
    {
        QRCamera.enabled = false;
        Application.runInBackground = true;
    }

    private void Update()
    {
        timeSinceLastCapture += Time.deltaTime;
        if (timeSinceLastCapture >= timeBtwCapture)
        {
            grabQR = true;
            QRCamera.enabled = true;
            // Debug.Log("Enabling Camera");
            timeSinceLastCapture = 0;
        }
    }
    /// <summary>
    /// Scan QRCode using ZXing
    /// </summary>
    private void OnPostRender()
    {
        if (grabQR)
        {
            //Create a new texture with the width and height of the screen
            Texture2D QRTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
            QRTexture.ReadPixels(new Rect(0, 0, 1, 1), 0, 0, false);
            QRTexture.Apply();

            //Reset the grab state
            grabQR = false;
            // No need for the QR camera
            QRCamera.enabled = false;
            // Debug.Log("Disabling Camera");


            // Scan Barcode using ZXing 
            try
            {
                IBarcodeReader barcodeReader = new BarcodeReader();
                // decode the current frame from QRCamera
                var result = barcodeReader.Decode(QRTexture.GetPixels32(), QRTexture.width, QRTexture.height);
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
