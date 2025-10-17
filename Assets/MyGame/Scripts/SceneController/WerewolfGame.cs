using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class WerewolfGame : MonoBehaviour
{
    public RootAction _rootAction;

    public WerewolfGameStartAction _werewolfGameStartAction;

    public string serverUrl = "http://192.168.192.54:8000/";

    public Button _nextButton;

    void Start()
    {
        Debug.Assert(_rootAction != null, "_bootAction is null");
        Debug.Assert(_werewolfGameStartAction != null, "_werewolfGameStartAction is null");
        Debug.Assert(_nextButton != null, "_nextButton is null");

        _nextButton.gameObject.SetActive(false);
        StartCoroutine(LoadApiEndpoints());
    }

    public void OnClickNext()
    {
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadApiEndpoints()
    {
        //隐藏按钮
        _nextButton.gameObject.SetActive(false);

        //加载 Root API endpoints
        yield return _rootAction.Call(serverUrl);
        if (!_rootAction.LastRequestSuccess)
        {
            Debug.LogError("Failed to load Root API endpoints.");
            yield break;
        }

        // 设置游戏上下文，内含 URL 配置
        WerewolfGameContext.Instance.Root = _rootAction.RootResponse;

        //设置下
        _werewolfGameStartAction.Setup(
            WerewolfGameContext.Instance.StartUrl,
            WerewolfGameContext.Instance.PlayerName,
            WerewolfGameContext.Instance.GameName
        );

        // 发起开始游戏请求
        yield return _werewolfGameStartAction.Call();
        if (_werewolfGameStartAction.Response == null)
        {
            Debug.LogError("Failed to start Werewolf game.");
            yield break;
        }

        // 开界面，可以进行下一步
        _nextButton.gameObject.SetActive(true);
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.0f);
        //SceneManager.LoadScene(_nextScene);
    }

}
