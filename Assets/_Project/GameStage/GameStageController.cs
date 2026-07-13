using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class GameStageController : MonoBehaviour
{

    void Start()
    {
        Debug.Log("GameStageController started");
    }

    public void OnClick(int index)
    {
        Debug.Log("Actor clicked: " + index);

        if (GameManager.Instance.IsServerConnected)
        {
            Debug.Log("Server is connected, would enter home scene.");
        }
        else
        {
            Debug.Log("Server is not connected, would mock enter next scene.");
        }
    }

}
