﻿using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/*
 *  YOLOv8n Inference Script
 *  ========================
 * 
 * Place this script on the Main Camera.
 * 
 * Place the yolob8n.sentis file in the asset folder and drag onto the asset field
 * Place a *.mp4 video file in the Assets/StreamingAssets folder
 * Create a RawImage in your scene and set it as the displayImage field
 * Drag the classes.txt into the labelsAsset field
 * Add a reference to a sprite image for the bounding box and a font for the text
 * 
 */


public class RunModel : MonoBehaviour
{
    // Drag the yolov8n.sentis file here
    [SerializeField] private ModelAsset modelAsset;
    // Link the classes.txt here:
    [SerializeField] private TextAsset labelsAsset;
    // Create a Raw Image in the scene and link it here:
    [SerializeField] private RawImage displayImage;
    [SerializeField] private CameraUpdate cam;
    [SerializeField] private GameObject objectBox;


    private Transform displayLocation;
    private Worker worker;
    private string[] labels;
    private RenderTexture targetRT;

    private QueryScript queryScript => GetComponent<QueryScript>();

    //Image size for the model
    private const int imageWidth = 640;
    private const int imageHeight = 640;

    List<GameObject> boxPool = new();

    [SerializeField, Range(0, 1)] float iouThreshold = 0.5f;
    [SerializeField, Range(0, 1)] float scoreThreshold = 0.5f;

    [SerializeField] private float detectionInterval = 0.1f;
    private float lastDetectionTime;

    Tensor<float> centersToCorners;
    //bounding box data
    public struct BoundingBox
    {
        public float centerX;
        public float centerY;
        public float width;
        public float height;
        public string label;
    }

    void Start()
    {
        //Parse neural net labels
        labels = labelsAsset.text.Split('\n');

        LoadModel();

        targetRT = new RenderTexture(imageWidth, imageHeight, 0);

        //Create image to display video
        displayLocation = displayImage.transform;
    }
    void LoadModel()
    {

        //Load model
        //var sourceModel = ModelLoader.Load(Path.Join(Application.streamingAssetsPath, modelName));
        Model sourceModel = ModelLoader.Load(modelAsset);

        centersToCorners = new Tensor<float>(new TensorShape(4, 4),
        new float[]
        {
                    1,      0,      1,      0,
                    0,      1,      0,      1,
                    -0.5f,  0,      0.5f,   0,
                    0,      -0.5f,  0,      0.5f
        });

        FunctionalGraph graph = new();

        FunctionalTensor input = graph.AddInputs(sourceModel)[0];
        FunctionalTensor output = Functional.Forward(sourceModel, input)[0];
        FunctionalTensor boxCoords = output[0, 0..4, ..].Transpose(0, 1);        //shape=(8400,4)
        FunctionalTensor allScores = output[0, 4.., ..];                         //shape=(80,8400)
        FunctionalTensor scores = Functional.ReduceMax(allScores, 0) - scoreThreshold;        //shape=(8400)
        FunctionalTensor classIDs = Functional.ArgMax(allScores, 0);                          //shape=(8400) 
        FunctionalTensor boxCorners = Functional.MatMul(boxCoords, Functional.Constant(centersToCorners));
        FunctionalTensor indices = Functional.NMS(boxCorners, scores, iouThreshold);           //shape=(N)
        FunctionalTensor indices2 = indices.Unsqueeze(-1).BroadcastTo(new int[] { 4 });//shape=(N,4)
        FunctionalTensor coords = Functional.Gather(boxCoords, 0, indices2);                  //shape=(N,4)
        FunctionalTensor labelIDs = Functional.Gather(classIDs, 0, indices);                  //shape=(N)
        Model runtimeModel = graph.Compile(coords, labelIDs);


        //Here we transform the output of the sourceModel by feeding it through a Non-Max-Suppression layer.
        //Model runtimeModel = Functional.Compile(
        //input =>
        //{
        //    FunctionalTensor modelOutput = sourceModel.Forward(input)[0];
        //    FunctionalTensor boxCoords = modelOutput[0, 0..4, ..].Transpose(0, 1);        //shape=(8400,4)
        //    FunctionalTensor allScores = modelOutput[0, 4.., ..];                         //shape=(80,8400)
        //    FunctionalTensor scores = Functional.ReduceMax(allScores, 0) - scoreThreshold;        //shape=(8400)
        //    FunctionalTensor classIDs = Functional.ArgMax(allScores, 0);                          //shape=(8400) 
        //    FunctionalTensor boxCorners = Functional.MatMul(boxCoords, FunctionalTensor.FromTensor(centersToCorners));
        //    FunctionalTensor indices = Functional.NMS(boxCorners, scores, iouThreshold);           //shape=(N)
        //    FunctionalTensor indices2 = indices.Unsqueeze(-1).BroadcastTo(new int[] { 4 });//shape=(N,4)
        //    FunctionalTensor coords = Functional.Gather(boxCoords, 0, indices2);                  //shape=(N,4)
        //    FunctionalTensor labelIDs = Functional.Gather(classIDs, 0, indices);                  //shape=(N)
        //    return (coords, labelIDs);
        //},
        //InputDef.FromModel(sourceModel)[0]
        //);

        //Create engine to run model
        worker = new(runtimeModel, BackendType.GPUCompute);
    }

