using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UI;

public class RunModel : MonoBehaviour
{
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private TextAsset labelsAsset;
    [SerializeField] private RawImage displayImage;
    [SerializeField] private CameraUpdate cam;
    [SerializeField] private GameObject objectBox;


    private Transform displayLocation;
    private Worker worker;
    private string[] labels;
    private RenderTexture targetRT;

    private const int imageWidth = 640;
    private const int imageHeight = 640;

    List<GameObject> boxPool = new();

    [SerializeField, Range(0, 1)] float iouThreshold = 0.5f;
    [SerializeField, Range(0, 1)] float scoreThreshold = 0.5f;
    [SerializeField] private int maxDetectionsInFrame = 10;

    private float lastDetectionTime;

    const int k_LayersPerFrame = 20;
    IEnumerator m_Schedule;
    bool m_Started = false;


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

    public void SetScoreThreshold(float newScoreThreshold)
    {
        scoreThreshold = newScoreThreshold;
    }

    public void SetIouThreshold(float newIouThreshold)
    {
        iouThreshold = newIouThreshold;
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
        FunctionalTensor scores = Functional.ReduceMax(allScores, 0);        //shape=(8400)
        FunctionalTensor classIDs = Functional.ArgMax(allScores, 0);                          //shape=(8400) 
        FunctionalTensor boxCorners = Functional.MatMul(boxCoords, Functional.Constant(centersToCorners));
        FunctionalTensor indices = Functional.NMS(boxCorners, scores, iouThreshold, scoreThreshold);           //shape=(N)
        FunctionalTensor indices2 = indices.Unsqueeze(-1).BroadcastTo(new int[] { 4 });//shape=(N,4)
        FunctionalTensor coords = Functional.Gather(boxCoords, 0, indices2);                  //shape=(N,4)
        FunctionalTensor labelIDs = Functional.Gather(classIDs, 0, indices);                  //shape=(N)
        Model runtimeModel = graph.Compile(coords, labelIDs);

        //Create engine to run model
        worker = new(runtimeModel, BackendType.GPUCompute);
    }

    void Update()
    {
        if (!m_Started)
        {
            ProcessFrame();
        }
    }

    private async void ProcessFrame()
    {
        if (!cam.webcamTexture) return;

        m_Started = true;

        using Tensor<float> input = TextureConverter.ToTensor(cam.webcamTexture, imageWidth, imageHeight, 3);

        var outputs = await ForwardAsync(worker, input);

        input?.Dispose();

        Tensor<float> output = outputs[0] as Tensor<float>;
        Tensor<int> labelIDs = outputs[1] as Tensor<int>;

        var cpuOutput = await output.ReadbackAndCloneAsync();
        var cpuLabelIDs = await labelIDs.ReadbackAndCloneAsync();

        output?.Dispose();
        labelIDs.Dispose();

        float displayWidth = displayImage.rectTransform.rect.width;
        float displayHeight = displayImage.rectTransform.rect.height;

        float scaleX = displayWidth / imageWidth;
        float scaleY = displayHeight / imageHeight;

        int boxesFound = cpuOutput.shape[0];

        ClearAnnotations();

        //Draw the bounding boxes
        for (int n = 0; n < Mathf.Min(boxesFound, maxDetectionsInFrame); n++)
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

            UIManager.Instance.OnFoodSelected(label);
            DrawBox(box, n);
        }

        m_Started = false;
        cpuOutput?.Dispose();
        cpuLabelIDs?.Dispose();
    }

    private async Task<Tensor[]> ForwardAsync(Worker modelWorker, Tensor inputs)
    {
        var executor = worker.ScheduleIterable(inputs);
        var it = 0;
        bool hasMoreWork;
        do
        {
            hasMoreWork = executor.MoveNext();
            if (++it % 20 == 0)
            {
                await Task.Delay(32);
            }
        } while (hasMoreWork);

        var result1 = modelWorker.PeekOutput("output_0") as Tensor<float>;
        var result2 = modelWorker.PeekOutput("output_1") as Tensor<int>;

        Tensor[] results = {result1, result2};
        return results;
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