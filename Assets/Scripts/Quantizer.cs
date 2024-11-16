using Unity.Sentis;
using UnityEngine;
using System.IO;

public class Quantizer : MonoBehaviour
{
    [SerializeField] ModelAsset modelAsset;
    private void Start()
    {
        Model model = ModelLoader.Load(modelAsset);
        QuantizeAndSerializeModel(model, Path.Combine(Application.streamingAssetsPath, "quantized.onnx"));
    }
    void QuantizeAndSerializeModel(Model model, string path)
    {
        // Sentis destructively edits the source model in memory when quantizing.
        ModelQuantizer.QuantizeWeights(QuantizationType.Float16, ref model);

        // Serialize the quantized model to a file.
        ModelWriter.Save(path, model);
        Debug.Log("Done");
    }

}
