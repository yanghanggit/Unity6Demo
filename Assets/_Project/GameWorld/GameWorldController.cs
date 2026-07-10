using UnityEngine;

public class GameWorldController : MonoBehaviour
{
    /// <summary>
    /// 当场景中的某个可点击对象被点击时调用，sceneName 对应被点击对象的场景名称。
    /// </summary>
    /// <param name="sceneName">被点击对象的场景名称</param>
    public void OnClickScene(int sceneIndex)
    {
        Debug.Log("Scene clicked: " + sceneIndex);
    }
}
