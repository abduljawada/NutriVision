using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.Rendering;

public class RunModel : MonoBehaviour
{
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private Camera QRCamera;

    private Worker worker;

    private const int imageWidth = 640;
    private const int imageHeight = 640;

    [SerializeField, Range(0, 1)] float iouThreshold = 0.5f;
    [SerializeField, Range(0, 1)] float scoreThreshold = 0.5f;


    const int k_LayersPerFrame = 20;
    IEnumerator m_Schedule;
    bool m_Started = false;
    private bool processFrame = false;

    Tensor<float> centersToCorners;

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
        FunctionalTensor labelIDs = Functional.Gather(classIDs, 0, indices);                  //shape=(N)
        Model runtimeModel = graph.Compile(labelIDs);

        //Create engine to run model
        worker = new(runtimeModel, BackendType.GPUCompute);
    }

    void Update()
    {
        if (!m_Started)
        {
            QRCamera.enabled = true;
            processFrame = true;
        }
    }

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

    private async void OnPostRender()
    {
        if (!processFrame) return;

        processFrame = false;
        m_Started = true;

        Debug.Log("Processing Frame");

        //Create a new texture with the width and height of the screen
        Texture2D QRTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
        QRTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
        QRTexture.Apply();

        Debug.Log("Texture Applied");

        using Tensor<float> input = TextureConverter.ToTensor(QRTexture, imageWidth, imageHeight, 3);

        Debug.Log("Texture Converted");

        QRCamera.enabled = false;

        var output = await ForwardAsync(worker, input);

        Debug.Log("Forwarded");

        input?.Dispose();

        var cpuOutput = output.ReadbackAndClone();

        Debug.Log("Readback");

        output?.Dispose();

        int boxesFound = cpuOutput.shape[0];

        Debug.Log(boxesFound);

        if (boxesFound > 0)
        {
            UIManager.Instance.OnFoodSelected(cpuOutput[0]);
            Debug.Log("Food Selected" + cpuOutput[0]);
        }

        m_Started = false;
        cpuOutput?.Dispose();
    }

    private async Task<Tensor<int>> ForwardAsync(Worker modelWorker, Tensor inputs)
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

        var result = modelWorker.PeekOutput() as Tensor<int>;

        return result;
    }

    private void OnDestroy()
    {
        centersToCorners?.Dispose();
        worker?.Dispose();
    }
}