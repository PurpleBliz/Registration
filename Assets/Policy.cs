using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Policy : MonoBehaviour
{
    public void OnMouseDown() 
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Info.pdf");
        System.Diagnostics.Process.Start(filePath);
    }
}
