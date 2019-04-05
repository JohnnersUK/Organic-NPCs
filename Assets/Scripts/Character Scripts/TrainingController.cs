using System.IO;

using UnityEngine;

public class TrainingController : AiBehaviour
{
    public override void Start()
    {
        name = "Training Node ";

        Inputs = NetworkTrainingScript.instance._Behaviours[0].Inputs;
        HiddenLayers = NetworkTrainingScript.instance._Behaviours[0].HiddenLayers;
        Outputs = NetworkTrainingScript.instance._Behaviours[0].Outputs;

        // Set the filePath and init the network
        FilePath = Path.Combine(Application.streamingAssetsPath, "Default.nn");
        base.Start();
    }
}