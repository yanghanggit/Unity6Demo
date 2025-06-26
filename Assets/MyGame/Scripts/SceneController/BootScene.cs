using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class BootScene : MonoBehaviour
{
    public string _nextScene = "LoginScene";

    public GetURLConfigurationAction _getUrlConfigurationAction;

    public GameConfig _gameConfig;

    public Button _nextButton;

    void Start()
    {
        Debug.Assert(_getUrlConfigurationAction != null, "_bootAction is null");
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
        yield return _getUrlConfigurationAction.Call(_gameConfig.LocalNet);
        if (_getUrlConfigurationAction.LastRequestSuccess)
        {
            _nextButton.gameObject.SetActive(true);
            yield break;
        }

        yield return _getUrlConfigurationAction.Call(_gameConfig.LocalHost);
        if (_getUrlConfigurationAction.LastRequestSuccess)
        {
            _nextButton.gameObject.SetActive(true);
            yield break;
        }
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.0f);
        SceneManager.LoadScene(_nextScene);
    }

}
