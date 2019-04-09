using System.IO;

using UnityEngine;

public class TrainingController : AiBehaviour
{
    public void Initialize(int count)
    {
        string pathString;

        name = "Training Node " + count;

        // Coppy network from existing bot
        Inputs = NetworkTrainingScript.Instance._Behaviours[0].Inputs;
        HiddenLayers = NetworkTrainingScript.Instance._Behaviours[0].HiddenLayers;
        Outputs = NetworkTrainingScript.Instance._Behaviours[0].Outputs;

        // Set the filePath and init the network
        pathString = NetworkTrainingScript.Instance.GetTrainingNetwork() + ".nn";

        FilePath = Path.Combine(Application.streamingAssetsPath, pathString);
        base.Start();
    }
}