using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class BootScene : MonoBehaviour
{
    public string _baseUrl = "http://192.168.2.121:8000/";

    public string _nextScene = "LoginScene";

    public RootAction _rootAction;

    public GameConfig _gameConfig;

    public Button _nextButton;

    void Start()
    {
        //GameContext.Instance.SetupGame = true;

        Debug.Assert(_rootAction != null, "_bootAction is null");
        Debug.Assert(_gameConfig != null, "_gameConfig is null");
        Debug.Assert(_nextButton != null, "_nextButton is null");

        _nextButton.gameObject.SetActive(false);
        StartCoroutine(InitializeApiEndpoints());
    }

    public void OnClickNextSceneLogin()
    {
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator InitializeApiEndpoints()
    {
        yield return _rootAction.Call(_baseUrl);
        if (_rootAction.ResponseData != null)
        {
            _nextButton.gameObject.SetActive(true);
            GameContext.Instance.Root = _rootAction.ResponseData;
            Debug.Log("Using LocalNet for API endpoints");
            yield break;
        }

        // yield return _rootAction.Call(_gameConfig.LocalHost);
        // if (_rootAction.ResponseData != null)
        // {
        //     _nextButton.gameObject.SetActive(true);
        //     GameContext.Instance.Root = _rootAction.ResponseData;
        //     Debug.Log("Using LocalHost for API endpoints");
        //     yield break;
        // }
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.0f);
        SceneManager.LoadScene(_nextScene);
    }
}