    void Update()
    {
        if (Time.time >= lastDetectionTime + detectionInterval)
        {
            ProcessFrame();
            lastDetectionTime = Time.time;
        }
    }

    private void ProcessFrame()
    {
        ClearAnnotations();

        if (!cam.webCamTexture) return;

        using Tensor<float> input = TextureConverter.ToTensor(cam.webCamTexture, imageWidth, imageHeight, 3);
        worker.Schedule(input);

        input?.Dispose();

        Tensor<float> output = worker.PeekOutput("output_0") as Tensor<float>;
        Tensor<int> labelIDs = worker.PeekOutput("output_1") as Tensor<int>;

        var cpuOutput = output.ReadbackAndClone();
        var cpuLabelIDs = labelIDs.ReadbackAndClone();

        output?.Dispose();
        labelIDs.Dispose();

        float displayWidth = displayImage.rectTransform.rect.width;
        float displayHeight = displayImage.rectTransform.rect.height;

        float scaleX = displayWidth / imageWidth;
        float scaleY = displayHeight / imageHeight;

        int boxesFound = cpuOutput.shape[0];
        //Draw the bounding boxes
        for (int n = 0; n < Mathf.Min(boxesFound, 200); n++)
        {
            string label = labels[cpuLabelIDs[n]];
            var box = new BoundingBox
            {
                centerX = cpuOutput[n, 0] * scaleX - displayWidth / 2,
                centerY = cpuOutput[n, 1] * scaleY - displayHeight / 2,
                width = cpuOutput[n, 2] * scaleX,
                height = cpuOutput[n, 3] * scaleY,
                label = label,
            };
            if (label.ToLower().Trim() != "apple" && label.ToLower().Trim() != "banana") return;
            DrawBox(box, n);
            //Debug.Log(box.label);
            queryScript.QueryFruitAndDisplay(label);
        }

        cpuOutput?.Dispose();
        cpuLabelIDs?.Dispose();
    }

    private void DrawBox(BoundingBox box, int id)
    {
        //Create the bounding box graphic or get from pool
        GameObject panel;
        if (id < boxPool.Count)
        {
            panel = boxPool[id];
            panel.SetActive(true);
        }
        else
        {
            panel = CreateNewBox();
        }
        //Set box position
        panel.transform.localPosition = new Vector3(box.centerX, -box.centerY);

        //Set box size
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(box.width, box.height);

        //Set label text
        //var label = panel.GetComponentInChildren<TMP_Text>();
        //label.text = box.label;
        //label.fontSize = (int)fontSize;
    }

    public GameObject CreateNewBox()
    {
        //Create the box
        var panel = Instantiate(objectBox, displayLocation);

        boxPool.Add(panel);
        return panel;
    }

    public void ClearAnnotations()
    {
        foreach (var box in boxPool)
        {
            box.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        centersToCorners?.Dispose();
        worker?.Dispose();
    }
}