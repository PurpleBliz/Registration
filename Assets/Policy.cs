using UnityEngine;

public class Policy : MonoBehaviour
{
    public GameObject UIPanel;

    public void Open()
    {
        UIPanel.SetActive(true);
    }

    public void Close()
    {
        UIPanel.SetActive(false);
    }
    
}
