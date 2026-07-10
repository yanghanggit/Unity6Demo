using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class GameWorldController : MonoBehaviour
{
    private const string HomeSceneName = "HomeScene";

    private const string MockNextSceneName = "TestLanding";

    /// <summary>
    /// 当场景中的某个可点击对象被点击时调用，sceneName 对应被点击对象的场景名称。
    /// </summary>
    /// <param name="sceneName">被点击对象的场景名称</param>
    public void OnClickScene(int sceneIndex)
    {
        Debug.Log("Scene clicked: " + sceneIndex);

        if (GameManager.Instance.IsServerConnected)
        {
            EnterHomeSceneAsync().Forget(); // 正常流程：启动新游戏
        }
        else
        {
            MockEnterNextSceneAsync().Forget(); // 模拟新游戏创建成功（用于测试）
        }
    }

    /// <summary>
    /// 进入主场景（HomeScene）的异步方法。
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid EnterHomeSceneAsync()
    {
        //_button.SetEnabled(false);
        await SceneManager.LoadSceneAsync(HomeSceneName);
    }

    /// <summary>
    /// 模拟进入下一个场景（TestLanding）的异步方法。
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid MockEnterNextSceneAsync()
    {
        // 模拟新游戏创建成功
        await UniTask.Delay(0);
        // 跳转到下一个场景
        await SceneManager.LoadSceneAsync(MockNextSceneName);
    }
}
